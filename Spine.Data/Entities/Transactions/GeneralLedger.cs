using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Transactions
{
    [Index(nameof(CompanyId), nameof(LedgerAccountId))]
    [Index(nameof(ValueDate))]
    public class GeneralLedger : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? LocationId { get; set; }
        
        public Guid LedgerAccountId { get; set; }
        
        public int AccountingPeriodId { get; set; }
        
        public DateTime ValueDate { get; set; }
        [MaxLength(2000)]
        public string Narration { get; set; }
        /// <summary>
        /// Use both ReferenceNo and TransactionGroupId to group entries,
        /// ReferenceNo will be the same for VendorPayments, InvoicePayments and Transactions since the serial no reset daily
        /// </summary>
        [MaxLength(50)]
        public string ReferenceNo { get; set; }
        public Guid TransactionGroupId { get; set; }

        public Guid? CustomerId { get; set; }
        public Guid? VendorId { get; set; }
        
        /// <summary>
        /// Could be GoodsReceivedId, InvoiceId, InventoryId
        /// </summary>
        public Guid? OrderId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }
       
        public TransactionType Type { get; set; }
        
        public int BaseCurrencyId { get; set; }
        public int ForexCurrencyId { get; set; }
        /// <summary>
        /// Forex amounts will be 0 if the currencyId is same as baseCurrencyId
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ForexDebitAmount { get; set; }
        /// <summary>
        /// Forex amounts will be 0 if the currencyId is same as baseCurrencyId
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ForexCreditAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExchangeRate { get; set; }
        
        public DateTime TransactionDate { get; set; }
        public Guid CreatedBy { get; set; }
        
        public bool IsClosingEntry { get; set; }
        public Guid? BookClosingId { get; set; }

    }
}
