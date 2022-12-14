using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.BillsPayments
{
    [Index(nameof(CompanyId))]
    public class BillPayment : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public DateTime DateCreated { get; set; }
        public Guid CreatedBy { get; set; }

        // from getbillers payment items
        [MaxLength(10)] public string CategoryId { get; set; }
        public string BillerId { get; set; }
        public bool IsAmountFixed { get; set; }
        [MaxLength(20)] public string PaymentItemId { get; set; }
        [MaxLength(256)] public string PaymentItemName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [MaxLength(50)] public string Code { get; set; }
        [MaxLength(10)] public string CurrencySymbol { get; set; }
        [MaxLength(10)] public string CurrencyCode { get; set; }
        [MaxLength(10)] public string ItemCurrencySymbol { get; set; }
        [MaxLength(50)] public string PaymentCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountToPay { get; set; }

        [MaxLength(50)]
        public string CustomerId { get; set; }

        [MaxLength(13)]
        public string CustomerMobile { get; set; }

        [MaxLength(50)]
        public string CustomerEmail { get; set; }

        [MaxLength(500)]
        public string FullName { get; set; }

        // from send billspayment advice
        /// <summary>
        /// REQUIRED
        /// Length <= 12
        /// Unique requestReference generated on Client’s system and sent in DoTransfer request. 4 digit requestreference prefix will be provided by Interswitch
        /// </summary>
        [MaxLength(256)]
        public string RequestReference { get; set; }

        /// <summary>
        /// Unique Transaction reference generated by Interswitch
        /// </summary>
        [MaxLength(256)]
        public string TransactionReference { get; set; }

        /// <summary>
        /// (Only returned if a billpayment is PIN based)	Biller’s Token
        /// </summary>
        [MaxLength(256)]
        public string MiscData { get; set; }

        /// <summary>
        /// (Only returned if a billpayment is PIN based)	Biller’s Token
        /// </summary>
        [MaxLength(256)]
        public string PIN { get; set; }

        /// <summary>
        /// A response of status is returned (SUCCESS, FAILED, or PENDING)
        /// </summary>
        [MaxLength(256)]
        public string ResponseCodeGrouping { get; set; }

        [MaxLength(256)]
        public string TransactionStatus { get; set; }

    }
}
