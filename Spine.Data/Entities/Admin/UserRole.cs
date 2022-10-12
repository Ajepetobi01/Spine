using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("UserRole")]
    public class UserRole
    {
        [Key]
        public int ID_UserRole { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
    }
}
