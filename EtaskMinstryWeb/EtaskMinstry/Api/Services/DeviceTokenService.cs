using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;

namespace EtaskMinstry.Api.Services
{
    /// <summary>
    /// CRUD over the MobileDeviceToken table.
    ///
    /// Direct ADO.NET — the EDMX is not extended (per CLAUDE.md "no .edmx
    /// changes"). Same pattern as RefreshTokenService.
    /// </summary>
    public class DeviceTokenService
    {
        public class ActiveToken
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string DeviceToken { get; set; }
            public string Platform { get; set; }
        }

        /// <summary>
        /// Idempotent register: if a row with this token already exists,
        /// update LastSeenDate (and re-activate if it was disabled). Else insert.
        /// </summary>
        public void Register(int userId, string token, string platform, string model)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            platform = string.IsNullOrWhiteSpace(platform) ? "FCM" : platform.Trim();

            using (var conn = OpenConnection())
            {
                // First try update (most calls are repeat heartbeats from the
                // same device). MERGE would be cleaner but legacy SQL Server
                // compat is the project default.
                const string updateSql = @"
UPDATE MobileDeviceToken
SET    LastSeenDate = GETDATE(),
       IsActive     = 1,
       UserID       = @UserID,
       Platform     = @Platform,
       DeviceModel  = @Model
WHERE  DeviceToken = @Token;
SELECT @@ROWCOUNT;";

                int rows;
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.Parameters.AddWithValue("@Platform", platform);
                    cmd.Parameters.AddWithValue("@Model", (object)model ?? DBNull.Value);
                    rows = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (rows > 0) return;

                const string insertSql = @"
INSERT INTO MobileDeviceToken (UserID, DeviceToken, Platform, DeviceModel, CreatedDate, LastSeenDate, IsActive)
VALUES (@UserID, @Token, @Platform, @Model, GETDATE(), GETDATE(), 1);";

                using (var cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.Parameters.AddWithValue("@Platform", platform);
                    cmd.Parameters.AddWithValue("@Model", (object)model ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deactivate a single token. Scoped to a user so one account can't
        /// disable another account's token by guessing the value.
        /// </summary>
        public void Unregister(int userId, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                "UPDATE MobileDeviceToken SET IsActive = 0 WHERE UserID = @UserID AND DeviceToken = @Token",
                conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deactivate a token regardless of user — used by FcmDispatcher when
        /// FCM returns InvalidRegistration / NotRegistered for the token.
        /// </summary>
        public void MarkInvalid(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                "UPDATE MobileDeviceToken SET IsActive = 0 WHERE DeviceToken = @Token",
                conn))
            {
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// All active tokens for a user. Used by FcmDispatcher fan-out.
        /// </summary>
        public List<ActiveToken> GetActive(int userId)
        {
            var list = new List<ActiveToken>();
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                "SELECT ID, UserID, DeviceToken, Platform FROM MobileDeviceToken WHERE UserID = @UserID AND IsActive = 1",
                conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                using (var reader = cmd.ExecuteReader(CommandBehavior.Default))
                {
                    while (reader.Read())
                    {
                        list.Add(new ActiveToken
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            DeviceToken = reader.GetString(2),
                            Platform = reader.GetString(3)
                        });
                    }
                }
            }
            return list;
        }

        private static SqlConnection OpenConnection()
        {
            string efConnStr = ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString;
            var builder = new EntityConnectionStringBuilder(efConnStr);
            var conn = new SqlConnection(builder.ProviderConnectionString);
            conn.Open();
            return conn;
        }
    }
}
