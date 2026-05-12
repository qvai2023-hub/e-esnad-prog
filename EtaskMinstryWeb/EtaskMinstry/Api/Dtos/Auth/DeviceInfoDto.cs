namespace EtaskMinstry.Api.Dtos.Auth
{
    /// <summary>
    /// Optional device metadata sent at login. Used (in Slice 6) to enrich
    /// MobileDeviceToken rows so support can see what device a push went to.
    /// </summary>
    public class DeviceInfoDto
    {
        public string Model { get; set; }
        public string Platform { get; set; }
    }
}
