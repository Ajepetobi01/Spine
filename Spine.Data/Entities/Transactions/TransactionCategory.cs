using System;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Transactions
{
    //categories
    [Index(nameof(CompanyId))]
    public class TransactionCategory : IEntity, ICompany, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public string Name { get; set; }
        public bool IsInflow { get; set; }
        public bool IsDefault { get; set; }

        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
