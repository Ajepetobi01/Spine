using System;
using System.ComponentModel.DataAnnotations;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
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

        [Obsolete("Not used", false)]
        public Guid? AddresssId { get; set; }

        [MaxLength(50)]
        public string BusinessType { get; set; }
        [MaxLength(100)]
        public string OperatingSector { get; set; }

        public string Description { get; set; }
        [MaxLength(256)]
        public string LogoId { get; set; }
        
        [Obsolete("no longer used")]
        public string SocialMediaProfile { get; set; }
        
        [MaxLength(500)]
        public string FacebookProfile { get; set; }
        [MaxLength(500)]
        public string InstagramProfile { get; set; }
        [MaxLength(500)]
        public string TwitterProfile { get; set; }
        
        [MaxLength(256)]
        public string Motto { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsVerified { get; set; }
        public Guid? DeletedBy { get; set; }
        public int BaseCurrencyId { get; set; }
        public int ID_Subscription { get; set; }
        public string TIN { get; set; }
        public string ReferralCode { get; set; }
        public string Ref_ReferralCode { get; set; }
        public bool ImportRecord { get; set; }
        public string BatchNo { get; set; }
        
        public string CacRegNo { get; set; }
    }
}
