using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId))]
    public class CompanyCurrency : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public int OldCurrencyId { get; set; }
        public int CurrencyId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }

        public DateTime ActivatedOn { get; set; }
        public Guid ActivatedBy { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public Guid? DeactivatedBy { get; set; }

    }
}
