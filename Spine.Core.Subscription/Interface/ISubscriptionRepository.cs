using Spine.Core.Subscription.ViewModel;
using Spine.Data.Entities;
using Spine.Data.Entities.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.Interface
{
    public interface ISubscriptionRepository
    {
        bool SaveAll();
        IQueryable<PlanViewModel> GetPlans();
        bool AddPlan(PlanViewModel model);
        bool UpdatePlan(PlanViewModel model);
        bool DeletePlan(int id);
        Task<bool> AddOnlineDepositOrder(CompanySubscription model);
        Task<bool> UpdateOnlineDepositOrder(string reference);
        bool ReQueryPayment(string reference);
        IQueryable<CompanySubscription> GetTransactionByCompanyId(int subscriptionId);
        IQueryable<CompanyViewModel> CompanyById(Guid companyId);
        IQueryable<CompanyViewModel> QueryCompany();
        //Task<bool> UpdateCompany(CompanyDTO model);
        bool isValidReferralCode(string Ref_referralCode);
        Response PromoCode(PromoCodeViewModel model);
        decimal ValidPromoCodeAmount(Guid promocodeId);
        Task<bool> UpdatePromoCodeTransactionRef(Guid promocodeId, string transactionRef);
        Task<bool> UpdatePromoCode(Guid? promocodeId = null, string transactionRef = null);
        bool isPromoCodeValid(Guid promocodeId);
        Response ReferralCode(ReferralViewModel model);
        decimal ValidReferralCodeAmount(Guid referralcodeId);
        Task<bool> UpdateReferralCodeTransactionRef(Guid referralcodeId, string transactionRef);
        Task<bool> UpdateReferralCode(Guid? referralcodeId = null, string transactionRef = null);
        bool isReferralCodeValid(Guid referralcodeId);
        bool isReferralCodeEnable();
        bool isPromoCodeEnable();
        Task<CompanySubscriptionDTO> GetSubscription(string reference);
    }
}
