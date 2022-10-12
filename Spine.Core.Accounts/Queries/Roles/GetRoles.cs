using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Roles
{
    public static class GetRoles
    {
        public class Query : IRequest<List<Model>>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public bool IsSystemDefined { get; set; }
            public string Role { get; set; }

        }

        public class Response : List<Model>
        {
        }

        public class Handler : IRequestHandler<Query, List<Model>>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<List<Model>> Handle(Query request, CancellationToken token)
            {
                var roles = await (from role in _dbContext.Roles.Where(x =>
                                                                        (x.CompanyId == request.CompanyId || x.IsSystemDefined)
                                                                            && !x.IsDeleted)
                                   select new Model
                                   {
                                       Id = role.Id,
                                       IsSystemDefined = role.IsSystemDefined,
                                       Role = role.Name
                                   }).ToListAsync();

                foreach (var item in roles)
                {
                    item.Role = item.Role.GetFirstPart();
                }

                return roles;
            }
        }

    }
}
