using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Wrappers
{
    public class PagedResponse<T> : Response<T>
    {
        public PagedResponse() { }
        public PagedResponse(IEnumerable<T> data)
        {
            Data = data;
        }
        public IEnumerable<T> Data { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string NextPage { get; set; }
        public string PreviousPage { get; set; }

        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }



        //public int PageNumber { get; set; }
        //public int PageSize { get; set; }
        //public Uri FirstPage { get; set; }
        //public Uri LastPage { get; set; }

        //public int TotalPages { get; set; }
        //public int TotalRecords { get; set; }
        //public Uri NextPage { get; set; }
        //public Uri PreviousPage { get; set; }

        //public PagedResponse(T data, int pageNumber, int pageSize)
        //{
        //    this.PageNumber = pageNumber;
        //    this.PageSize = pageSize;
        //    this.Data = data;
        //    this.Message = null;
        //    this.Succeeded = true;
        //    this.Errors = null;
        //}
    }
}
