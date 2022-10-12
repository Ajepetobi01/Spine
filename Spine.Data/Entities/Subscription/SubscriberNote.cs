using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Subscription
{
    [Table("SubscriberNote")]
    public class SubscriberNote
    {
        [Key]
        public int ID_Note { get; set; }
        public Guid CompanyId { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
