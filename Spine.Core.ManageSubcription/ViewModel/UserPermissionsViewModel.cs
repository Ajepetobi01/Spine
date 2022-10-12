using Spine.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class UserPermissionsViewModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }
    public class PermissionsViewModel
    {
        public string Name { get; set; }
    }

    public class ModelPermissions
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
        public List<ModelPermissions> Permissions { get; set; }

    }

    public class UserPermission
    {
        public string Permission { get; set; }
        public bool Granted { get; set; }
    }

    public class RoleAccess
    {
        public string permissionId { get; set; }
        public string Permission { get; set; }
        public bool Granted { get; set; }
        public Guid RoleId { get; set; }
    }
}
