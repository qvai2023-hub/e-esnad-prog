namespace EtaskMinstry.Api.Dtos.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DeviceInfoDto DeviceInfo { get; set; }
    }
}
