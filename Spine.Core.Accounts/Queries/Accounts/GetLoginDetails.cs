using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Core.Subscription.ViewModel;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Accounts
{
    public static class GetLoginDetail
    {
        public class Query : IRequest<Response>
        {
            public Guid UserId { get; set; }
            
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public List<string> Permissions { get; set; }
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
                var userRoleId = await _dbContext.Users.Where(x => x.Id == request.UserId).Select(x => x.RoleId)
                    .SingleOrDefaultAsync();

                var permissions = new List<string>();
                if (userRoleId != Guid.Empty)
                {
                    permissions = await _dbContext.RoleClaims.Where(x =>
                            x.ClaimType == Constants.PermissionClaim && x.RoleId == userRoleId)
                        .Select(x => x.ClaimValue).ToListAsync();
                }
                
                return new Response
                {
                    Permissions = permissions
                };
            }
            
        }

    }
}
