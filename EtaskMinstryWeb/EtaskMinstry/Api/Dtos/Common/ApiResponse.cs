using System.Collections.Generic;

namespace EtaskMinstry.Api.Dtos.Common
{
    /// <summary>
    /// Standard envelope returned by every Mobile API endpoint.
    /// All fields are serialised camelCase by the API formatter.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Optional machine-readable error code (e.g. "ACCOUNT_STOPPED",
        /// "INVALID_CREDENTIALS", "TOKEN_REVOKED"). Null on success.
        /// Serialised only when set (NullValueHandling.Ignore).
        /// </summary>
        public string Code { get; set; }

        public List<ErrorItem> Errors { get; set; }

        public ApiResponse()
        {
            Errors = new List<ErrorItem>();
            Message = string.Empty;
        }

        public static ApiResponse Ok(object data = null, string message = "")
        {
            return new ApiResponse { Success = true, Data = data, Message = message };
        }

        public static ApiResponse Fail(string message, string code = null, List<ErrorItem> errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Code = code,
                Errors = errors ?? new List<ErrorItem>()
            };
        }
    }
}
