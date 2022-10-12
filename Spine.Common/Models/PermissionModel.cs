using System.Collections.Generic;
using Spine.Common.Enums;

namespace Spine.Common.Models
{
    public class PermissionModel
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        public Permissions Permission { get; set; }
        public List<Permissions> Dependencies { get; set; }

    }
}
