using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("Resources")]
    public class Resources
    {
        [Key]
        public Guid ID_Resources { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
