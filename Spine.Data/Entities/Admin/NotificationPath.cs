using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("NotificationPath")]
    public class NotificationPath
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string PathDesscription { get; set; }
        public bool IsActive { get; set; }
    }
}
