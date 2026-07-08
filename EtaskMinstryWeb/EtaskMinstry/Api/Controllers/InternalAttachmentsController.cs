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

        // ──────────────────────────── helpers ────────────────────────────

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
