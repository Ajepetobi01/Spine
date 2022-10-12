using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    [Index(nameof(CompanyId))]
    public class Customer : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        [MaxLength(30)]
        public string PhoneNumber { get; set; }

        [MaxLength(256)]
        public string BusinessName { get; set; }
        [MaxLength(50)]
        public string BusinessType { get; set; }
        [MaxLength(100)]
        public string OperatingSector { get; set; }

        public Guid BillingAddress { get; set; }
        public Guid ShippingAddress { get; set; }

        public double AmountReceived { get; set; }
        public double AmountOwed { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
