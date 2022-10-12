using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId))]
    public class ApplicationUser : IdentityUser<Guid>, IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid CompanyId { get; set; }
        [MaxLength(256)]
        public string FullName { get; set; }
        [MaxLength(256)]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid RoleId { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsBusinessOwner { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
