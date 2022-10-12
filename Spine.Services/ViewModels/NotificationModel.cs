using System;

namespace Spine.Services.ViewModels
{
    public class NotificationModel
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
