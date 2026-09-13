using System;

namespace EtaskMinstry.Api.Dtos.Internal
{
    /// <summary>
    /// One item of the response <c>data</c> array for
    /// GET /api/internal/tasks/{taskId}/attachments (Ops Portal server-to-server
    /// read). Serialised camelCase by the API formatter.
    ///
    /// Note the formatter sets <c>NullValueHandling.Ignore</c>, so any property
    /// left null is omitted from the JSON entirely rather than emitted as
    /// <c>null</c>. Callers must treat "key absent" as "unknown".
    /// </summary>
    public class OpsAttachmentListItemDto
    {
        public int AttachmentId { get; set; }
        public int TaskId { get; set; }

        /// <summary>Stored name on disk under /Upload/Task/ (GUID + extension), no path.</summary>
        public string FileName { get; set; }

        /// <summary>Original display name as uploaded (Attachment.OriginalFileName).</summary>
        public string OriginalFileName { get; set; }

        public string Description { get; set; }

        /// <summary>Physical size on disk. 0 when the file is missing — check <see cref="FileExists"/>.</summary>
        public long SizeBytes { get; set; }

        /// <summary>Guessed from the extension. "application/octet-stream" when unrecognised.</summary>
        public string ContentType { get; set; }

        /// <summary>
        /// False when the dbo.Attachment row exists but its file is gone from
        /// /Upload/Task/. Such a row is hidden by the Telesak web UI, and
        /// GET /api/internal/attachments/{id}/content returns 410 for it.
        /// Callers building an export should skip these rather than write an
        /// empty file.
        /// </summary>
        public bool FileExists { get; set; }

        /// <summary>
        /// When the attachment was added, in UTC. Derived from the dbo.TaskLog row
        /// written alongside the insert — dbo.Attachment itself has no date column.
        /// Null when no matching log row exists (rows predating the log feature, or
        /// inserted outside LogTask.LogAddAttachment).
        /// </summary>
        public DateTime? UploadedAtUtc { get; set; }

        /// <summary>
        /// "employee" or "company", from TaskLog.IsFromCompany. Null when unknown.
        /// See <see cref="UploadedByUserTypeId"/> for the caveat on "company".
        /// </summary>
        public string UploadedBySource { get; set; }

        /// <summary>
        /// Telesak user-type id: 2 = employee, 3 = company side. Null when unknown.
        ///
        /// **Caveat:** 3 covers both a company user and an Ops Portal / Saqia
        /// dispatch — dbo.TaskLog records only a single IsFromCompany bit, not the
        /// acting account, so those two cannot be told apart. The distinction that
        /// *is* reliable is 2 vs not-2: "the employee attached this" vs "someone on
        /// the company/dispatch side did".
        /// </summary>
        public int? UploadedByUserTypeId { get; set; }

        /// <summary>
        /// Always null — no uploader account id is recorded anywhere in the schema.
        /// Present so the contract does not have to change if a column is added later.
        /// </summary>
        public int? UploadedByUserAccountId { get; set; }

        /// <summary>Always null — see <see cref="UploadedByUserAccountId"/>.</summary>
        public string UploadedByName { get; set; }
    }
}
