using System;

namespace EtaskMinstry.Api.Dtos.Notifications
{
    /// <summary>
    /// One notification row for /api/v1/notifications.
    ///
    /// `Id` is the per-user NotificationCollection row id — that's the id
    /// the mobile app passes back to POST /api/v1/notifications/{id}/read.
    /// `NotificationId` is the shared Notification row id (in case the
    /// app needs to dedupe across users).
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public int NotificationId { get; set; }
        public string Value { get; set; }
        public string Link { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsSeen { get; set; }
        public string Type { get; set; }
    }
}
