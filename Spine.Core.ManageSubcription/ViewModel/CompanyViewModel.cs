using Spine.Common.ActionResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
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
        public string IsActive { get; set; }
        public Guid? DeletedBy { get; set; }
        public int BaseCurrencyId { get; set; }
        public string TIN { get; set; }
        public int ID_Subscription { get; set; }
        public string Ref_ReferralCode { get; set; }
        public string ReferralCode { get; set; }
        public bool ImportRecord { get; set; }
        public string BatchNo { get; set; }
        public string BusinessName { get; set; }
        public string BusinessSector { get; set; }
    }

    public class CompanyParam
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherName { get; set; }
        public string PhoneNumber { get; set; }
        public string BusinessName { get; set; }
        public string OperatingSector { get; set; }
        public string BusinessType { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string TIN { get; set; }
        public string Ref_ReferralCode { get; set; }
        public List<BillingVM> Billing { get; set; }
        public List<ShippingVM> Shipping { get; set; }
    }
    public class UserVM
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = false;
        public Guid Role { get; set; }
    }
    public class UpdateUserVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public Guid Role { get; set; }
    }

    public class RoleTransferVM
    {
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
    }
    public class ListUserVM
    {
        public Guid ID_User { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CreatedOn { get; set; }
        public DateTime GetCreatedOn { get; set; }
        public Guid RoleId { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
    }
    public class BillingVM
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string ID_Country { get; set; }
        public string ID_State { get; set; }
        public string PostalCode { get; set; }
        public int ID_Billing { get; set; }
    }
    public class ShippingVM
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string ID_Country { get; set; }
        public string ID_State { get; set; }
        public string PostalCode { get; set; }
        public int ID_Shipping { get; set; }
    }

    public class UpdateCompanyParam
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherName { get; set; }
        public string PhoneNumber { get; set; }
        public string BusinessName { get; set; }
        public string OperatingSector { get; set; }
        public string BusinessType { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string TIN { get; set; }
        public string Ref_ReferralCode { get; set; }
        public List<BillingVM> Billing { get; set; }
        public List<ShippingVM> Shipping { get; set; }
    }

    public class CompanyDTO
    {
        public Guid CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherName { get; set; }
        public string PhoneNumber { get; set; }
        public string BusinessName { get; set; }
        public string OperatingSector { get; set; }
        public string BusinessType { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string TIN { get; set; }
        public string Ref_ReferralCode { get; set; }
    }

    public class StatusModel
    {
        public string id { get; set; }
        public string ReferralCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class CompanyUploadModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherName { get; set; }
        public string PhoneNumber { get; set; }
        public string BusinessName { get; set; }
        public string OperatingSector { get; set; }
        public string BusinessType { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string TIN { get; set; }
        public string Ref_ReferralCode { get; set; }
        public string BillingAddress1 { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingID_Country { get; set; }
        public string BillingID_State { get; set; }
        public string BillingPostalCode { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingID_Country { get; set; }
        public string ShippingID_State { get; set; }
        public string ShippingPostalCode { get; set; }
    }

    public class BirthDayVM
    {
        public string Name { get; set; }
        public DateTime? GetDateofBirth { get; set; }
        public string DateofBirth { get; set; }
    }

    public class GetUploadSubscriber
    {
        public decimal TotalAmount { get; set; }
        public List<SubscriberPlanList> SubscriberPlanList { get; set; }
    }
    public class SubscriberPlanList
    {
        public Guid CompanyId { get; set; }
        public int PlanId { get; set; }
    }

    public class Response : BasicActionResult
    {
        public string LoginToken { get; set; }

        public Response(HttpStatusCode statusCode, string token)
        {
            Status = statusCode;
            LoginToken = token;
        }

        public Response(string message)
        {
            ErrorMessage = message;
            Status = HttpStatusCode.BadRequest;
        }
    }
}
