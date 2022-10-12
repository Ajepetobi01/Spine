using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("ResourcesAccess")]
    public class ResourcesAccess
    {
        [Key]
        public Guid ID_ResourcesAccess { get; set; }
        public Guid ID_Role { get; set; }
        public Guid ID_Permission { get; set; }
    }
}
