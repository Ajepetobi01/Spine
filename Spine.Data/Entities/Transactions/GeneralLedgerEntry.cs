using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Transactions
{
    [Index(nameof(CompanyId))]
    [Index(nameof(ValueDate))]
    public class GeneralLedgerEntry : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? LocationId { get; set; }
        
        public Guid DebitLedgerAccountId { get; set; }
        public Guid CreditLedgerAccountId { get; set; }
        
        public int AccountingPeriodId { get; set; }
        
        public DateTime ValueDate { get; set; }
        [MaxLength(2000)]
        public string Narration { get; set; }
        [MaxLength(50)]
        public string ReferenceNo { get; set; }
        public Guid TransactionGroupId { get; set; }

        public Guid? CustomerId { get; set; }
        public Guid? VendorId { get; set; }
        
        /// <summary>
        /// Could be Purchase orderId, InvoiceId, InventoryId
        /// </summary>
        public Guid? OrderId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
       
        public TransactionType Type { get; set; }

        public int CurrencyId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ForexAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExchangeRate { get; set; }
        
        public DateTime TransactionDate { get; set; }
        public Guid CreatedBy { get; set; }

    }
}
