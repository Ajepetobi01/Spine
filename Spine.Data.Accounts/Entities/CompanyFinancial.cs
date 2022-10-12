using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    public class CompanyFinancial : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public int Year { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedTurnOver { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LastTurnOver { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LastProfitBeforeTax { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedProfitBeforeTax { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LastProfit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedProfit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LastEarningBeforeInterest { get; set; }
        public bool? HasCreditDebitTerminal { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}
