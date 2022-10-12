using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Transactions
{
    [Index(nameof(CompanyId))]
    [Index(nameof(PostingDate))]
    public class JournalPosting : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid JournalId { get; set; }
        public Guid CompanyId { get; set; }
        
        public DateTime PostingDate { get; set; }
        
        public Guid? ProductId { get; set; }
        [MaxLength(256)]
        public string ProductName { get; set; }
        [MaxLength(2000)]
        public string Description { get; set; }
        
        [MaxLength(50)]
        public string JournalNo { get; set; }
        
        public bool IsCashBased { get; set; }
        
        public int CurrencyId { get; set; }
        public int BaseCurrencyId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal RateToBaseCurrency { get; set; }
        
        [MaxLength(2000)]
        public string ItemDescription { get; set; }
        public Guid LedgerAccountId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }

    }
}
