using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementModel
{
    /// <summary>
    /// Partial class to extend Attendance with heartbeat tracking properties.
    /// These properties map to columns that need to be added to the database:
    /// ALTER TABLE Attendance ADD LastHeartbeat DATETIME NULL;
    /// ALTER TABLE Attendance ADD AutoCheckout BIT DEFAULT 0;
    /// </summary>
    public partial class Attendance
    {
        /// <summary>
        /// Last heartbeat time from the JavaScript tracker.
        /// Updated every 60 seconds while the user is active.
        /// Used by the server-side auto-checkout job.
        /// </summary>
        [NotMapped] // Will be mapped after DB migration
        public DateTime? LastHeartbeat { get; set; }

        /// <summary>
        /// Flag indicating whether this checkout was performed automatically
        /// by the server-side job due to session timeout.
        /// </summary>
        [NotMapped] // Will be mapped after DB migration
        public bool AutoCheckout { get; set; }
    }
}
