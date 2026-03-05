namespace TaskManagementModel
{
    using System.Data.Entity;

    public partial class EtaskMinstryEntities
    {
        public DbSet<ActivityLog> ActivityLogs { get; set; }
    }
}
