using ManageSubcription.Api.Helpers;
using Microsoft.AspNetCore.WebUtilities;
using Spine.Core.ManageSubcription.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Services
{
    public class UriService : IUriService
    {
        private readonly string _baseUri;
        public UriService(string baseUri)
        {
            _baseUri = baseUri;
        }
        public Uri GetPostUri(string postId, string route)
        {
            return new Uri(string.Concat(_baseUri, route));//new Uri(_baseUri + ApiRoutes.Posts.Get.Replace());
        }
        public Uri GetAllPostsUri(PaginationQuery pagination = null)
        {
            var uri = new Uri(_baseUri);

            if(pagination == null)
            {
                return uri;
            }

            var modifiedUrl = QueryHelpers.AddQueryString(_baseUri, name: "pageNumber", value: pagination.PageNumber.ToString());
            modifiedUrl = QueryHelpers.AddQueryString(modifiedUrl, name: "pageSize", value: pagination.PageNumber.ToString());

            return new Uri(modifiedUrl);
        }

        
        //private readonly string _baseUri;
        //public UriService(string baseUri)
        //{
        //    _baseUri = baseUri;
        //}
        //public Uri GetPageUri(PaginationFilter filter, string route)
        //{
        //    var _enpointUri = new Uri(string.Concat(_baseUri, route));
        //    var modifiedUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "pageNumber", filter.PageNumber.ToString());
        //    modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());
        //    return new Uri(modifiedUri);
        //}

    }
}
