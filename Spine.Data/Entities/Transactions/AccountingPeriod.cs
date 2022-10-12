using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Transactions
{
    [Index(nameof(CompanyId), nameof(StartDate), nameof(EndDate))]
    public class AccountingPeriod : ICompany
    {
        [Key]
        public int Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid BookClosingId { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public string PeriodCode { get; set; }
       
        public DateTime? BookClosedDate { get; set; }
        public AccountingMethod? AccountingMethod { get; set; }
        public bool IsClosed { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
    }
}
