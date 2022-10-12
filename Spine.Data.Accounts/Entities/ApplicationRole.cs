using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    [Index(nameof(CompanyId))]
    public class ApplicationRole : IdentityRole<Guid>, IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid CompanyId { get; set; }
        public bool IsSystemDefined { get; set; }
        public bool IsOwnerRole { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }

    }
}
