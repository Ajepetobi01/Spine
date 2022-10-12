using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Subscription
{
    [Table("SubscriberShipping")]
    public class SubscriberShipping
    {
        [Key]
        public int ID_Shipping { get; set; }
        public Guid ID_Company { get; set; }
        [MaxLength(50)]
        public string Address1 { get; set; }
        [MaxLength(50)]
        public string Address2 { get; set; }
        [MaxLength(50)]
        public string ID_Country { get; set; }
        [MaxLength(50)]
        public string ID_State { get; set; }
        [MaxLength(10)]
        public string PostalCode { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
