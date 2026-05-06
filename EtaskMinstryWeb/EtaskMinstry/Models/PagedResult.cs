using System;
using System.Collections.Generic;

namespace EtaskMinstry.Models
{
    /// <summary>
    /// Bug #36 — server-side pagination wrapper.
    /// Returned by VM Select methods that page at the DB level (Skip/Take)
    /// so the view does NOT have to load every row to render a single page.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalPages
        {
            get { return PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize); }
        }

        public PagedResult()
        {
            Items = new List<T>();
            PageNumber = 1;
            PageSize = 10;
        }

        public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = totalCount;
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 10 : pageSize;
        }
    }
}
