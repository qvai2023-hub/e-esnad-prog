namespace EtaskMinstry.Api.Dtos.Auth
{
    /// <summary>
    /// Returned from /auth/login and /auth/refresh.
    /// </summary>
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }   // seconds
        public UserSummaryDto User { get; set; }
    }
}
