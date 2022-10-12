using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Vendor
{
    [Index(nameof(CompanyId), nameof(VendorId))]
    public class VendorContactPerson : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid VendorId { get; set; }

        [MaxLength(256)]
        public string Role { get; set; }
        [MaxLength(256)]
        public string FullName { get; set; }
        [MaxLength(256)]
        public string EmailAddress { get; set; }
        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
