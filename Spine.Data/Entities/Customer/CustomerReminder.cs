using System;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId), nameof(CustomerId))]
    public class CustomerReminder : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid CustomerId { get; set; }

        public string Description { get; set; }
        public DateTime ReminderDate { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
