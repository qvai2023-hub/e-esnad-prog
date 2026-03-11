using System;
using System.Data.SqlClient;
using System.Threading;
using System.Web;

namespace EtaskMinstry.Services
{
    /// <summary>
    /// Server-side job that automatically checks out orphaned attendance sessions.
    /// This is the "safety net" (Layer 3) for cases when:
    /// - Browser crash without sendBeacon
    /// - Network failure preventing client-side checkout
    /// - User closes laptop lid (sleep mode)
    ///
    /// Runs every 15 minutes and closes any open attendance records where
    /// LastHeartbeat is older than 15 minutes.
    /// </summary>
    public static class AutoCheckoutJob
    {
        private static Timer _timer;
        private static readonly object _lock = new object();
        private static bool _isRunning = false;

        // Configuration
        private const int JOB_INTERVAL_MINUTES = 15;
        private const int HEARTBEAT_TIMEOUT_MINUTES = 15;

        /// <summary>
        /// Starts the auto-checkout background job.
        /// Call this from Application_Start in Global.asax.cs
        /// </summary>
        public static void Start()
        {
            if (_timer != null) return;

            // Run first time after 2 minutes, then every 15 minutes
            _timer = new Timer(
                ProcessOrphanedSessions,
                null,
                TimeSpan.FromMinutes(2),
                TimeSpan.FromMinutes(JOB_INTERVAL_MINUTES)
            );

            System.Diagnostics.Debug.WriteLine("[AutoCheckoutJob] Started - will run every {0} minutes", JOB_INTERVAL_MINUTES);
        }

        /// <summary>
        /// Stops the auto-checkout background job.
        /// Call this from Application_End in Global.asax.cs
        /// </summary>
        public static void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Main job logic - finds and closes orphaned attendance sessions
        /// </summary>
        private static void ProcessOrphanedSessions(object state)
        {
            // Prevent concurrent runs
            lock (_lock)
            {
                if (_isRunning) return;
                _isRunning = true;
            }

            try
            {
                var connStr = GetSqlConnectionString();
                if (string.IsNullOrEmpty(connStr))
                {
                    System.Diagnostics.Debug.WriteLine("[AutoCheckoutJob] No connection string available");
                    return;
                }

                var cutoffTime = DateTime.Now.AddMinutes(-HEARTBEAT_TIMEOUT_MINUTES);
                int processedCount = 0;

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Find orphaned sessions: open attendance with LastHeartbeat > 15 minutes ago
                    // Or CheckIn > 15 minutes ago if LastHeartbeat is null (JS tracker never connected)
                    string selectSql = @"
                        SELECT Id, EmpId, CheckIn, LastHeartbeat
                        FROM Attendance
                        WHERE CheckOut IS NULL
                          AND (
                              (LastHeartbeat IS NOT NULL AND LastHeartbeat < @CutoffTime)
                              OR
                              (LastHeartbeat IS NULL AND CheckIn < @CutoffTime)
                          )";

                    using (var cmd = new SqlCommand(selectSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CutoffTime", cutoffTime);

                        using (var reader = cmd.ExecuteReader())
                        {
                            var orphanedSessions = new System.Collections.Generic.List<OrphanedSession>();

                            while (reader.Read())
                            {
                                orphanedSessions.Add(new OrphanedSession
                                {
                                    Id = reader.GetInt32(0),
                                    EmpId = reader.GetInt32(1),
                                    CheckIn = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                    LastHeartbeat = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3)
                                });
                            }

                            reader.Close();

                            // Process each orphaned session
                            foreach (var session in orphanedSessions)
                            {
                                try
                                {
                                    ProcessOrphanedSession(conn, session);
                                    processedCount++;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        "[AutoCheckoutJob] Error processing session {0}: {1}",
                                        session.Id, ex.Message);
                                }
                            }
                        }
                    }
                }

                if (processedCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[AutoCheckoutJob] Processed {0} orphaned sessions at {1}",
                        processedCount, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AutoCheckoutJob] Error: {0}", ex.Message);
            }
            finally
            {
                _isRunning = false;
            }
        }

        /// <summary>
        /// Processes a single orphaned session - sets CheckOut time and handles midnight split
        /// </summary>
        private static void ProcessOrphanedSession(SqlConnection conn, OrphanedSession session)
        {
            // Checkout time is LastHeartbeat if available, otherwise CheckIn
            DateTime checkoutTime = session.LastHeartbeat ?? session.CheckIn ?? DateTime.Now;

            // Handle midnight split if session crosses midnight
            if (session.CheckIn.HasValue && checkoutTime.Date > session.CheckIn.Value.Date)
            {
                // Close original session at 23:59:59 of check-in day
                var endOfDay = session.CheckIn.Value.Date.AddDays(1).AddSeconds(-1);
                UpdateCheckout(conn, session.Id, endOfDay, true);

                // Create new records for subsequent days
                DateTime currentDate = session.CheckIn.Value.Date.AddDays(1);
                while (currentDate.Date <= checkoutTime.Date)
                {
                    DateTime dayStart = currentDate.Date;
                    DateTime dayEnd = currentDate.Date == checkoutTime.Date
                        ? checkoutTime
                        : currentDate.Date.AddDays(1).AddSeconds(-1);

                    InsertSplitAttendance(conn, session.EmpId, dayStart, dayEnd);
                    currentDate = currentDate.AddDays(1);
                }
            }
            else
            {
                // Simple case - same day checkout
                UpdateCheckout(conn, session.Id, checkoutTime, true);
            }
        }

        private static void UpdateCheckout(SqlConnection conn, int attendanceId, DateTime checkoutTime, bool autoCheckout)
        {
            string sql = "UPDATE Attendance SET CheckOut = @CheckOut, AutoCheckout = @AutoCheckout WHERE Id = @Id";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CheckOut", checkoutTime);
                cmd.Parameters.AddWithValue("@AutoCheckout", autoCheckout);
                cmd.Parameters.AddWithValue("@Id", attendanceId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertSplitAttendance(SqlConnection conn, int empId, DateTime checkIn, DateTime checkOut)
        {
            string sql = @"INSERT INTO Attendance (EmpId, CheckIn, CheckOut, AutoCheckout, LastHeartbeat)
                           VALUES (@EmpId, @CheckIn, @CheckOut, 1, @CheckOut)";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@EmpId", empId);
                cmd.Parameters.AddWithValue("@CheckIn", checkIn);
                cmd.Parameters.AddWithValue("@CheckOut", checkOut);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets SQL Server connection string from EF connection string
        /// </summary>
        private static string GetSqlConnectionString()
        {
            try
            {
                var efConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"]?.ToString();
                if (string.IsNullOrEmpty(efConnStr)) return null;

                var entityBuilder = new System.Data.EntityClient.EntityConnectionStringBuilder(efConnStr);
                return entityBuilder.ProviderConnectionString;
            }
            catch
            {
                return null;
            }
        }

        private class OrphanedSession
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public DateTime? CheckIn { get; set; }
            public DateTime? LastHeartbeat { get; set; }
        }
    }
}
