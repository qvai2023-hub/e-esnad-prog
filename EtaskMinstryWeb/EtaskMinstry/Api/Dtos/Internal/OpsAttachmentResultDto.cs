namespace EtaskMinstry.Api.Dtos.Internal
{
    /// <summary>
    /// Response <c>data</c> for POST /api/internal/tasks/{taskId}/attachments
    /// (Ops Portal server-to-server upload). Serialised camelCase by the API
    /// formatter.
    /// </summary>
    public class OpsAttachmentResultDto
    {
        public int AttachmentId { get; set; }
        public int TaskId { get; set; }

        /// <summary>Generated stored name (GUID hex + original extension), no path.</summary>
        public string FileName { get; set; }

        /// <summary>Sanitised original display name (Attachment.OriginalFileName).</summary>
        public string OriginalFileName { get; set; }

        public string Description { get; set; }
    }
}
