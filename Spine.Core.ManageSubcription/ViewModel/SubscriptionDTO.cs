using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class SubscriptionDTO
    {
        public SubscriptionDTO()
        {
            this.Billing = new List<BillingVM>();
            this.Shipping = new List<ShippingVM>();
        }
        public Guid ID_Company { get; set; }
        public int? ID_Subscription { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PlanName { get; set; }
        public string BusinessType { get; set; }
        public string SubscriptionDate { get; set; }
        public string TransactionRef { get; set; }
        public string ExpiredDate { get; set; }
        public DateTime? GetSubscriptionDate { get; set; }
        public DateTime? GetExpiredDate { get; set; }
        public DateTime? GetCapturedDate { get; set; }
        public DateTime? GetDateOfBirth { get; set; }
        public string DateOfBirth { get; set; }
        public string Month { get; set; }
        public int? DaysToExpired { get; set; }
        public string ExpiredMessage { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string Status { get; set; }
        public string ReferralCode { get; set; }
        public string Ref_ReferralCode { get; set; }
        public bool ImportRecord { get; set; }
        public string BatchNo { get; set; }
        public string CapturedDate { get; set; }
        public string BusinessName { get; set; }
        public string BusinessSector { get; set; }
        public string Gender { get; set; }
        public string TIN { get; set; }
        public List<BillingVM> Billing { get; set; }
        public List<ShippingVM> Shipping { get; set; }
    }
    public class ReferralSubscriptionDTO
    {
        public Guid ID_Company { get; set; }
        public int? ID_Subscription { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PlanName { get; set; }
        public string BusinessType { get; set; }
        public string SubscriptionDate { get; set; }
        public string TransactionRef { get; set; }
        public string ExpiredDate { get; set; }
        public DateTime? GetSubscriptionDate { get; set; }
        public DateTime? GetExpiredDate { get; set; }
        public DateTime? GetCapturedDate { get; set; }
        public int? DaysToExpired { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string Status { get; set; }
        public string ReferralCode { get; set; }
        public string Ref_ReferralCode { get; set; }
        public bool ImportRecord { get; set; }
        public string BatchNo { get; set; }
        public string CapturedDate { get; set; }
        public int NoofReferral { get; set; }
        public string ExpiredMessage { get; set; }
        public string BusinessName { get; set; }
        public string BusinessSector { get; set; }
        public string Gender { get; set; }
        public string TIN { get; set; }
    }

    public class DashBoardViewModel
    {
        public DashBoardViewModel()
        {
            this.PlanStatistics = new PlanStatistics();
        }
        public decimal TotalTransactionAmount { get; set; }
        public int ActiveSubscriber { get; set; }
        public string ActiveSubscriberpersentage { get; set; }
        public bool? isActiveSubscriberpersentageLess { get; set; }
        //public decimal LastMonthActiveSubscriberpersentage { get; set; }
        //public decimal CurrentMonthActiveSubscriberpersentage { get; set; }
        public int InActiveSubscriber { get; set; }
        public string InActiveSubscriberpersentage { get; set; }
        public bool? isInActiveSubscriberpersentageLess { get; set; }
        //public decimal LastMonthInActiveSubscriberpersentage { get; set; }
        //public decimal CurrentMonthInActiveSubscriberpersentage { get; set; }
        public int SubscriberOnTrial { get; set; }
        public string SubscriberOnTrialpersentage { get; set; }
        public bool? isSubscriberOnTrialpersentageLess { get; set; }
        //public decimal LastMonthSubscriberOnTrialpersentage { get; set; }
        //public decimal CurrentMonthSubscriberOnTrialpersentage { get; set; }
        public int TotalSubscriber { get; set; }
        public int TotalUnSubscriber { get; set; }
        public string TotalSubscriberpersentage { get; set; }
        public bool? isTotalSubscriberpersentageLess { get; set; }
        //public decimal LastMonthTotalSubscriberpersentage { get; set; }
        //public decimal CurrentMonthTotalSubscriberpersentage { get; set; }
        public PlanStatistics PlanStatistics { get; set; }
        public List<BirthDayVM> SubscritionsBirthDate { get; set; }
        public List<AdminNotiVM> Notification { get; set; }
        public List<OnboardingChartVM> OnboardingChart { get; set; }
    }
    public class PlanStatistics
    {
        public int TotalPlanCount { get; set; }
        public List<SubscriberPlan> SubscritionPlanStatistics { get; set; }
    }
    public class OnboardingChartVM
    {
        public string Key { get; set; }
        public int Valus { get; set; }
    }
    public class AdminNotiVM
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string ReminderDate { get; set; }
    }

    public class DashBoardSubscriptionQuery
    {
        public Guid ID_Company { get; set; }
        public int? ID_Subscription { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PlanName { get; set; }
        public string BusinessType { get; set; }
        public string SubscriptionDate { get; set; }
        public string TransactionRef { get; set; }
        public string ExpiredDate { get; set; }
        public DateTime? GetSubscriptionDate { get; set; }
        public DateTime? GetExpiredDate { get; set; }
        public DateTime? GetCapturedDate { get; set; }
        public DateTime? GetDateOfBirth { get; set; }
        public string DateOfBirth { get; set; }
        public string Month { get; set; }
        public int? DaysToExpired { get; set; }
        public string ExpiredMessage { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string Status { get; set; }
        public string ReferralCode { get; set; }
        public string Ref_ReferralCode { get; set; }
        public bool ImportRecord { get; set; }
        public string BatchNo { get; set; }
        public string CapturedDate { get; set; }
        public bool IsActive { get; set; }
        public bool PlanIsFree { get; set; }
        public int ID_Plan { get; set; }
    }
}
