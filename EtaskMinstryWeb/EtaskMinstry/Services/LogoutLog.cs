using System;
using System.Data.SqlClient;

namespace EtaskMinstry.Services
{
    /// <summary>
    /// Writes the Logout event (LogGeneral.ActionID = 9) at most once per Login.
    ///
    /// The same sign-in can be ended from several places: the Logout button, the
    /// inactivity checkout, a heartbeat that finds the attendance already closed, and
    /// AutoCheckoutJob when heartbeats stop. Each of them calls WriteOnce, and the row
    /// is only inserted when the user's latest Login/Logout event is a Login, so the
    /// first one wins and the rest write nothing.
    /// </summary>
    public static class LogoutLog
    {
        /// <param name="userId">EmpID for an employee, CompanyID for a company</param>
        /// <param name="companyId">the employee's company; null looks it up from Employee</param>
        /// <returns>true if the Logout row was written</returns>
        public static bool WriteOnce(SqlConnection conn, int userId, int? companyId, bool isCompany, string ip, DateTime actionTime)
        {
            const string sql = @"
                INSERT INTO dbo.LogGeneral (ActionID, ActionTime, Value, EmployeeID, CompanyID, IsCompany, ControllerID)
                SELECT @LogOut, @ActionTime, @Value,
                       CASE WHEN @IsCompany = 1 THEN NULL ELSE @UserId END,
                       CASE WHEN @IsCompany = 1 THEN @UserId
                            ELSE ISNULL(@CompanyId, (SELECT CompanyID FROM dbo.Employee WHERE EmpID = @UserId)) END,
                       @IsCompany,
                       (SELECT TOP 1 ControllerID FROM dbo.ControllerName WHERE ControllerNameEnglish = 'Security')
                WHERE (SELECT TOP 1 lg.ActionID
                       FROM   dbo.LogGeneral lg
                       WHERE  lg.ActionID IN (@Login, @LogOut)
                         AND  ISNULL(lg.IsCompany, 0) = @IsCompany
                         AND  ((@IsCompany = 0 AND lg.EmployeeID = @UserId)
                            OR (@IsCompany = 1 AND lg.CompanyID = @UserId AND lg.EmployeeID IS NULL))
                       ORDER BY lg.ActionTime DESC, lg.LogID DESC) = @Login";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@LogOut", (int)ActionType.LogOut);
                cmd.Parameters.AddWithValue("@Login", (int)ActionType.Login);
                cmd.Parameters.AddWithValue("@ActionTime", actionTime);
                cmd.Parameters.AddWithValue("@Value", (object)ip ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CompanyId", (object)companyId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsCompany", isCompany);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>Same as above, on its own connection; never throws.</summary>
        public static bool WriteOnce(int userId, int? companyId, bool isCompany, string ip, DateTime actionTime)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString()))
                {
                    conn.Open();
                    return WriteOnce(conn, userId, companyId, isCompany, ip, actionTime);
                }
            }
            catch
            {
                return false;
            }
        }

        internal static string ConnectionString()
        {
            var efConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString();
            return new System.Data.EntityClient.EntityConnectionStringBuilder(efConnStr).ProviderConnectionString;
        }
    }
}
