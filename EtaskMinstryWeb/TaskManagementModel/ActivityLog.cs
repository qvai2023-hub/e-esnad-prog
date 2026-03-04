namespace TaskManagementModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ActivityLog")]
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmpId { get; set; }

        [Required]
        public int AttendanceId { get; set; }

        [Required]
        public DateTime LastActivityTime { get; set; }

        [StringLength(50)]
        public string ActivityType { get; set; }

        [ForeignKey("EmpId")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("AttendanceId")]
        public virtual Attendance Attendance { get; set; }
    }
}
