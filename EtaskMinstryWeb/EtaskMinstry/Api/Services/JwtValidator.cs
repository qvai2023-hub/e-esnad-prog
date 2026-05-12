using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EtaskMinstry.Api.Services
{
    /// <summary>
    /// Validates HS256 JWTs minted by JwtIssuer. Returns claim values on success
    /// or null on any failure (bad format, bad signature, expired, wrong issuer).
    /// </summary>
    public class JwtValidator
    {
        public class ClaimsResult
        {
            public int UserId { get; set; }
            public int UserTypeId { get; set; }
            public int? CompanyId { get; set; }
            public string Name { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        public ClaimsResult Validate(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            string[] parts = token.Split('.');
            if (parts.Length != 3) return null;

            string secret = ConfigurationManager.AppSettings["JwtSecret"];
            string issuer = ConfigurationManager.AppSettings["JwtIssuer"] ?? "Telesak";
            if (string.IsNullOrEmpty(secret) || secret.StartsWith("REPLACE_")) return null;

            // 1. signature check
            string signingInput = parts[0] + "." + parts[1];
            byte[] expected;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            }

            byte[] actual;
            try { actual = Base64UrlDecode(parts[2]); }
            catch { return null; }

            if (!ConstantTimeEquals(expected, actual)) return null;

            // 2. decode payload
            JObject payload;
            try
            {
                string json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                payload = JObject.Parse(json);
            }
            catch { return null; }

            // 3. issuer + expiry
            string iss = (string)payload["iss"];
            if (!string.Equals(iss, issuer, StringComparison.Ordinal)) return null;

            long? exp = (long?)payload["exp"];
            if (!exp.HasValue) return null;

            DateTime expiresAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(exp.Value);
            if (expiresAt <= DateTime.UtcNow) return null;

            // 4. extract claims
            int userId;
            if (!int.TryParse((string)payload["sub"], out userId)) return null;

            int userTypeId = (int)(payload["userTypeId"] ?? 0);
            int? companyId = payload["companyId"] != null && payload["companyId"].Type != JTokenType.Null
                ? (int?)payload["companyId"]
                : null;
            string name = (string)payload["name"];

            return new ClaimsResult
            {
                UserId = userId,
                UserTypeId = userTypeId,
                CompanyId = companyId,
                Name = name,
                ExpiresAt = expiresAt
            };
        }

        internal static byte[] Base64UrlDecode(string s)
        {
            string padded = s.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
