using System;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace EtaskMinstry.Api.Services
{
    /// <summary>
    /// Persists refresh tokens as SHA-256 hashes in the RefreshToken table.
    /// Uses direct ADO.NET so the EDMX model is not touched (per brief: no EF
    /// version upgrades, no .edmx changes).
    ///
    /// Connection is derived from the existing ETaskEntities EF connection
    /// string by extracting its provider connection string component.
    /// </summary>
    public class RefreshTokenService
    {
        public class StoreResult
        {
            public int Id { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        public class LookupResult
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int UserTypeId { get; set; }
            public DateTime ExpiresAt { get; set; }
            public bool IsRevoked { get; set; }
        }

        public static string Hash(string plaintext)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
                var sb = new StringBuilder(64);
                for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        public StoreResult Store(int userId, int userTypeId, string plaintext, int? replacedByTokenId = null)
        {
            int days;
            if (!int.TryParse(ConfigurationManager.AppSettings["JwtRefreshExpiryDays"], out days))
                days = 30;

            DateTime expiresAt = DateTime.Now.AddDays(days);
            string hash = Hash(plaintext);

            using (var conn = OpenConnection())
            {
                const string sql = @"
INSERT INTO RefreshToken (UserID, UserTypeID, TokenHash, ExpiresAt, IsRevoked, CreatedAt, ReplacedByTokenId)
VALUES (@UserID, @UserTypeID, @TokenHash, @ExpiresAt, 0, GETDATE(), @ReplacedByTokenId);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@UserTypeID", userTypeId);
                    cmd.Parameters.AddWithValue("@TokenHash", hash);
                    cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
                    cmd.Parameters.AddWithValue("@ReplacedByTokenId",
                        (object)replacedByTokenId ?? DBNull.Value);

                    object idObj = cmd.ExecuteScalar();
                    int id = idObj == null ? 0 : Convert.ToInt32(idObj);
                    return new StoreResult { Id = id, ExpiresAt = expiresAt };
                }
            }
        }

        public LookupResult FindByPlaintext(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext)) return null;
            string hash = Hash(plaintext);

            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                "SELECT ID, UserID, UserTypeID, ExpiresAt, IsRevoked FROM RefreshToken WHERE TokenHash = @Hash",
                conn))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read()) return null;
                    return new LookupResult
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        UserTypeId = reader.GetInt32(2),
                        ExpiresAt = reader.GetDateTime(3),
                        IsRevoked = reader.GetBoolean(4)
                    };
                }
            }
        }

        public void Revoke(int id, string reason)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                @"UPDATE RefreshToken SET IsRevoked = 1, RevokedAt = GETDATE(), RevokeReason = @Reason
                  WHERE ID = @Id AND IsRevoked = 0",
                conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Reason", (object)reason ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void LinkReplacement(int oldId, int newId)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                "UPDATE RefreshToken SET ReplacedByTokenId = @NewId WHERE ID = @OldId",
                conn))
            {
                cmd.Parameters.AddWithValue("@NewId", newId);
                cmd.Parameters.AddWithValue("@OldId", oldId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Revokes every non-revoked refresh token for a user.
        /// Called when reuse of a revoked token is detected (token theft).
        /// </summary>
        public void RevokeAllForUser(int userId, string reason)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                @"UPDATE RefreshToken SET IsRevoked = 1, RevokedAt = GETDATE(), RevokeReason = @Reason
                  WHERE UserID = @UserId AND IsRevoked = 0",
                conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Reason", (object)reason ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes rows whose ExpiresAt is older than (now - 30 days).
        /// Invoked by RefreshTokenCleanupJob.
        /// </summary>
        public int DeleteExpiredOlderThan(int days)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(
                "DELETE FROM RefreshToken WHERE ExpiresAt < DATEADD(day, -@Days, GETDATE())",
                conn))
            {
                cmd.Parameters.AddWithValue("@Days", days);
                return cmd.ExecuteNonQuery();
            }
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
