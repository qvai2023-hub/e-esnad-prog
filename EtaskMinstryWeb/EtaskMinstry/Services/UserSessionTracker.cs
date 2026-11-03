using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace EtaskMinstry.Services
{
    /// <summary>
    /// One row in dbo.UserSession per sign-in (Company and Employee; Admin is not logged).
    ///
    /// Every tab of the same browser shares the ASP.NET session and so the same row:
    /// each open tab sends a heartbeat (Scripts/session-heartbeat.js), closing one tab
    /// while another is open changes nothing, and the session is closed only by an
    /// explicit Logout, a new Login in the same browser, or the heartbeat timeout
    /// (AutoCheckoutJob). Whoever closes the row writes the single LogGeneral Logout
    /// (ActionID 9) and checks out the employee's open attendance - the close is one
    /// UPDATE ... WHERE LogoutAt IS NULL, so a session can never be logged out twice.
    ///
    /// Uses direct SQL: dbo.UserSession is not in the EDMX. If the table does not exist
    /// yet (Telesak-Docs/sql/user-session-tracking.sql not run) every call fails quietly
    /// and Login/Logout keep their previous behaviour.
    /// </summary>
    public static class UserSessionTracker
    {
        /// <summary>ASP.NET Session key holding this browser session's dbo.UserSession.SessionKey.</summary>
        public const string SessionKeyName = "UserSessionKey";

        public const string ReasonLogout = "Logout";
        public const string ReasonTimeout = "Timeout";
        public const string ReasonRelogin = "Relogin";

        private const int DefaultTimeoutMinutes = 15;

        public enum CloseResult
        {
            /// <summary>No tracked session (table missing, or signed in before tracking existed).</summary>
            NotTracked,
            /// <summary>This call closed the session and wrote the Logout log.</summary>
            Closed,
            /// <summary>The session was already closed (timeout or another tab); nothing written.</summary>
            AlreadyClosed
        }

        public enum HeartbeatResult
        {
            NotTracked,
            Active,
            /// <summary>The server already closed this session: the page should sign out.</summary>
            Closed
        }

        /// <summary>Minutes without a heartbeat before the server closes a session (appSettings SessionHeartbeatTimeoutMinutes).</summary>
        public static int TimeoutMinutes
        {
            get
            {
                int minutes;
                return int.TryParse(ConfigurationManager.AppSettings["SessionHeartbeatTimeoutMinutes"], out minutes) && minutes > 0
                    ? minutes
                    : DefaultTimeoutMinutes;
            }
        }

        /// <summary>
        /// Opens the session row after a successful Login (call after the Login log is written).
        /// An open session already held by this browser is closed first as Relogin.
        /// </summary>
        public static void Start(int? employeeId, int? companyId, bool isCompany, string ipAddress)
        {
            var httpSession = HttpContext.Current?.Session;
            if (httpSession == null)
                return;

            try
            {
                // LoginETask has already checked the employee in, so the previous session
                // must not check attendance out (it would close the row just opened)
                Guid previousKey;
                if (TryGetKey(out previousKey))
                    Close(previousKey, ReasonRelogin, ipAddress, false);

                var key = Guid.NewGuid();
                var now = DateTime.Now;
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.UserSession (SessionKey, EmployeeID, CompanyID, IsCompany, IpAddress, LoginAt, LastHeartbeatAt)
                    VALUES (@SessionKey, @EmployeeID, @CompanyID, @IsCompany, @IpAddress, @Now, @Now)", conn))
                {
                    cmd.Parameters.AddWithValue("@SessionKey", key);
                    cmd.Parameters.AddWithValue("@EmployeeID", (object)employeeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyID", (object)companyId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsCompany", isCompany);
                    cmd.Parameters.AddWithValue("@IpAddress", (object)Truncate(ipAddress, 64) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Now", now);
                    cmd.ExecuteNonQuery();
                }
                httpSession[SessionKeyName] = key;
            }
            catch (Exception ex)
            {
                httpSession.Remove(SessionKeyName);
                System.Diagnostics.Debug.WriteLine("[UserSessionTracker] Start failed: {0}", ex.Message);
            }
        }

        /// <summary>Records that a page of this session is still open.</summary>
        public static HeartbeatResult Heartbeat()
        {
            Guid key;
            if (!TryGetKey(out key))
                return HeartbeatResult.NotTracked;

            try
            {
                using (var conn = OpenConnection())
                {
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.UserSession SET LastHeartbeatAt = @Now WHERE SessionKey = @SessionKey AND LogoutAt IS NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@SessionKey", key);
                        if (cmd.ExecuteNonQuery() == 1)
                            return HeartbeatResult.Active;
                    }
                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.UserSession WHERE SessionKey = @SessionKey", conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionKey", key);
                        return (int)cmd.ExecuteScalar() > 0 ? HeartbeatResult.Closed : HeartbeatResult.NotTracked;
                    }
                }
            }
            catch (Exception ex)
            {
                // Never sign a user out because the database could not be reached
                System.Diagnostics.Debug.WriteLine("[UserSessionTracker] Heartbeat failed: {0}", ex.Message);
                return HeartbeatResult.NotTracked;
            }
        }

        /// <summary>Closes this browser's session (explicit Logout).</summary>
        public static CloseResult CloseCurrent(string reason, string ipAddress)
        {
            Guid key;
            if (!TryGetKey(out key))
                return CloseResult.NotTracked;

            HttpContext.Current.Session.Remove(SessionKeyName);
            return Close(key, reason, ipAddress, true);
        }

        /// <summary>
        /// Closes every session whose last heartbeat is older than the timeout, logging the
        /// Logout at the last heartbeat (the last moment the user was seen). Run by AutoCheckoutJob.
        /// </summary>
        public static int CloseExpired(SqlConnection conn)
        {
            var cutoff = DateTime.Now.AddMinutes(-TimeoutMinutes);
            using (var tx = conn.BeginTransaction())
            {
                var closed = new List<ClosedSession>();
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.UserSession
                    SET LogoutAt = LastHeartbeatAt, CloseReason = @Reason
                    OUTPUT inserted.EmployeeID, inserted.CompanyID, inserted.IsCompany, inserted.IpAddress, inserted.LogoutAt
                    WHERE LogoutAt IS NULL AND LastHeartbeatAt < @Cutoff", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Reason", ReasonTimeout);
                    cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            closed.Add(ReadClosed(reader));
                    }
                }

                foreach (var session in closed)
                    WriteLogout(conn, tx, session, session.IpAddress);
                tx.Commit();

                foreach (var session in closed)
                    CheckoutAttendance(conn, session);
                return closed.Count;
            }
        }

        private static CloseResult Close(Guid key, string reason, string ipAddress, bool checkoutAttendance)
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    ClosedSession session = null;
                    using (var tx = conn.BeginTransaction())
                    {
                        using (var cmd = new SqlCommand(@"
                            UPDATE dbo.UserSession
                            SET LogoutAt = @Now, CloseReason = @Reason
                            OUTPUT inserted.EmployeeID, inserted.CompanyID, inserted.IsCompany, inserted.IpAddress, inserted.LogoutAt
                            WHERE SessionKey = @SessionKey AND LogoutAt IS NULL", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Reason", reason);
                            cmd.Parameters.AddWithValue("@SessionKey", key);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                    session = ReadClosed(reader);
                            }
                        }

                        if (session == null)
                        {
                            tx.Commit();
                            return CloseResult.AlreadyClosed;
                        }

                        WriteLogout(conn, tx, session, ipAddress ?? session.IpAddress);
                        tx.Commit();
                    }

                    if (checkoutAttendance)
                        CheckoutAttendance(conn, session);
                    return CloseResult.Closed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[UserSessionTracker] Close failed: {0}", ex.Message);
                return CloseResult.NotTracked;
            }
        }

        /// <summary>Same row Generallog.LoginORLogout(ActionType.LogOut) writes, at the close time.</summary>
        private static void WriteLogout(SqlConnection conn, SqlTransaction tx, ClosedSession session, string ipAddress)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.LogGeneral (ActionID, ActionTime, Value, EmployeeID, CompanyID, IsCompany, ControllerID)
                VALUES (@ActionID, @ActionTime, @Value, @EmployeeID, @CompanyID, @IsCompany,
                        (SELECT TOP 1 ControllerID FROM dbo.ControllerName WHERE ControllerNameEnglish = 'Security'))", conn, tx))
            {
                cmd.Parameters.AddWithValue("@ActionID", (int)ActionType.LogOut);
                cmd.Parameters.AddWithValue("@ActionTime", session.LogoutAt);
                cmd.Parameters.AddWithValue("@Value", (object)ipAddress ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmployeeID", (object)session.EmployeeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CompanyID", (object)session.CompanyId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsCompany", session.IsCompany);
                cmd.ExecuteNonQuery();
            }
        }

        private static void CheckoutAttendance(SqlConnection conn, ClosedSession session)
        {
            if (session.IsCompany || !session.EmployeeId.HasValue)
                return;
            try
            {
                // Attendance is per employee, sessions are per browser: leave it open while
                // the employee is still signed in somewhere else
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM dbo.UserSession WHERE EmployeeID = @EmployeeID AND IsCompany = 0 AND LogoutAt IS NULL", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", session.EmployeeId.Value);
                    if ((int)cmd.ExecuteScalar() > 0)
                        return;
                }
                AutoCheckoutJob.CloseOpenAttendance(conn, session.EmployeeId.Value, session.LogoutAt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[UserSessionTracker] Attendance checkout failed for {0}: {1}", session.EmployeeId, ex.Message);
            }
        }

        private static ClosedSession ReadClosed(SqlDataReader reader)
        {
            return new ClosedSession
            {
                EmployeeId = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                CompanyId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                IsCompany = reader.GetBoolean(2),
                IpAddress = reader.IsDBNull(3) ? null : reader.GetString(3),
                LogoutAt = reader.GetDateTime(4)
            };
        }

        private static bool TryGetKey(out Guid key)
        {
            key = Guid.Empty;
            var value = HttpContext.Current?.Session?[SessionKeyName];
            if (value is Guid)
            {
                key = (Guid)value;
                return true;
            }
            return false;
        }

        internal static SqlConnection OpenConnection()
        {
            var efConnStr = ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString();
            var entityBuilder = new System.Data.EntityClient.EntityConnectionStringBuilder(efConnStr);
            var conn = new SqlConnection(entityBuilder.ProviderConnectionString);
            conn.Open();
            return conn;
        }

        private static string Truncate(string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(0, length) : value;
        }

        private class ClosedSession
        {
            public int? EmployeeId { get; set; }
            public int? CompanyId { get; set; }
            public bool IsCompany { get; set; }
            public string IpAddress { get; set; }
            public DateTime LogoutAt { get; set; }
        }
    }
}
