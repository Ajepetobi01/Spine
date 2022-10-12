using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Spine.Common.Models;
using Spine.Core.Accounts.Helpers;

namespace Spine.Core.Accounts.Queries.Common
{
    public static class GetAllPermissions
    {
        public class Query : IRequest<List<Model>>
        {
        }

        public class Model
        {
            public List<PermissionModel> Permissions { get; set; }
            public string GroupName { get; set; }

        }

        public class Response : List<Model>
        {
        }


        public class Handler : IRequestHandler<Query, List<Model>>
        {
            private readonly IPermissionHelper _permissionHelper;

            public Handler(IPermissionHelper permissionHelper)
            {
                _permissionHelper = permissionHelper;
            }

            public async Task<List<Model>> Handle(Query request, CancellationToken token)
            {
                var permissions = _permissionHelper.GetAllPermissions();

                var grouped = permissions.GroupBy(x => x.GroupName).Select(x => new Model
                {
                    GroupName = x.Key,
                    Permissions = x.ToList()
                }).ToList();

                return grouped;
            }
        }

    }
}
