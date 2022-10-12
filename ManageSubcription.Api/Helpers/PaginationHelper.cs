using ManageSubcription.Api.Services;
using ManageSubcription.Api.Wrappers;
using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Helpers
{
    public class PaginationHelper
    {
        public static object PagedResponse<T>(IUriService uriService, PaginationFilter pagination, List<T> response, int totalRecords)
        {
            var totalPages = ((double)totalRecords / (double)pagination.PageSize);
            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));
            var nextPage = pagination.PageNumber >= 1 ? uriService
                .GetAllPostsUri(new PaginationQuery(pagination.PageNumber + 1, pagination.PageSize)).ToString()
                : null;

            var previousPage = pagination.PageNumber - 1 >= 1
                ? uriService.GetAllPostsUri(new PaginationQuery(pagination.PageNumber - 1, pagination.PageSize)).ToString()
                : null;

            return new PagedResponse<T>
            {
                Data = response,
                PageNumber = pagination.PageNumber >= 1 ? pagination.PageNumber : (int?)null,
                PageSize = pagination.PageSize >= 1 ? pagination.PageSize : (int?)null,
                NextPage = response.Any() ? nextPage : null,
                PreviousPage = previousPage,
                TotalPages = roundedTotalPages,
                TotalRecords = totalRecords
            };
        }
        //public static PagedResponse<List<T>> CreatePagedReponse<T>(List<T> pagedData, PaginationFilter validFilter, int totalRecords, IUriService uriService, string route)
        //{
        //    var respose = new PagedResponse<List<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize);
        //    var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
        //    int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));
        //    respose.NextPage =
        //        validFilter.PageNumber >= 1 && validFilter.PageNumber < roundedTotalPages
        //        ? uriService.GetPageUri(new PaginationFilter(validFilter.PageNumber + 1, validFilter.PageSize), route)
        //        : null;
        //    respose.PreviousPage =
        //        validFilter.PageNumber - 1 >= 1 && validFilter.PageNumber <= roundedTotalPages
        //        ? uriService.GetPageUri(new PaginationFilter(validFilter.PageNumber - 1, validFilter.PageSize), route)
        //        : null;
        //    respose.FirstPage = uriService.GetPageUri(new PaginationFilter(1, validFilter.PageSize), route);
        //    respose.LastPage = uriService.GetPageUri(new PaginationFilter(roundedTotalPages, validFilter.PageSize), route);
        //    respose.TotalPages = roundedTotalPages;
        //    respose.TotalRecords = totalRecords;
        //    return respose;
        //}

        //internal static object CreatePagedReponse<T>(object pagedData, PaginationFilter validFilter, object totalRecords, object uriService, string route)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
