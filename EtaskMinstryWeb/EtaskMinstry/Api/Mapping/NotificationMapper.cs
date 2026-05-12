using EtaskMinstry.Api.Dtos.Notifications;
using TaskManagementModel;

namespace EtaskMinstry.Api.Mapping
{
    public static class NotificationMapper
    {
        public static NotificationDto ToDto(NotificationCollection c)
        {
            if (c == null) return null;
            return new NotificationDto
            {
                Id = c.ID,
                NotificationId = c.NotificationID,
                Value = c.Notification != null ? c.Notification.Value : null,
                Link = c.Notification != null ? c.Notification.LinkToGo : null,
                SendDate = c.Notification != null ? c.Notification.CreateDate : System.DateTime.MinValue,
                IsSeen = c.IsSeen,
                Type = (c.Notification != null && c.Notification.NotificationType != null)
                            ? c.Notification.NotificationType.Name
                            : null
            };
        }
    }
}
