using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Dtos.Internal;
using EtaskMinstry.AppCode;
using TaskManagementModel;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Ops Portal server-to-server attachment upload.
    /// URL: POST /api/internal/tasks/{taskId}/attachments
    ///
    /// This is NOT part of the Mobile API (/api/v1). It is authenticated by a
    /// shared secret in the X-Ops-Portal-Key header (Web.config: OpsPortalKey),
    /// NOT by a JWT — so it deliberately does NOT carry [JwtAuthorize] and never
    /// sets MvcApplication.userData.
    ///
    /// It writes the physical file into Telesak's own /Upload/Task/ folder FIRST,
    /// then inserts the dbo.Attachment row via the existing
    /// TaskManger.AttachTaskFile (which also logs and notifies employee + company,
    /// fanning out to FCM). Ordering guarantees a returned attachmentId always has
    /// a file on disk — Telesak hides attachment rows whose file is missing.
    ///
    /// See Telesak-Docs/5-OPS-PORTAL-INTERNAL-API.md for the full contract.
    /// </summary>
    public class InternalAttachmentsController : ApiController
    {
        private const string ApiKeyHeader = "X-Ops-Portal-Key";
        private const int MaxDescriptionLength = 350;     // dbo.Attachment.Description nvarchar(350)
        private const int MaxOriginalNameLength = 255;    // dbo.Attachment.OriginalFileName nvarchar(255)

        /// <summary>Fallback media type when the extension maps to nothing useful.</summary>
        private const string DefaultContentType = "application/octet-stream";

        /// <summary>
        /// Narrows the dbo.TaskLog scan to attachment rows. LogTask.LogTaskSingleValue
        /// writes Value as [{'columnName':'AttachmentID','oldVal':'','newVal':'&lt;stored name&gt;'}],
        /// and comments use the same shape with 'TaskCommentID', so the column name has
        /// to be part of the match.
        /// </summary>
        private const string AttachmentLogColumnMarker = "'columnName':'AttachmentID'";

        /// <summary>Prefix of the Value fragment carrying the stored file name.</summary>
        private const string AttachmentLogValuePrefix = "'newVal':'";

        /// <summary>Page size used when the caller does not ask for one.</summary>
        private const int DefaultPageSize = 100;

        /// <summary>
        /// Hard ceiling on page size. Also bounds the IN clause of the per-page log
        /// lookup, keeping it well inside SQL Server's ~2100 parameter limit. Matches
        /// the value in the Ops Portal team's own example request.
        /// </summary>
        private const int MaxPageSize = 500;

        /// <summary>Ceiling on how many ids the empIds filter accepts, for the same reason.</summary>
        private const int MaxEmpIdsFilter = 500;

        /// <summary>The one date format the range filters accept, and the one taskStartDate is returned in.</summary>
        private const string FilterDateFormat = "yyyy-MM-dd";

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> Upload(int taskId)
        {
            // 1) Shared-secret gate (fail closed if the key isn't configured).
            var keyError = ValidateApiKey();
            if (keyError != null) return keyError;

            // 2) Must be multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return Fail(HttpStatusCode.BadRequest, "بيانات الطلب غير صحيحة", "INVALID_REQUEST");

            // 3) Parse the parts: file (required), description + originalFileName (optional).
            var provider = await Request.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider());
            string description = null;
            string providedOriginalName = null;
            HttpContent fileContent = null;

            foreach (var part in provider.Contents)
            {
                var disposition = part.Headers.ContentDisposition;
                string name = TrimQuotes(disposition != null ? disposition.Name : null);

                if (string.Equals(name, "description", StringComparison.OrdinalIgnoreCase))
                {
                    description = (await part.ReadAsStringAsync()).Trim();
                }
                else if (string.Equals(name, "originalFileName", StringComparison.OrdinalIgnoreCase))
                {
                    providedOriginalName = (await part.ReadAsStringAsync()).Trim();
                }
                else if (string.Equals(name, "file", StringComparison.OrdinalIgnoreCase)
                         && disposition != null && !string.IsNullOrEmpty(disposition.FileName))
                {
                    fileContent = part;
                }
            }

            // 4) File must be present and non-empty.
            if (fileContent == null)
                return Fail(HttpStatusCode.BadRequest, "يجب اختيار ملف", "MISSING_FILE");

            byte[] bytes = await fileContent.ReadAsByteArrayAsync();
            if (bytes == null || bytes.Length == 0)
                return Fail(HttpStatusCode.BadRequest, "يجب اختيار ملف", "MISSING_FILE");
            // (Oversized files are rejected earlier by ASP.NET/IIS maxRequestLength —
            //  no app-level size check by design; see the plan doc §0.)

            // 5) Task must exist, not be deleted, and not be Done/Approved.
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var task = uow.TaskRepository.GetByID(taskId);
            if (task == null || task.IsDeleted)
                return Fail(HttpStatusCode.NotFound, "لم يتم العثور على المهمة", "TASK_NOT_FOUND");
            if (task.StatusID == (int)TaskStatus.Done || task.StatusID == (int)TaskStatus.Approved)
                return Fail(HttpStatusCode.Conflict, "لا يمكن إضافة مرفق لمهمة منتهية أو معتمدة", "INVALID_STATE");

            // 6) Resolve original file name (explicit field wins, else the file part's
            //    own filename), sanitize, and cap to the column limit.
            string rawOriginal = !string.IsNullOrWhiteSpace(providedOriginalName)
                ? providedOriginalName
                : TrimQuotes(fileContent.Headers.ContentDisposition.FileName);
            string originalFileName = Extentions.SanitizeFileName(rawOriginal);
            originalFileName = Cap(originalFileName, MaxOriginalNameLength);
            description = Cap(description, MaxDescriptionLength);

            // 7) Generate stored name (GUID + original extension, no path) and write
            //    the physical bytes FIRST, so a saved row always has its file.
            string extension = Path.GetExtension(originalFileName ?? string.Empty);
            string storedFileName = Guid.NewGuid().ToString("N") + extension;
            try
            {
                string uploadRoot = HostingEnvironment.MapPath("~/Upload/Task/");
                if (!Directory.Exists(uploadRoot)) Directory.CreateDirectory(uploadRoot);
                File.WriteAllBytes(Path.Combine(uploadRoot, storedFileName), bytes);
            }
            catch
            {
                return Fail(HttpStatusCode.InternalServerError, "تعذر رفع الملف", "SAVE_FAILED");
            }

            // 8) Insert the row through existing AppCode (also logs + notifies
            //    employee/company → FCM). No new business logic here.
            bool ok;
            try
            {
                ok = TaskManger.AttachTaskFile(taskId, storedFileName, description, originalFileName);
            }
            catch
            {
                ok = false;
            }
            if (!ok)
                return Fail(HttpStatusCode.InternalServerError, "تعذر رفع الملف", "SAVE_FAILED");

            // 9) Read back the identity id to echo it.
            var saved = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString)
                .AttachmentRepository.Get(
                    filter: a => a.TaskID == taskId && a.FileName == storedFileName,
                    orderBy: q => q.OrderByDescending(a => a.AttachmentID))
                .FirstOrDefault();

            return Request.CreateResponse(HttpStatusCode.Created, ApiResponse.Ok(new OpsAttachmentResultDto
            {
                AttachmentId = saved != null ? saved.AttachmentID : 0,
                TaskId = taskId,
                FileName = storedFileName,
                OriginalFileName = originalFileName,
                Description = description
            }, "تم رفع الملف"));
        }

        /// <summary>
        /// Ops Portal server-to-server attachment list.
        /// URL: GET /api/internal/tasks/{taskId}/attachments
        ///
        /// Read-only counterpart to <see cref="Upload"/>. Same shared-secret gate,
        /// same ApiResponse envelope. Returns every dbo.Attachment row of the task,
        /// including rows whose physical file has gone missing — those carry
        /// fileExists = false and sizeBytes = 0 so an exporter can skip them
        /// instead of writing an empty file into a customer's ZIP.
        ///
        /// Upload date and uploader side come from the dbo.TaskLog row that
        /// LogTask.LogAddAttachment writes next to each insert; dbo.Attachment
        /// itself records neither. See OpsAttachmentListItemDto for what that can
        /// and cannot tell the caller.
        /// </summary>
        [HttpGet]
        public HttpResponseMessage List(int taskId)
        {
            // 1) Shared-secret gate (fail closed if the key isn't configured).
            var keyError = ValidateApiKey();
            if (keyError != null) return keyError;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            // 2) Same task visibility rule as the upload: unknown and soft-deleted
            //    tasks are indistinguishable to the caller.
            var task = uow.TaskRepository.GetByID(taskId);
            if (task == null || task.IsDeleted)
                return Fail(HttpStatusCode.NotFound, "لم يتم العثور على المهمة", "TASK_NOT_FOUND");

            var attachments = uow.AttachmentRepository
                .Get(filter: a => a.TaskID == taskId,
                     orderBy: q => q.OrderBy(a => a.AttachmentID))
                .ToList();

            // 3) One query for the task's attachment log rows, matched in memory —
            //    a task has a bounded number of logs, so this beats a query per file.
            var uploadLogs = uow.TaskLogRepository
                .Get(filter: l => l.TaskID == taskId && l.Value.Contains(AttachmentLogColumnMarker),
                     orderBy: q => q.OrderBy(l => l.TaskLogID))
                .ToList();

            var items = attachments.Select(a => ToListItem(a, taskId, uploadLogs)).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(items));
        }

        /// <summary>
        /// Ops Portal server-to-server attachment download.
        /// URL: GET /api/internal/attachments/{attachmentId}/content
        ///
        /// Returns the raw bytes — this is the one thing the Ops Portal cannot do
        /// any other way, since /Upload/Task/ is Telesak's own web root and ADR-048
        /// ruled out letting another app read it directly.
        ///
        /// Streams straight from disk rather than buffering: an export run is a few
        /// hundred of these back to back, and there is no reason to hold each file
        /// in memory. Web API disposes the response content, and with it the file
        /// handle, once the body has been written.
        ///
        /// Every failure carries a real status code and a JSON body — never a 200
        /// wrapping an HTML page, which would otherwise be saved into a customer's
        /// ZIP as if it were their document.
        ///
        /// The action is named Download rather than Content so it cannot collide
        /// with ApiController's own protected Content&lt;T&gt; helpers during action
        /// selection; the URL the Ops Portal calls is still /content.
        /// </summary>
        [HttpGet]
        public HttpResponseMessage Download(int attachmentId)
        {
            // 1) Shared-secret gate (fail closed if the key isn't configured).
            var keyError = ValidateApiKey();
            if (keyError != null) return keyError;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            var attachment = uow.AttachmentRepository.GetByID(attachmentId);
            if (attachment == null)
                return Fail(HttpStatusCode.NotFound, "لم يتم العثور على المرفق", "ATTACHMENT_NOT_FOUND");

            // 2) An attachment hanging off an unknown or soft-deleted task does not
            //    exist as far as this API is concerned — same rule the list endpoint
            //    applies, so a file cannot be pulled out of a deleted task by id.
            var task = attachment.TaskID.HasValue
                ? uow.TaskRepository.GetByID(attachment.TaskID.Value)
                : null;
            if (task == null || task.IsDeleted)
                return Fail(HttpStatusCode.NotFound, "لم يتم العثور على المرفق", "ATTACHMENT_NOT_FOUND");

            // 3) Row exists but the bytes are gone → 410, distinct from 404, so the
            //    caller can tell "no such attachment" from "we lost the file".
            string storedName = SafeStoredName(attachment.FileName);
            string path = storedName == null
                ? null
                : HostingEnvironment.MapPath("~/Upload/Task/" + storedName);

            if (path == null || !File.Exists(path))
                return Fail(HttpStatusCode.Gone, "الملف غير موجود على الخادم", "FILE_MISSING");

            // 4) FileShare.Read so concurrent downloads — and the web UI serving the
            //    same file — do not lock each other out.
            FileStream stream;
            long length;
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                length = stream.Length;
            }
            catch (FileNotFoundException)
            {
                // Deleted between the File.Exists check above and the open.
                return Fail(HttpStatusCode.Gone, "الملف غير موجود على الخادم", "FILE_MISSING");
            }
            catch (DirectoryNotFoundException)
            {
                return Fail(HttpStatusCode.Gone, "الملف غير موجود على الخادم", "FILE_MISSING");
            }
            catch
            {
                return Fail(HttpStatusCode.InternalServerError, "تعذر قراءة الملف", "READ_FAILED");
            }

            string displayName = !string.IsNullOrWhiteSpace(attachment.OriginalFileName)
                ? attachment.OriginalFileName
                : storedName;

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = BuildMediaType(GuessContentType(displayName));
            response.Content.Headers.ContentLength = length;
            response.Content.Headers.ContentDisposition = BuildContentDisposition(displayName, storedName);
            return response;
        }

        /// <summary>
        /// Ops Portal server-to-server attachment list for a whole company and date
        /// range.
        /// URL: GET /api/internal/companies/{companyId}/attachments
        ///        ?fromDate=2026-08-01&amp;toDate=2026-08-31&amp;empIds=101,102&amp;page=1&amp;pageSize=500
        ///
        /// Exists to collapse an export run from one request per task — several
        /// hundred for a month of a 40-employee company — into a handful. Every item
        /// carries the same shape the per-task list returns, plus the employee and
        /// task context needed to build a folder-per-employee ZIP without going back
        /// for each task.
        ///
        /// All query parameters are optional. Omitting the dates returns the
        /// company's whole history; omitting empIds covers every employee.
        /// </summary>
        [HttpGet]
        public HttpResponseMessage ByCompany(int companyId, string fromDate = null, string toDate = null,
                                             string empIds = null, int page = 1, int pageSize = DefaultPageSize)
        {
            // 1) Shared-secret gate (fail closed if the key isn't configured).
            var keyError = ValidateApiKey();
            if (keyError != null) return keyError;

            // 2) Dates are parsed exact-and-invariant, never with the ambient culture.
            //    A server whose OS locale is ar-SA defaults to the UmAlQura calendar,
            //    where DateTime.Parse("2026-08-01") yields a Gregorian date six
            //    centuries out — silently, and only on that machine.
            DateTime? from, to;
            if (!TryParseFilterDate(fromDate, out from))
                return Fail(HttpStatusCode.BadRequest, "صيغة التاريخ غير صحيحة", "INVALID_DATE");
            if (!TryParseFilterDate(toDate, out to))
                return Fail(HttpStatusCode.BadRequest, "صيغة التاريخ غير صحيحة", "INVALID_DATE");
            if (from.HasValue && to.HasValue && to.Value < from.Value)
                return Fail(HttpStatusCode.BadRequest, "نطاق التاريخ غير صحيح", "INVALID_DATE_RANGE");

            List<int> employeeIds;
            if (!TryParseEmpIds(empIds, out employeeIds))
                return Fail(HttpStatusCode.BadRequest, "قائمة الموظفين غير صحيحة", "INVALID_EMP_IDS");

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = DefaultPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            var company = uow.Company.GetByID(companyId);
            if (company == null || company.IsDeleted == true)
                return Fail(HttpStatusCode.NotFound, "لم يتم العثور على الشركة", "COMPANY_NOT_FOUND");

            // 3) Build the query in steps so each filter is only added when it
            //    applies — clearer SQL than one expression full of constant guards.
            var query = uow.AttachmentRepository
                .Get(filter: a => a.Task != null && a.Task.CompanyID == companyId && !a.Task.IsDeleted,
                     includeProperties: "Task,Task.Employee");

            if (from.HasValue)
            {
                DateTime fromValue = from.Value;
                query = query.Where(a => a.Task.StartDate.HasValue && a.Task.StartDate.Value >= fromValue);
            }

            if (to.HasValue)
            {
                // toDate is inclusive of the whole day, so compare against the start
                // of the next one — Task.StartDate can carry a time component.
                DateTime toExclusive = to.Value.AddDays(1);
                query = query.Where(a => a.Task.StartDate.HasValue && a.Task.StartDate.Value < toExclusive);
            }

            if (employeeIds.Count > 0)
                query = query.Where(a => a.Task.EmpID.HasValue && employeeIds.Contains(a.Task.EmpID.Value));

            int totalCount = query.Count();

            // 4) Order by AttachmentID: unique and monotonic, so paging is stable
            //    even if rows are added between the caller's pages.
            var rows = query
                .OrderBy(a => a.AttachmentID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5) One log query for the whole page rather than one per task. Page size
            //    is capped at MaxPageSize partly to keep this IN clause well inside
            //    SQL Server's parameter limit.
            var taskIds = rows.Where(a => a.TaskID.HasValue).Select(a => a.TaskID.Value).Distinct().ToList();
            var uploadLogs = taskIds.Count == 0
                ? new List<TaskLog>()
                : uow.TaskLogRepository
                     .Get(filter: l => l.TaskID.HasValue && taskIds.Contains(l.TaskID.Value)
                                       && l.Value.Contains(AttachmentLogColumnMarker),
                          orderBy: q => q.OrderBy(l => l.TaskLogID))
                     .ToList();

            var logsByTask = uploadLogs
                .GroupBy(l => l.TaskID.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var items = rows.Select(a => ToCompanyItem(a, logsByTask)).ToList();

            return Request.CreateResponse(HttpStatusCode.OK,
                PagedResponse.Build(items, page, pageSize, totalCount));
        }

        // ──────────────────────────── helpers ────────────────────────────

        /// <summary>
        /// Projects one dbo.Attachment row into the wire shape, resolving physical
        /// file facts from disk and upload facts from the task's log rows.
        /// </summary>
        private OpsAttachmentListItemDto ToListItem(Attachment a, int taskId, List<TaskLog> uploadLogs)
        {
            string storedName = SafeStoredName(a.FileName);
            string path = storedName == null
                ? null
                : HostingEnvironment.MapPath("~/Upload/Task/" + storedName);

            bool exists = false;
            long size = 0L;
            if (path != null)
            {
                try
                {
                    var info = new FileInfo(path);
                    exists = info.Exists;
                    if (exists) size = info.Length;
                }
                catch
                {
                    // Unreadable path (permissions, name the OS rejects) reads as
                    // "no file" — same outcome the caller needs either way.
                    exists = false;
                    size = 0L;
                }
            }

            var item = new OpsAttachmentListItemDto
            {
                AttachmentId = a.AttachmentID,
                TaskId = taskId,
                FileName = a.FileName,
                OriginalFileName = a.OriginalFileName,
                Description = a.Description,
                FileExists = exists,
                SizeBytes = size,
                ContentType = GuessContentType(
                    !string.IsNullOrWhiteSpace(a.OriginalFileName) ? a.OriginalFileName : a.FileName)
            };

            var log = FindUploadLog(uploadLogs, a.FileName);
            if (log != null)
            {
                item.UploadedAtUtc = ToUtc(log.LogDate);
                item.UploadedBySource = log.IsFromCompany ? "company" : "employee";
                item.UploadedByUserTypeId = log.IsFromCompany
                    ? (int)LoggedUserType.Company
                    : (int)LoggedUserType.Employee;
            }

            // UploadedByUserAccountId / UploadedByName stay null: no uploader
            // identity is recorded anywhere in the schema.
            return item;
        }

        /// <summary>
        /// The earliest log row whose Value carries this stored file name. Null when
        /// the attachment predates the log feature or was inserted outside
        /// LogTask.LogAddAttachment.
        /// </summary>
        private static TaskLog FindUploadLog(List<TaskLog> uploadLogs, string storedFileName)
        {
            if (uploadLogs == null || string.IsNullOrEmpty(storedFileName)) return null;

            string marker = AttachmentLogValuePrefix + storedFileName + "'";
            return uploadLogs.FirstOrDefault(
                l => l.Value != null && l.Value.IndexOf(marker, StringComparison.Ordinal) >= 0);
        }

        /// <summary>
        /// The bare file name of a stored attachment, with any directory component
        /// stripped. Defence in depth: nothing should ever have written a path into
        /// dbo.Attachment.FileName, but this endpoint turns that column straight into
        /// a filesystem read, so a "..\..\web.config" value must not escape
        /// /Upload/Task/. Returns null for anything unusable.
        /// </summary>
        private static string SafeStoredName(string storedFileName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName)) return null;
            try
            {
                string name = Path.GetFileName(storedFileName.Trim());
                return string.IsNullOrEmpty(name) ? null : name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Media type guessed from the extension; never null.</summary>
        private static string GuessContentType(string fileName)
        {
            try
            {
                string mapped = System.Web.MimeMapping.GetMimeMapping(fileName ?? string.Empty);
                return string.IsNullOrWhiteSpace(mapped) ? DefaultContentType : mapped;
            }
            catch
            {
                return DefaultContentType;
            }
        }

        /// <summary>
        /// Converts a dbo.TaskLog.LogDate to UTC. LogTask writes DateTime.Now (server
        /// local time) and SQL Server hands it back with Kind = Unspecified, which the
        /// JSON formatter's DateTimeZoneHandling.Utc would otherwise stamp as UTC
        /// without shifting it — publishing Riyadh time labelled "Z". Stamping Local
        /// first makes the conversion real.
        /// </summary>
        private static DateTime ToUtc(DateTime serverLocal)
        {
            return DateTime.SpecifyKind(serverLocal, DateTimeKind.Local).ToUniversalTime();
        }

        /// <summary>
        /// Projects one dbo.Attachment row, with its Task (and Employee) loaded, into
        /// the company-range wire shape.
        /// </summary>
        private OpsCompanyAttachmentItemDto ToCompanyItem(Attachment a, Dictionary<int, List<TaskLog>> logsByTask)
        {
            int taskId = a.TaskID.HasValue ? a.TaskID.Value : 0;

            List<TaskLog> taskLogs;
            if (!logsByTask.TryGetValue(taskId, out taskLogs)) taskLogs = null;

            // Build the shared part once, then copy across. The base projection owns
            // the disk lookup and the log matching; this method only adds context.
            var baseItem = ToListItem(a, taskId, taskLogs);

            var task = a.Task;
            var employee = task != null ? task.Employee : null;

            return new OpsCompanyAttachmentItemDto
            {
                AttachmentId = baseItem.AttachmentId,
                TaskId = baseItem.TaskId,
                FileName = baseItem.FileName,
                OriginalFileName = baseItem.OriginalFileName,
                Description = baseItem.Description,
                SizeBytes = baseItem.SizeBytes,
                ContentType = baseItem.ContentType,
                FileExists = baseItem.FileExists,
                UploadedAtUtc = baseItem.UploadedAtUtc,
                UploadedBySource = baseItem.UploadedBySource,
                UploadedByUserTypeId = baseItem.UploadedByUserTypeId,
                UploadedByUserAccountId = baseItem.UploadedByUserAccountId,
                UploadedByName = baseItem.UploadedByName,

                EmpId = task != null ? task.EmpID : null,
                EmpName = employee != null ? employee.Name : null,
                TaskTitle = task != null ? task.Title : null,
                TaskStartDate = (task != null && task.StartDate.HasValue)
                    ? task.StartDate.Value.ToString(FilterDateFormat, CultureInfo.InvariantCulture)
                    : null
            };
        }

        /// <summary>
        /// Parses a yyyy-MM-dd filter value. Returns false only for a value that is
        /// present but unparseable; a null or blank value is a valid "no filter" and
        /// yields true with a null result.
        ///
        /// Exact-and-invariant on purpose. DateTime.Parse would follow the ambient
        /// culture, and on a server whose OS locale is ar-SA that means the UmAlQura
        /// calendar — "2026-08-01" would parse to a Gregorian date six centuries
        /// later, on that machine only, with no error.
        /// </summary>
        private static bool TryParseFilterDate(string value, out DateTime? parsed)
        {
            parsed = null;
            if (string.IsNullOrWhiteSpace(value)) return true;

            DateTime result;
            if (!DateTime.TryParseExact(value.Trim(), FilterDateFormat, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out result))
                return false;

            parsed = result.Date;
            return true;
        }

        /// <summary>
        /// Parses the comma-separated empIds filter. Blank entries are skipped, so
        /// "101,,102," is accepted. Returns false for a non-numeric entry or for more
        /// than <see cref="MaxEmpIdsFilter"/> ids — silently dropping either would
        /// return a quietly wrong subset of an export.
        /// </summary>
        private static bool TryParseEmpIds(string value, out List<int> ids)
        {
            ids = new List<int>();
            if (string.IsNullOrWhiteSpace(value)) return true;

            foreach (string part in value.Split(','))
            {
                string trimmed = part.Trim();
                if (trimmed.Length == 0) continue;

                int id;
                if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                    return false;

                if (!ids.Contains(id)) ids.Add(id);
            }

            return ids.Count <= MaxEmpIdsFilter;
        }

        /// <summary>
        /// Content-Type header value, falling back to octet-stream if the mapped
        /// string is somehow not a legal media type.
        /// </summary>
        private static MediaTypeHeaderValue BuildMediaType(string contentType)
        {
            try
            {
                return new MediaTypeHeaderValue(contentType);
            }
            catch
            {
                return new MediaTypeHeaderValue(DefaultContentType);
            }
        }

        /// <summary>
        /// Content-Disposition for a download, carrying the Arabic display name.
        ///
        /// <c>FileNameStar</c> emits the RFC 5987 form the Ops Portal asked for —
        /// note .NET writes the charset token lowercase (<c>filename*=utf-8''…</c>),
        /// which is case-insensitive per the RFC and accepted everywhere.
        ///
        /// A plain ASCII <c>filename=</c> is set alongside it. RFC 6266 clients
        /// prefer <c>filename*</c> when both are present, so this changes nothing
        /// for a correct client; it just means a client that ignores the extended
        /// form saves the GUID stored name instead of mojibake.
        /// </summary>
        private static ContentDispositionHeaderValue BuildContentDisposition(string displayName, string storedName)
        {
            var disposition = new ContentDispositionHeaderValue("attachment");

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                try { disposition.FileNameStar = displayName; }
                catch { /* unencodable name — the ASCII fallback below still applies */ }
            }

            string fallback = AsciiFallbackName(storedName);
            if (fallback != null)
            {
                try { disposition.FileName = fallback; }
                catch { }
            }

            return disposition;
        }

        /// <summary>
        /// An ASCII-only, header-safe version of a file name for the plain
        /// <c>filename=</c> parameter. Anything outside [A-Za-z0-9._-] becomes an
        /// underscore, which also rules out header injection via a stored name
        /// containing quotes or CR/LF. Returns null when nothing usable is left.
        /// </summary>
        private static string AsciiFallbackName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                bool safe = (c >= 'a' && c <= 'z')
                         || (c >= 'A' && c <= 'Z')
                         || (c >= '0' && c <= '9')
                         || c == '.' || c == '_' || c == '-';
                sb.Append(safe ? c : '_');
            }

            string result = sb.ToString().Trim('_', '.');
            return result.Length == 0 ? null : result;
        }

        /// <summary>
        /// Returns null when the X-Ops-Portal-Key header matches the configured
        /// OpsPortalKey; otherwise the error response to short-circuit with.
        /// Fails closed (503) when the key is unset / still a REPLACE_ placeholder.
        /// </summary>
        private HttpResponseMessage ValidateApiKey()
        {
            string configured = ConfigurationManager.AppSettings["OpsPortalKey"];
            if (string.IsNullOrWhiteSpace(configured)
                || configured.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase))
            {
                return Fail(HttpStatusCode.ServiceUnavailable, "الخدمة غير مفعّلة", "OPS_API_DISABLED");
            }

            string provided = null;
            IEnumerable<string> values;
            if (Request.Headers.TryGetValues(ApiKeyHeader, out values) && values != null)
                provided = values.FirstOrDefault();

            if (string.IsNullOrEmpty(provided) || !FixedTimeEquals(provided, configured))
                return Fail(HttpStatusCode.Unauthorized, "غير مصرح", "INVALID_API_KEY");

            return null;
        }

        /// <summary>Length-and-content comparison with no early-out, to avoid timing side channels.</summary>
        private static bool FixedTimeEquals(string a, string b)
        {
            byte[] ba = Encoding.UTF8.GetBytes(a ?? string.Empty);
            byte[] bb = Encoding.UTF8.GetBytes(b ?? string.Empty);
            int diff = ba.Length ^ bb.Length;
            for (int i = 0; i < ba.Length && i < bb.Length; i++)
                diff |= ba[i] ^ bb[i];
            return diff == 0;
        }

        private static string Cap(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length > max ? value.Substring(0, max) : value;
        }

        private static string TrimQuotes(string value)
        {
            return string.IsNullOrEmpty(value) ? value : value.Trim().Trim('"');
        }

        private HttpResponseMessage Fail(HttpStatusCode status, string message, string code)
        {
            return Request.CreateResponse(status, ApiResponse.Fail(message, code));
        }
    }
}
