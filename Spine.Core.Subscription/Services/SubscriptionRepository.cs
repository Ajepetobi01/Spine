using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Core.Subscription.Interface;
using Spine.Core.Subscription.ViewModel;
using Spine.Data;
using Spine.Data.Entities.Admin;
using Spine.Data.Entities.Subscription;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.Services
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly SpineContext _context;
        private readonly IEmailSender emailSender;

        public SubscriptionRepository(SpineContext context, IEmailSender emailSender)
        {
            _context = context;
            this.emailSender = emailSender;
        }

        public bool SaveAll()
        {
            return _context.SaveChanges() > 0;
        }
        public IQueryable<PlanViewModel> GetPlans()
        {
            //var con = _context.Database.GetDbConnection().ConnectionString;
            return _context.Plans.AsNoTracking().Where(x => x.Status == true).Select(x => new PlanViewModel
            {
                PlanId = x.PlanId,
                PlanName = x.PlanName,
                Amount = x.Amount,
                PlanDuration = x.PlanDuration == null ? 0 : x.PlanDuration.Value,
                IsFreePlan = x.IsFreePlan,
                Description = x.Description,
                IncludePromotion = x.IncludePromotion == true ? "Yes" : "No",
                Status = x.Status == true ? "Active" : "In-active",
            });
        }
        public bool AddPlan(PlanViewModel model)
        {
            var newPlan = new Plan()
            {
                PlanName = model.PlanName,
                Amount = model.Amount,
                PlanDuration = model.PlanDuration == null ? 0 : model.PlanDuration.Value,
                IsFreePlan = model.IsFreePlan,
                Description = model.Description,
            };

            _context.Plans.Add(newPlan);

            return SaveAll();
        }
        public bool UpdatePlan(PlanViewModel model)
        {
            bool result = false;

            if (model.PlanId > 0)
            {
                var targetPlan = _context.Plans.Find(model.PlanId);

                if (targetPlan != null)
                {
                    targetPlan.PlanName = model.PlanName;
                    targetPlan.Amount = model.Amount;
                    targetPlan.PlanDuration = model.PlanDuration == null ? 0 : model.PlanDuration.Value;
                    targetPlan.IsFreePlan = model.IsFreePlan;
                    targetPlan.Description = model.Description;

                    SaveAll();

                    result = true;
                }
            }

            return result;
        }
        public bool DeletePlan(int id)
        {
            var result = false;
            var record = _context.Plans.Find(id);
            if (record != null)
            {

                _context.Plans.Remove(record);

                SaveAll();

                result = true;
            }

            return result;
        }

        public async Task<bool> AddOnlineDepositOrder(CompanySubscription model)
        {
            var result = false;
            _context.CompanySubscriptions.Add(model);
            SaveAll();
            var targetCompany = _context.Companies.Where(x => x.Id == model.ID_Company).FirstOrDefault();
            if (targetCompany != null)
            {
                targetCompany.ID_Subscription = model.ID_Subscription;
                await _context.SaveChangesAsync();

                result = true;
            }
            return result;
        }
        public async Task<bool> UpdateOnlineDepositOrder(string reference)
        {
            var result = false;
            var targetOrder = _context.CompanySubscriptions.Where(x => x.TransactionRef == reference).FirstOrDefault();

            if (targetOrder != null)
            {
                targetOrder.PaymentStatus = true;
                targetOrder.IsActive = true;

                var targetCompany = _context.Companies.Where(x => x.Id == targetOrder.ID_Company).FirstOrDefault();
                if (targetCompany != null)
                {
                    targetCompany.ID_Subscription = targetOrder.ID_Subscription;
                }

                if (await _context.SaveChangesAsync() > 0)
                {
                    var plan = _context.Plans.Find(targetOrder.ID_Plan);
                    var user = _context.Users.Where(x => x.CompanyId == targetOrder.ID_Company).FirstOrDefault();
                    var emailModel = new SubscrberPaymentNotification
                    {
                        UserName = user.FullName,
                        UserEmail = user.Email,
                        Plan = plan.PlanName,
                        Duration = $"{plan.PlanDuration} Month(s)",
                        //Amount = plan.Amount.ToString("#,##0.00;(#,##0.00)"),
                        SubscriptionDate = DateTime.Now.ToString("dd/MM/yyyy")
                    };

                    var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - Subscription Detail ", EmailTemplateEnum.SubscrberPaymentNotification, emailModel);

                }

                result = true;
            }



            //await _context.SaveChangesAsync();
            return result;
        }
        public bool ReQueryPayment(string reference)
        {
            return _context.CompanySubscriptions.Where(x => x.TransactionRef == reference)
                .Select(x => x.PaymentStatus)
                .FirstOrDefault();
        }
        public IQueryable<CompanySubscription> GetTransactionByCompanyId(int subscriptionId)
        {
            return _context.CompanySubscriptions.AsNoTracking().Where(x => x.ID_Subscription == subscriptionId)
                .Select(x => new CompanySubscription
                {
                    ID_Subscription = x.ID_Subscription,
                    ID_Company = x.ID_Company,
                    ID_Plan = x.ID_Plan,
                    PlanType = x.PlanType,
                    Amount = x.Amount,
                    PaymentStatus = x.PaymentStatus,
                    IsActive = x.IsActive,
                    TransactionRef = x.TransactionRef,
                    TransactionDate = x.TransactionDate,
                    PaymentMethod = x.PaymentMethod,
                    ExpiredDate = x.ExpiredDate.Value
                });
        }
        public IQueryable<CompanyViewModel> CompanyById(Guid companyId)
        {
            return _context.Companies.AsNoTracking().Where(x => x.Id == companyId).Select(x => new CompanyViewModel
            {
                Id = x.Id,
                ID_Subscription = x.ID_Subscription,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Name = x.Name,
                Address = x.Address,
                BaseCurrencyId = x.BaseCurrencyId,
                BusinessType = x.BusinessType,
                City = x.City,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                DateEstablished = x.DateEstablished,
                DeletedBy = x.DeletedBy,
                Description = x.Description,
                EmployeeCount = x.EmployeeCount,
                IsDeleted = x.IsDeleted,
                IsVerified = x.IsVerified,
                LastModifiedBy = x.LastModifiedBy,
                LogoId = x.LogoId,
                ModifiedOn = x.ModifiedOn,
                Motto = x.Motto,
                OperatingSector = x.OperatingSector,
                SocialMediaProfile = x.SocialMediaProfile,
                ReferralCode = x.ReferralCode,
                Ref_ReferralCode = x.Ref_ReferralCode,
                TIN = x.TIN,
                ImportRecord = x.ImportRecord,
                BatchNo = x.BatchNo
            });
        }
        public IQueryable<CompanyViewModel> QueryCompany()
        {
            return _context.Companies.AsNoTracking().Select(x => new CompanyViewModel
            {
                Id = x.Id,
                ID_Subscription = x.ID_Subscription,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Name = x.Name,
                ReferralCode = x.ReferralCode,
                Ref_ReferralCode = x.Ref_ReferralCode
            });
        }
        //public async Task<bool> UpdateCompany(CompanyDTO model)
        //{
        //    var targetUser = _context.Companies.Where(x => x.Id == model.CompanyId).FirstOrDefault();

        //    if (targetUser != null)
        //    {
        //        if (model.ID_Subscription > 0)
        //        {
        //            targetUser.ID_Subscription = model.ID_Subscription;
        //        }
        //        if (!string.IsNullOrEmpty(model.PhoneNumber))
        //        {
        //            targetUser.PhoneNumber = model.PhoneNumber;
        //        }
        //        if (!string.IsNullOrEmpty(model.BusinessName))
        //        {
        //            targetUser.Name = model.BusinessName;
        //        }
        //        if (!string.IsNullOrEmpty(model.BusinessType))
        //        {
        //            targetUser.BusinessType = model.BusinessType;
        //        }
        //        if (!string.IsNullOrEmpty(model.OperatingSector))
        //        {
        //            targetUser.OperatingSector = model.OperatingSector;
        //        }
        //        targetUser.ModifiedOn = DateTime.Now;
        //    }

        //    await _context.SaveChangesAsync();

        //    return true;
        //}
        public bool isValidReferralCode(string Ref_referralCode)
        {
            return _context.Companies.Select(x => x.ReferralCode == Ref_referralCode).FirstOrDefault();
        }

        public Response PromoCode(PromoCodeViewModel model)
        {
            var response = new Response();

            var promoCode = _context.OfferPromotions.Where(x => x.PromotionCode == model.PromoCode).FirstOrDefault();
            if (promoCode == null)
            {
                throw new("Please use a valide promo code");
            }
            if (!promoCode.EnablePromotion)
            {
                throw new("Promo Code is not enable");
            }

            var plan = _context.Plans.Where(p => p.PlanId == model.PlanId).FirstOrDefault();
            if (plan == null)
            {
                throw new($"No plan with id {model.PlanId}");
            }

            decimal distcount = (promoCode.Percentage / 100) * plan.Amount;
            decimal amountAfterDistcount = plan.Amount - distcount;

            var rowId = Guid.NewGuid();
            var promotionalCode = new PromotionalCode()
            {
                Id = rowId,
                PromoCode = model.PromoCode,
                UserId = model.CompanyId,
                DateCreated = DateTime.Now,
                PercentageOffer = promoCode.Percentage,
                PlanId = model.PlanId,
                ActualAmount = plan.Amount,
                AmountAfterDistcount = Math.Abs(amountAfterDistcount),
                IsUsed = false,
                TransactionRef = string.Empty,
            };
            _context.PromotionalCodes.Add(promotionalCode);
            response.IsSaved = _context.SaveChanges() > 0;

            response.PlanId = plan.PlanId;
            response.Amount = amountAfterDistcount;
            response.CompanyId = model.CompanyId;
            response.PromoCodeId = rowId;
            response.ReferralCodeId = null;

            return response;
        }
        public decimal ValidPromoCodeAmount(Guid promocodeId)
        {
            return _context.PromotionalCodes.Find(promocodeId).AmountAfterDistcount;
        }
        public async Task<bool> UpdatePromoCodeTransactionRef(Guid promocodeId, string transactionRef)
        {
            var result = false;
            var targetOrder = _context.PromotionalCodes.Find(promocodeId);

            if (targetOrder != null)
            {
                targetOrder.TransactionRef = transactionRef;
            }
            result = await _context.SaveChangesAsync() > 0;

            return result;
        }
        public async Task<bool> UpdatePromoCode(Guid? promocodeId = null, string transactionRef = null)
        {
            var result = false;
            if (promocodeId != Guid.Empty && promocodeId != null)
            {
                var targetOrder = _context.PromotionalCodes.Find(promocodeId.Value);

                if (targetOrder != null)
                {
                    targetOrder.IsUsed = true;
                }
            }
            var transactions = _context.PromotionalCodes.Where(x => x.TransactionRef == transactionRef).FirstOrDefault();
            if (transactions != null)
            {
                transactions.IsUsed = true;
            }
            result = await _context.SaveChangesAsync() > 0;

            return result;
        }
        public bool isPromoCodeValid(Guid promocodeId)
        {
            return _context.PromotionalCodes.Where(x => x.Id == promocodeId)
                .Select(x => x.IsUsed)
                .FirstOrDefault();
        }
        public Response ReferralCode(ReferralViewModel model)
        {
            var response = new Response();

            var referralCode = _context.ReferralCodes.Where(x => x.Status).FirstOrDefault();
            if (referralCode == null)
            {
                //throw new("Referral code is not enable");
            }

            if (!string.IsNullOrEmpty(model.ReferralCode))
            {
                var isReferralCodeValid = _context.Companies.Where(x => x.ReferralCode.Trim().ToLower() == model.ReferralCode.Trim().ToLower())
                    .Select(x => x.ReferralCode).FirstOrDefault();
                if (string.IsNullOrEmpty(isReferralCodeValid))
                {
                    return response;
                    //throw new($"Referral Code {model.ReferralCode} not valid");
                }
            }

            var plan = _context.Plans.Where(p => p.PlanId == model.PlanId).FirstOrDefault();
            if (plan == null)
            {
                //throw new($"No plan with id {model.PlanId}");
            }

            decimal distcount = (referralCode.Percentage / 100) * plan.Amount;
            decimal amountAfterDistcount = plan.Amount - distcount;

            var rowId = Guid.NewGuid();
            var usedReferralCode = new UsedReferralCode()
            {
                Id = rowId,
                ReferralCode = model.ReferralCode,
                UserId = model.CompanyId,
                DateCreated = DateTime.Now,
                PercentageOffer = referralCode.Percentage,
                PlanId = model.PlanId,
                ActualAmount = plan.Amount,
                AmountAfterDistcount = Math.Abs(amountAfterDistcount),
                IsUsed = false,
                TransactionRef = string.Empty,
            };
            _context.UsedReferralCodes.Add(usedReferralCode);
            response.IsSaved = _context.SaveChanges() > 0;

            response.PlanId = plan.PlanId;
            response.Amount = amountAfterDistcount;
            response.CompanyId = model.CompanyId;
            response.ReferralCodeId = rowId;
            response.PromoCodeId = null;

            return response;
        }
        public decimal ValidReferralCodeAmount(Guid referralcodeId)
        {
            return _context.UsedReferralCodes.Find(referralcodeId).AmountAfterDistcount;
        }
        public async Task<bool> UpdateReferralCodeTransactionRef(Guid referralcodeId, string transactionRef)
        {
            var result = false;
            var targetOrder = _context.UsedReferralCodes.Find(referralcodeId);

            if (targetOrder != null)
            {
                targetOrder.TransactionRef = transactionRef;
            }
            result = await _context.SaveChangesAsync() > 0;

            return result;
        }
        public async Task<bool> UpdateReferralCode(Guid? referralcodeId = null, string transactionRef = null)
        {
            var result = false;
            if (referralcodeId != Guid.Empty && referralcodeId != null)
            {
                var targetOrder = _context.UsedReferralCodes.Find(referralcodeId);

                if (targetOrder != null)
                {
                    targetOrder.IsUsed = true;
                }
            }
            var transactions = _context.UsedReferralCodes.Where(x => x.TransactionRef == transactionRef).FirstOrDefault();
            if (transactions != null)
            {
                transactions.IsUsed = true;
            }
            result = await _context.SaveChangesAsync() > 0;

            return result;
        }
        public bool isReferralCodeValid(Guid referralcodeId)
        {
            return _context.UsedReferralCodes.Where(x => x.Id == referralcodeId)
                .Select(x => x.IsUsed)
                .FirstOrDefault();
        }

        public bool isReferralCodeEnable()
        {
            return _context.ReferralCodes.Select(x => x.Status).FirstOrDefault();
        }
        public bool isPromoCodeEnable()
        {
            return _context.OfferPromotions.Select(x => x.EnablePromotion).FirstOrDefault();
        }
        public async Task<CompanySubscriptionDTO> GetSubscription(string reference)
        {
            var subscription = await _context.CompanySubscriptions.Where(x => x.TransactionRef == reference && x.ExpiredDate >= DateTime.Now && x.PaymentStatus)
                        .Select(x => new CompanySubscriptionDTO
                        {
                            Id_Subscription = x.ID_Subscription,
                            Id_Plan = x.ID_Plan,
                            SubscriptionDate = x.TransactionDate == null ? null : x.TransactionDate.Value.ToString("dd/MM/yyyy"),
                            ExpiredDate = x.ExpiredDate == null ? null : x.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                            Amount = x.Amount,
                            IsActive = x.IsActive,
                            PaymentStatus = x.PaymentStatus,
                        }).FirstOrDefaultAsync();

            return subscription;
        }
    }
}
