using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
