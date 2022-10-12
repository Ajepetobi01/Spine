using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId))]
    public class ApplicationRole : IdentityRole<Guid>, IEntity, IAuditable, IDeletable
    {
        public Guid? CompanyId { get; set; }
        /// <summary>
        /// this should be true if it's being added by an admin and should apply to subscribers
        /// </summary>
        public bool IsSystemDefined { get; set; }
        public bool IsOwnerRole { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool? Expose { get; set; }
        public Guid? DeletedBy { get; set; }

    }
}
