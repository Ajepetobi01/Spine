using Spine.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Filter
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        //public int PageNumber { get; set; }
        //public int PageSize { get; set; }
        //public string SortBy { get; set; }
        //public PaginationFilter()
        //{
        //    this.PageNumber = 1;
        //    this.PageSize = 10;
        //}
        //public PaginationFilter(int pageNumber, int pageSize)
        //{
        //    this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
        //    this.PageSize = pageSize > 10 ? 10 : pageSize;
        //}

    }
}
