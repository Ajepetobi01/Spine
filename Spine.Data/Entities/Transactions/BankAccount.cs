using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Transactions
{
    [Index(nameof(CompanyId))]
    public class BankAccount : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(256)]
        public string BankName { get; set; }
        [MaxLength(10)]
        public string BankCode { get; set; }
        [MaxLength(30)]
        public string AccountType { get; set; }
        [MaxLength(256)]
        public string AccountName { get; set; }
        [MaxLength(20)]
        public string AccountNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        public Guid LedgerAccountId { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }

        public bool IsCash { get; set; }
        [MaxLength(10)]
        public string Currency { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }

        public bool IsActive { get; set; }
        public DateTime? DateDeactivated { get; set; }
        public BankAccountIntegrationProvider IntegrationProvider { get; set; }

        [MaxLength(256)]
        public string AccountCode { get; set; }
        /// <summary>
        /// Used for integration with Mono
        /// </summary>
        [MaxLength(256)]
        public string AccountId { get; set; }
    }
}
