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
    public class OpeningBalance : IEntity, ICompany
    {
        public Guid Id { get; set; }
        
        public int SerialNo { get; set; }
        public Guid CompanyId { get; set; }
        
        public Guid LedgerAccountId { get; set; }
        public Guid BookClosingId { get; set; }
        
        public DateTime ValueDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; }
       
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public DateTime TransactionDate { get; set; }
        public Guid CreatedBy { get; set; }

    }
}
