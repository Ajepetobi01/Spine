using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Vendor
{
    [Index(nameof(CompanyId), nameof(VendorId))]
    public class VendorBankAccount : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid VendorId { get; set; }

        [MaxLength(256)]
        public string BankName { get; set; }
        [MaxLength(10)]
        public string BankCode { get; set; }
        [MaxLength(256)]
        public string AccountName { get; set; }
        [MaxLength(20)]
        public string AccountNumber { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
