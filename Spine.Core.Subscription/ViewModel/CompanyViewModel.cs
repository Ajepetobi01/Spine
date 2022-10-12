using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.ViewModel
{
    public class CompanyViewModel
    {
        public Guid User_ID { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public int? EmployeeCount { get; set; }
        public DateTime? DateEstablished { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        public Guid? AddresssId { get; set; }
        public string BusinessType { get; set; }
        public string OperatingSector { get; set; }
        public string Description { get; set; }
        public string LogoId { get; set; }
        public string SocialMediaProfile { get; set; }
        public string Motto { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsVerified { get; set; }
        public Guid? DeletedBy { get; set; }
        public int BaseCurrencyId { get; set; }
        public string TIN { get; set; }
        public int ID_Subscription { get; set; }
        public string Ref_ReferralCode { get; set; }
        public string ReferralCode { get; set; }
        public bool ImportRecord { get; set; }
        public string BatchNo { get; set; }
    }

}
