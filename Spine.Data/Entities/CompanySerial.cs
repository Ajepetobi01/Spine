using System;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId))]
    public class CompanySerial : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public int LastUsedPO { get; set; }
        public int LastUsedGR { get; set; }
        public int LastUsedJournal { get; set; }
        public int LastUsedPeriodNo { get; set; }
        
        public DateTime CurrentDate { get; set; }
        public int LastUsedTransactionNo { get; set; }

    }
}
