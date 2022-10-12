using System;
using System.ComponentModel.DataAnnotations;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Accounts.Entities
{
    public class Company : IEntity, IAuditable, IDeletable
    {
        public Guid Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        [MaxLength(30)]
        public string PhoneNumber { get; set; }
        [MaxLength(256)]
        public string Website { get; set; }

        public int? EmployeeCount { get; set; }
        public DateTime? DateEstablished { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }
        [MaxLength(100)]
        public string City { get; set; }

        public Guid? AddresssId { get; set; }

        [MaxLength(50)]
        public string BusinessType { get; set; }
        //   [MaxLength(100)]
        public string OperatingSector { get; set; } // will contain comma or colon separated list of opearting sectors

        public string Description { get; set; }
        [MaxLength(256)]
        public string LogoId { get; set; }
        public string SocialMediaProfile { get; set; }
        [MaxLength(256)]
        public string Motto { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
