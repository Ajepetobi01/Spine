using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Interface
{
    public interface IManageSubcriptionRepository
    {
        bool SaveAll();
        //subscriber module
        IQueryable<SubscriptionDTO> GetUnSubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null);
        IQueryable<SubscriptionDTO> GetUnSubacriberByCompayId(Guid companyId);
        IQueryable<SubscriptionDTO> GetSubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null);
        IQueryable<SubscriptionDTO> GetAlmostExpirySubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null);
        IQueryable<SubscriptionDTO> GetOnboradingAnalysis(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null);
        IQueryable<SubscriptionDTO> GetImportedSubscribers(string BatchNo, GetAllPostFilter filter = null, PaginationFilter paginationFilter = null);
        IQueryable<ReferralSubscriptionDTO> GetReferralSubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null);
        IQueryable<CompanyViewModel> CompanyById(Guid companyId);
        IQueryable<SubscriptionDTO> GetSubacriberByCompayId(Guid companyId);
        DashBoardViewModel DashboardList();
        int TotalNumberOfSubscribers();
        int TotalNumberOfSubscriberByReferralCode(string referralcode);
        int TotalNumberOfActiveSubscribers();
        int TotalNumberOfActiveSubscriberByReferralCode(string referralcode);
        int TotalNumberOfInActiveSubscribers();
        int TotalNumberOfInActiveSubscriberByReferralCode(string referralcode);
        decimal TotalTransaction();
        IQueryable<BirthDayVM> GetBirthDayList(PostBirthFilter filter = null, PaginationFilter paginationFilter = null);
        IEnumerable<SubscriberPlan> TotalNumberOfSubscribersByPlan();
        Task<StatusModel> SaveSubcribers(List<CompanyUploadModel> subcribers, Guid CompanyId, Guid UserId);
        Task<StatusModel> SaveSubcriber(CompanyParam model, Guid CompanyId, Guid UserId);
        StatusModel UpdateSubcriber(Guid CompanyId, UpdateCompanyParam param, Guid UserId);
        Task<bool> SaveUploadSubscription(GetUploadSubscriber model, Guid CompanyId, Guid UserId);
        Task<bool> SaveUploadSubscriptionNoPayment(GetUploadSubscriber model, Guid CompanyId, Guid UserId);
        IQueryable<SubscriberBillingViewModel> GetBillings();
        IQueryable<SubscriberBillingViewModel> GetBillingsById(int id);
        bool CreateBilling(SubscriberBillingDTO model, Guid CompanyId, Guid UserId);
        bool UpdateBilling(SubscriberBillingDTO model, Guid CompanyId, Guid UserId);
        bool DeleteBilling(int id, Guid CompanyId, Guid UserId);
        IQueryable<SubscriberShippingViewModel> GetShipping();
        IQueryable<SubscriberShippingViewModel> GetShippingById(int id);
        bool CreateShipping(SubscriberShippingDTO model, Guid CompanyId, Guid UserId);
        bool UpdateShipping(SubscriberShippingDTO model, Guid CompanyId, Guid UserId);
        bool DeleteShipping(int id, Guid CompanyId, Guid UserId);
        bool EnableSubscriber(Guid companyId);
        IQueryable<SubscriberNoteViewModel> GetNotes();
        IQueryable<SubscriberNoteViewModel> GetNote(int noteId);
        IQueryable<SubscriberNoteViewModel> GetCompanyNote(Guid companyId);
        bool AddNote(NoteRequest param, Guid CompanyId, Guid UserId);
        bool UpdateNote(NoteRequest param, Guid CompanyId, Guid UserId);
        bool DeleteNote(int id, Guid CompanyId, Guid UserId);
        IQueryable<SubscriberNotificationViewModel> GetNotefications(FilterAdminNotification filter = null, PaginationFilter paginationFilter = null);
        IQueryable<SubscriberNotificationViewModel> GetNotefication(Guid notificationId);
        IQueryable<SubscriberNotificationViewModel> GetCompanyNotefication(Guid companyId);
        Task<bool> AddNotification(NotificationRequest param, Guid CompanyId, Guid UserId);
        Task<bool> UpdateNotification(NotificationRequest param, Guid CompanyId, Guid UserId);
        int CountNotifications();
        bool DeleteNotification(int id, Guid CompanyId, Guid UserId);
        IQueryable<PlanViewModel> GetPlans(FilterPlan filter = null, PaginationFilter paginationFilter = null);
        IQueryable<PlanViewModel> GetPlanById(int Id);
        bool AddPlan(AddPlanViewModel model, Guid CompanyId, Guid UserId);
        bool UpdatePlan(int PlanId, AddPlanViewModel model, Guid CompanyId, Guid UserId);
        bool DeletePlan(int id, Guid CompanyId, Guid UserId);
        bool TogglePlan(int Id);


        List<RoleClaimsViewModel> GetPermissions();
        Task<UserRoleViewModel> GetRoleById(Guid roleId);
        Task<IQueryable<ListRoleViewModel>> GetRoles(RolePostFilter filter = null);
        Task<IQueryable<GetDropDowmRole>> GetSlimRoles();
        Task<IQueryable<GetDropDowmRole>> GetSubscriberRoles();
        Task<IQueryable<GetDropDowmRole>> GetAdminRoles();
        Task<bool> AddRole(RoleViewModel model, Guid CompanyId, Guid UserId);
        Task<bool> UpdateRole(Guid Id, RoleViewModel model, Guid CompanyId, Guid UserId);
        Task<bool> DeleteRole(Guid id, Guid CompanyId, Guid UserId);
        bool ToggleRole(Guid Id);
        Task<Response> CreateUser(UserVM model, Guid CompanyId, Guid UserId);
        bool UpdateUser(Guid Id, UpdateUserVM model, Guid CompanyId, Guid UserId);
        bool RoleTransfer(RoleTransferVM model, Guid CompanyId, Guid UserId);
        IQueryable<ListUserVM> GetUsers(PostUserFilter filter = null, PaginationFilter paginationFilter = null);
        IQueryable<ListUserVM> GetUser(Guid Id);
        bool DeleteUser(Guid id, Guid CompanyId, Guid UserId);
        bool ToggleUser(Guid Id);
        IQueryable<AuditLogViewModel> GetAuditLog(FilterAuditLog filter = null, PaginationFilter paginationFilter = null);
        IQueryable<DocumentTemplateViewModel> GetTemplates();
        bool CreateTemplate(CreateTemplateViewModel model);
        bool UpdateTemplate(Guid Id, CreateTemplateViewModel model);
        bool DeleteTemplate(Guid Id);
        IQueryable<NotificationPathViewModel> GetNotificationPath();
        bool CreateNotificationPath(CreateNotificationPathViewModel model, Guid CompanyId, Guid UserId);
        bool UpdateNotificationPath(Guid Id, CreateNotificationPathViewModel model, Guid CompanyId, Guid UserId);
        bool DeleteNotificationPath(Guid Id, Guid CompanyId, Guid UserId);
        bool ToggleNotificationPath(Guid Id);

        IQueryable<AdminNotificationVM> GetAdminNotification(FilterAdminNotification filter = null, PaginationFilter paginationFilter = null);
        IQueryable<AdminNotificationVM> GetAdminNotificationById(Guid Id);
        bool CreateAdmiNotification(AdminNotificationDTO model, Guid CompanyId);
        bool UpdateAdminNotification(Guid Id, AdminNotificationDTO model, Guid CompanyId);
        bool RemindmeLater(Guid Id, NotificationReminder model, Guid CompanyId);
        bool DeleteAdmiNotification(Guid Id, Guid CompanyId);
        bool ToggleNotification(Guid Id);

        IQueryable<OfferPromotionViewModel> GetOfferPromotion(FilterPromotion filter = null, PaginationFilter paginationFilter = null);
        IQueryable<OfferPromotionViewModel> GetOfferPromotionById(Guid Id);
        bool CreateOfferPromotion(CreatePromotionViewModel model, Guid CompanyId);
        bool UpdateOfferPromotion(Guid Id, CreatePromotionViewModel model, Guid CompanyId);
        bool DeleteOfferPromotion(Guid Id, Guid CompanyId);
        bool TogglePromotion(Guid Id);

        IQueryable<PromoViewModel> GetPromoCode(FilterPromotionalCode filter = null, PaginationFilter paginationFilter = null);
        IQueryable<PromoViewModel> GetPromoCodeById(Guid Id);
        bool CreatePromoCode(CreatePromoViewModel model, Guid CompanyId);
        bool UpdatePromoCode(Guid Id, CreatePromoViewModel model, Guid CompanyId);
        bool DeletePromoCode(Guid Id, Guid CompanyId);

        IQueryable<ReferralCodeViewModel> GetReferralCode();
        bool CreateReferralCode(CreateReferralCodeViewModel model, Guid CompanyId, Guid UserId);
        bool AnyRecordInReferralCode();
        bool UpdateReferralCode(Guid Id, CreateReferralCodeViewModel model, Guid CompanyId, Guid UserId);
        bool DeleteReferralCode(Guid Id, Guid CompanyId, Guid UserId);
        Task<StatusModel> Adminlogin(AdminLogin request);
        Task<bool> ForgotPassword(ForgotPasswordVM request);
        Task<bool> ResetPassword(ResetPasswordVM request);
        Task<bool> ChangePassword(ChangePassword request);
    }
}
