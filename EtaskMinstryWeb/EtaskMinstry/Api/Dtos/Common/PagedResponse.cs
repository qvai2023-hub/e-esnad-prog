using System.Collections.Generic;

namespace EtaskMinstry.Api.Dtos.Common
{
    /// <summary>
    /// Standard envelope for paginated list endpoints.
    /// All fields are serialised camelCase by the API formatter.
    /// </summary>
    public class PagedResponse
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string Message { get; set; }
        public List<ErrorItem> Errors { get; set; }

        public PagedResponse()
        {
            Errors = new List<ErrorItem>();
            Message = string.Empty;
        }

        public static PagedResponse Build(object items, int page, int pageSize, int totalCount, string message = "")
        {
            int totalPages = (pageSize <= 0 || totalCount <= 0)
                ? 0
                : (totalCount + pageSize - 1) / pageSize;

            return new PagedResponse
            {
                Success = true,
                Data = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Message = message
            };
        }
    }
}
