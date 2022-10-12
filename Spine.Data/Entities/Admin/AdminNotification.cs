using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("AdminNotification")]
    public class AdminNotification
    {
        [Key]
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime ReminderDate { get; set; }
        public TimeSpan ReminderTime { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRead { get; set; }
    }
}
