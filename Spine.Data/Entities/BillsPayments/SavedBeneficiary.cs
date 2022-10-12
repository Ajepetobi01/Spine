using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.BillsPayments
{
    [Index(nameof(CompanyId))]
    public class SavedBeneficiary : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(20)]
        public string AccountNo { get; set; }

        [MaxLength(256)]
        public string AccountName { get; set; }

        [MaxLength(256)]
        public string BankName { get; set; }

        [MaxLength(10)]
        public string BankCode { get; set; }

        public DateTime DateCreated { get; set; }
        public Guid CreatedBy { get; set; }

    }
}
