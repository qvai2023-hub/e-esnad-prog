namespace EtaskMinstry.Api.Dtos.Notifications
{
    /// <summary>
    /// Body for POST /api/v1/device-tokens.
    /// `Platform` is "FCM" for now (room to add APNs / WebPush later).
    /// </summary>
    public class DeviceTokenRequest
    {
        public string Token { get; set; }
        public string Platform { get; set; }
        public string Model { get; set; }
    }
}
