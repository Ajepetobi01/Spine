using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Spine.Common.Data.Interfaces;
using Spine.Common.Models;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Accounts.Queries.Roles
{
    public static class GetUsersInRole
    {
        public class Query : IRequest<Response>, IPagedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid RoleId { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }

        }

        public class Response : PagedResult<Model>
        {
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var query = (from user in _dbContext.Users.Where(x => x.CompanyId == request.CompanyId && x.RoleId == request.RoleId && !x.IsDeleted)
                             select new Model
                             {
                                 Id = user.Id,
                                 Email = user.Email,
                                 Name = user.FullName
                             });

                var data = await query.OrderBy(x => x.Name).ToPageResultsAsync<Model, Response>(request);

                return data;

            }
        }

    }
}
