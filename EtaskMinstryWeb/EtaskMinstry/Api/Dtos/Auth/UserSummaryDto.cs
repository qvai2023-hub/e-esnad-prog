namespace EtaskMinstry.Api.Dtos.Auth
{
    /// <summary>
    /// Lightweight user profile included in login/refresh responses and /me.
    /// </summary>
    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }       // "Employee" or "Company"
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsStopped { get; set; }
    }
}
