using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace EtaskMinstry.Api.Services
{
    /// <summary>
    /// Issues HS256-signed JWT access tokens for the Mobile API.
    ///
    /// Token format (RFC 7519):
    ///   base64url(header) "." base64url(payload) "." base64url(HMACSHA256(header.payload))
    ///
    /// Claims used:
    ///   sub        - userId (int as string)
    ///   userTypeId - 2 = Employee, 3 = Company (matches LoggedUserType enum)
    ///   companyId  - the CompanyID for the user (employee's company or the company itself)
    ///   name       - user display name
    ///   iss        - issuer (Web.config: JwtIssuer)
    ///   iat        - issued-at (Unix seconds)
    ///   exp        - expiry (Unix seconds)
    ///   jti        - random id, prevents token caching
    /// </summary>
    public class JwtIssuer
    {
        public class IssuedToken
        {
            public string Token { get; set; }
            public DateTime ExpiresAt { get; set; }
            public int ExpiresInSeconds { get; set; }
        }

        public IssuedToken Issue(int userId, int userTypeId, int? companyId, string name)
        {
            string secret = ConfigurationManager.AppSettings["JwtSecret"];
            string issuer = ConfigurationManager.AppSettings["JwtIssuer"] ?? "Telesak";
            int minutes;
            if (!int.TryParse(ConfigurationManager.AppSettings["JwtAccessExpiryMinutes"], out minutes))
                minutes = 60;

            if (string.IsNullOrEmpty(secret) || secret.StartsWith("REPLACE_"))
                throw new InvalidOperationException("JwtSecret is not configured in Web.config.");

            DateTime now = DateTime.UtcNow;
            DateTime exp = now.AddMinutes(minutes);

            var header = new Dictionary<string, object>
            {
                { "alg", "HS256" },
                { "typ", "JWT" }
            };

            var payload = new Dictionary<string, object>
            {
                { "sub", userId.ToString() },
                { "userTypeId", userTypeId },
                { "companyId", companyId },
                { "name", name ?? string.Empty },
                { "iss", issuer },
                { "iat", ToUnixSeconds(now) },
                { "exp", ToUnixSeconds(exp) },
                { "jti", Guid.NewGuid().ToString("N") }
            };

            string headerEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)));
            string payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
            string signingInput = headerEncoded + "." + payloadEncoded;

            byte[] signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            }

            string token = signingInput + "." + Base64UrlEncode(signature);

            return new IssuedToken
            {
                Token = token,
                ExpiresAt = exp,
                ExpiresInSeconds = minutes * 60
            };
        }

        /// <summary>
        /// Generates a random opaque refresh token (raw bytes base64-url-encoded).
        /// Caller stores SHA-256 hash; plaintext is returned to the client only.
        /// </summary>
        public string IssueRefreshToken()
        {
            byte[] bytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        internal static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                          .TrimEnd('=')
                          .Replace('+', '-')
                          .Replace('/', '_');
        }

        internal static long ToUnixSeconds(DateTime utc)
        {
            return (long)(utc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
