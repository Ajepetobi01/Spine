using System;
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
    public static class GetRole
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public string Role { get; set; }

            public bool IsSystemDefined { get; set; }
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
                var item = await (from role in _dbContext.Roles.Where(x =>
                                                                        (x.CompanyId == request.CompanyId || x.IsSystemDefined)
                                                                        && x.Id == request.Id
                                                                            && !x.IsDeleted)
                                  select new Response
                                  {
                                      Id = role.Id,
                                      IsSystemDefined = role.IsSystemDefined,
                                      Role = role.Name
                                  }).SingleOrDefaultAsync();

                if (item != null)
                {
                    item.Role = item.Role.GetFirstPart();
                }

                return item;

            }
        }

    }
}
