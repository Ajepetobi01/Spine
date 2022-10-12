using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId), nameof(UserId))]
    public class Notification : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public Guid UserId { get; set; }
        public NotificationCategory Category { get; set; }
        public Guid EntityId { get; set; }

        [MaxLength(3000)]
        public string Message { get; set; }

        public bool IsRead { get; set; }
        public bool IsCleared { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }

    }
}
