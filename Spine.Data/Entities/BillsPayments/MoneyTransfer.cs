using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.BillsPayments
{
    [Index(nameof(CompanyId))]
    public class MoneyTransfer : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public Guid AccountFrom { get; set; }
        public Guid? TransactionTagId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(20)]
        public string RecipientAccountNo { get; set; }

        [MaxLength(256)]
        public string RecipientAccountName { get; set; }

        [MaxLength(256)]
        public string RecipientBank { get; set; }

        [MaxLength(10)]
        public string BankCode { get; set; }

        [MaxLength(500)]
        public string Remark { get; set; }

        [MaxLength(256)]
        public string RefNo { get; set; }

        public DateTime DateCreated { get; set; }
        public Guid CreatedBy { get; set; }

    }
}
