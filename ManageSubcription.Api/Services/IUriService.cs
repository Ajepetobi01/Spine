using ManageSubcription.Api.Helpers;
using Spine.Core.ManageSubcription.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Services
{
    public interface IUriService
    {
        Uri GetPostUri(string postId, string route);
        Uri GetAllPostsUri(PaginationQuery pagination = null);
        //public Uri GetPageUri(PaginationFilter filter, string route);
    }
}
