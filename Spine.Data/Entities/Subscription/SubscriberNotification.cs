using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Subscription
{
    [Table("SubscriberNotification")]
    public class SubscriberNotification
    {
        [Key]
        public Guid ID { get; set; }
        public Guid CompanyId { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime? TimeRead { get; set; }
        public string Comments { get; set; }
        public DateTime ReminderDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public Guid? NotificationPath { get; set; }
    }
}
