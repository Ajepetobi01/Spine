using System;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;
using Spine.Common.Helper;

namespace Spine.Data.Entities.Transactions
{
    [Index(nameof(CompanyId))]
    public class LedgerAccount : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountName { get; set; }
        public string GLAccountNo { get; set; }
        public int SerialNo { get; set; } //used to keep the number for the account types, so they can be added serially
        
        public int? ParentId { get; set; }
        
        public GlobalAccountType GlobalAccountType { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public LedgerAccount()
        {
            Id = SequentialGuid.Create();
            CreatedOn = DateTime.Now;
            CreatedBy = Guid.Empty;

        }
    }
    
    
}
