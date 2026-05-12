namespace EtaskMinstry.Api.Dtos.Auth
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; }

        /// <summary>
        /// Optional FCM device token to deactivate on this logout (Slice 6).
        /// Slice 2 ignores this if MobileDeviceToken table is empty.
        /// </summary>
        public string DeviceToken { get; set; }
    }
}
