using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Core.Accounts.Helpers;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Queries.Roles
{
    public static class GetRolePermissions
    {
        public class Query : IRequest<List<GroupedModel>>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid RoleId { get; set; }
        }

        public class Model
        {
            public string GroupName { get; set; }
            public string Description { get; set; }
            public Permissions Permission { get; set; }
            public List<Permissions> Dependencies { get; set; }
            public bool Granted { get; set; }
        }

        public class GroupedModel
        {
            public string GroupName { get; set; }
            public List<Model> Permissions { get; set; }

        }

        public class Response : List<GroupedModel>
        {
        }


        public class Handler : IRequestHandler<Query, List<GroupedModel>>
        {
            private readonly SpineContext _dbContext;
            private readonly IPermissionHelper _permissionHelper;
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly IMapper _mapper;

            public Handler(SpineContext dbContext, IPermissionHelper permissionHelper, RoleManager<ApplicationRole> roleManager, IMapper mapper)
            {
                _dbContext = dbContext;
                _permissionHelper = permissionHelper;
                _roleManager = roleManager;
                _mapper = mapper;
            }

            public async Task<List<GroupedModel>> Handle(Query request, CancellationToken token)
            {
                var allPermissions = _mapper.Map<List<Model>>(_permissionHelper.GetAllPermissions());

                var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
                var claims = await _roleManager.GetClaimsAsync(role);
                var allClaimValues = allPermissions.Select(a => a.Permission.GetStringValue()).ToList();
                var roleClaimValues = claims.Select(a => a.Value).ToList();
                var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();

                foreach (var item in allPermissions)
                {
                    if (authorizedClaims.Contains(item.Permission.GetStringValue()))
                    {
                        item.Granted = true;
                    }
                }

                var grouped = allPermissions.GroupBy(x => x.GroupName).Select(x => new GroupedModel
                {
                    GroupName = x.Key,
                    Permissions = x.ToList()
                }).ToList();

                return grouped;
            }
        }
    }
}
