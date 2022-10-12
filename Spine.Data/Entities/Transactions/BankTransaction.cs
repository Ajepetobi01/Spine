using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Transactions
{
    [Index(nameof(CompanyId))]
    [Index(nameof(BankAccountId))]
    public class BankTransaction : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid BankAccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        [MaxLength(2000)]
        public string Description { get; set; }
        [MaxLength(50)]
        public string ReferenceNo { get; set; }
        [MaxLength(50)]
        public string UserReferenceNo { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        public TransactionStatus Status { get; set; }

        [MaxLength(256)]
        public string Payee { get; set; }
        [MaxLength(100)]
        public string ChequeNo { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}
