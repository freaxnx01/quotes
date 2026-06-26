using System;
using System.Collections.Generic;

namespace Quotes.DataModel
{
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }

        public PaginatedResponse()
        {
            Items = new List<T>();
        }

        public PaginatedResponse(List<T> items, int page, int pageSize, int total)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            Total = total;
        }
    }
}