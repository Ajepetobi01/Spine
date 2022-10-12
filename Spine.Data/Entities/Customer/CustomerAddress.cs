using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId), nameof(CustomerId))]
    public class CustomerAddress : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid CustomerId { get; set; }

        [MaxLength(256)]
        public string AddressLine1 { get; set; }
        [MaxLength(256)]
        public string AddressLine2 { get; set; }
        [MaxLength(256)]
        public string City { get; set; }
        [MaxLength(256)]
        public string State { get; set; }
        [MaxLength(256)]
        public string Country { get; set; }
        [MaxLength(20)]
        public string PostalCode { get; set; }
        
        public bool IsPrimary { get; set; }
        public bool IsBilling { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
