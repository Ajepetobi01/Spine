using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.Helpers;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Admin;
using Spine.Data.Entities.Subscription;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Spine.Core.ManageSubcription.Jobs;

namespace Spine.Core.ManageSubcription.Services
{

    public class ManageSubcriptionRepository : IManageSubcriptionRepository
    {
        private readonly SpineContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration configuration;
        private readonly JwtSettings jwtSettings;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IPermissionHelper permissionHelper;
        private readonly CommandsScheduler _scheduler;

        public ManageSubcriptionRepository(SpineContext context, UserManager<ApplicationUser> userManager, IMapper mapper,
            IEmailSender emailSender, IConfiguration configuration, IOptions<JwtSettings> jwtSettings,
            RoleManager<ApplicationRole> roleManager, IPermissionHelper permissionHelper, CommandsScheduler scheduler)
        {
            _context = context;
            this.userManager = userManager;
            this.mapper = mapper;
            this.emailSender = emailSender;
            this.configuration = configuration;
            this.jwtSettings = jwtSettings.Value;
            this.roleManager = roleManager;
            this.permissionHelper = permissionHelper;
            _scheduler = scheduler;
        }

        public bool SaveAll()
        {
            return _context.SaveChanges() > 0;
        }

        //subscriber module
        public IQueryable<SubscriptionDTO> GetUnSubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var subscriptionList = (from com in _context.Companies
                                    join usr in _context.Users on com.Email equals usr.Email
                                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                                    where com.IsDeleted == false && com.ID_Subscription < 1
                                    select new SubscriptionDTO
                                    {
                                        ID_Company = com.Id,
                                        FirstName = usr.FirstName,
                                        LastName = usr.LastName,
                                        FullName = usr.FullName,
                                        EmailAddress = usr.Email,
                                        PhoneNumber = usr.PhoneNumber,
                                        BusinessType = com.BusinessType,
                                        ReferralCode = com.ReferralCode,
                                        Ref_ReferralCode = com.Ref_ReferralCode,
                                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                                        GetCapturedDate = com.CreatedOn,
                                        GetDateOfBirth = usr.DateOfBirth,
                                        DateOfBirth = usr.DateOfBirth.Value.ToString("dd MMM yyyy"),
                                        Month = usr.DateOfBirth.Value.ToString("MMM"),
                                        BatchNo = com.BatchNo,
                                        ImportRecord = com.ImportRecord,
                                        BusinessName = com.Name,
                                        BusinessSector = com.OperatingSector,
                                        Gender = usr.Gender,
                                        TIN = com.TIN,
                                        Shipping = _context.SubscriberShippings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new ShippingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Shipping = x.ID_Shipping,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                                        Billing = _context.SubscriberBillings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new BillingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Billing = x.ID_Billing,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                                    });

            subscriptionList = AddFilterOnQuery(filter, subscriptionList);

            if (paginationFilter == null)
            {
                return subscriptionList.OrderByDescending(x => x.GetCapturedDate);
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return subscriptionList.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<SubscriptionDTO> GetUnSubacriberByCompayId(Guid companyId)
        {
            return (from com in _context.Companies
                    join usr in _context.Users on com.Email equals usr.Email
                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                    where com.IsDeleted == false && com.ID_Subscription < 1 && com.Id == companyId
                    select new SubscriptionDTO
                    {
                        ID_Company = com.Id,
                        FirstName = usr.FirstName,
                        LastName = usr.LastName,
                        FullName = usr.FullName,
                        EmailAddress = usr.Email,
                        PhoneNumber = usr.PhoneNumber,
                        BusinessType = com.BusinessType,
                        ReferralCode = com.ReferralCode,
                        Ref_ReferralCode = com.Ref_ReferralCode,
                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                        GetCapturedDate = com.CreatedOn,
                        GetDateOfBirth = usr.DateOfBirth,
                        DateOfBirth = usr.DateOfBirth.Value.ToString("dd MMM yyyy"),
                        Month = usr.DateOfBirth.Value.ToString("MMM"),
                        BatchNo = com.BatchNo,
                        ImportRecord = com.ImportRecord,
                        BusinessName = com.Name,
                        BusinessSector = com.OperatingSector,
                        Gender = usr.Gender,
                        TIN = com.TIN,
                        Shipping = _context.SubscriberShippings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new ShippingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Shipping = x.ID_Shipping,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                        Billing = _context.SubscriberBillings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new BillingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Billing = x.ID_Billing,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                    });
        }
        public IQueryable<SubscriptionDTO> GetSubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var subscriptionList = (from com in _context.Companies
                                    join usr in _context.Users on com.Email equals usr.Email
                                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                                    join pl in _context.Plans on sub.ID_Plan equals pl.PlanId
                                    where com.IsDeleted == false && sub.PaymentStatus
                                    select new SubscriptionDTO
                                    {
                                        ID_Company = com.Id,
                                        ID_Subscription = (int?)sub.ID_Subscription,
                                        FirstName = usr.FirstName,
                                        LastName = usr.LastName,
                                        FullName = usr.FullName,
                                        EmailAddress = usr.Email,
                                        PhoneNumber = usr.PhoneNumber,
                                        PlanName = pl.PlanName,
                                        BusinessType = com.BusinessType,
                                        OpeningBalance = (decimal?)sub.Amount,
                                        TransactionRef = sub.TransactionRef,
                                        Status = sub.IsActive == true ? "Active" : "In-Active",
                                        ReferralCode = com.ReferralCode,
                                        Ref_ReferralCode = com.Ref_ReferralCode,
                                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                                        GetCapturedDate = com.CreatedOn,
                                        SubscriptionDate = sub.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        ExpiredDate = sub.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                                        GetExpiredDate = sub.ExpiredDate.Value,
                                        GetDateOfBirth = usr.DateOfBirth,
                                        DateOfBirth = usr.DateOfBirth != null ? usr.DateOfBirth.Value.ToString("dd/MMM/yyyy") : "",
                                        Month = usr.DateOfBirth.Value.ToString("MMM"),
                                        GetSubscriptionDate = sub.TransactionDate,
                                        BusinessName = com.Name,
                                        BusinessSector = com.OperatingSector,
                                        Gender = usr.Gender,
                                        TIN = com.TIN,
                                        DaysToExpired = (int?)sub.ExpiredDate.Value.Day >= 0 ? Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) : 0,
                                        ExpiredMessage = sub.ExpiredDate.Value.Day >= 0 ? EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) > 0 ? +
                                                         EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) + " day(s) to Overdue" : "Overdue by " + Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) + " day(s) " : string.Empty,
                                        BatchNo = com.BatchNo,
                                        ImportRecord = com.ImportRecord,
                                        Shipping = _context.SubscriberShippings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new ShippingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Shipping = x.ID_Shipping,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                                        Billing = _context.SubscriberBillings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new BillingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Billing = x.ID_Billing,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                                    });

            subscriptionList = AddFilterOnQuery(filter, subscriptionList);

            if (paginationFilter == null)
            {
                return subscriptionList.OrderByDescending(x => x.GetCapturedDate);
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            //subscriptionList.OrderByDescending(x => x.GetCapturedDate).Skip(skip).Take(paginationFilter.PageSize);
            return subscriptionList.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<SubscriptionDTO> GetAlmostExpirySubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var subscriptionList = (from com in _context.Companies
                                    join usr in _context.Users on com.Email equals usr.Email
                                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                                    join pl in _context.Plans on (int?)sub.ID_Plan equals pl.PlanId
                                    where com.IsDeleted == false && EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) <= 7
                                    select new SubscriptionDTO
                                    {
                                        ID_Company = com.Id,
                                        ID_Subscription = (int?)sub.ID_Subscription,
                                        FirstName = usr.FirstName,
                                        LastName = usr.LastName,
                                        FullName = usr.FullName,
                                        EmailAddress = usr.Email,
                                        PhoneNumber = usr.PhoneNumber,
                                        PlanName = pl.PlanName,
                                        BusinessType = com.BusinessType,
                                        OpeningBalance = (decimal?)sub.Amount,
                                        TransactionRef = sub.TransactionRef,
                                        Status = sub.IsActive == true ? "Active" : "In-Active",
                                        ReferralCode = com.ReferralCode,
                                        Ref_ReferralCode = com.Ref_ReferralCode,
                                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                                        GetCapturedDate = com.CreatedOn,
                                        SubscriptionDate = sub.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        ExpiredDate = sub.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                                        GetExpiredDate = sub.ExpiredDate.Value,
                                        GetDateOfBirth = usr.DateOfBirth,
                                        DateOfBirth = usr.DateOfBirth != null ? usr.DateOfBirth.Value.ToString("dd/MMM/yyyy") : "",
                                        Month = usr.DateOfBirth.Value.ToString("MMM"),
                                        GetSubscriptionDate = sub.TransactionDate,
                                        BusinessName = com.Name,
                                        BusinessSector = com.OperatingSector,
                                        Gender = usr.Gender,
                                        TIN = com.TIN,
                                        DaysToExpired = (int?)sub.ExpiredDate.Value.Day >= 0 ? Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) : 0,
                                        ExpiredMessage = sub.ExpiredDate.Value.Day >= 0 ? EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) > 0 ? +
                                                         EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) + " day(s) to Overdue" : "Overdue by " + Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) + " day(s) " : string.Empty,
                                        BatchNo = com.BatchNo,
                                        ImportRecord = com.ImportRecord
                                    });

            subscriptionList = AddFilterOnQuery(filter, subscriptionList);

            if (paginationFilter == null)
            {
                return subscriptionList.OrderByDescending(x => x.GetCapturedDate);
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return subscriptionList.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<SubscriptionDTO> GetOnboradingAnalysis(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var subscriptionList = (from com in _context.Companies
                                    join usr in _context.Users on com.Email equals usr.Email
                                    join sub in _context.CompanySubscriptions on com.Id equals (Guid?)sub.ID_Company
                                    join pl in _context.Plans on (int?)sub.ID_Plan equals pl.PlanId
                                    where com.IsDeleted == false
                                    select new SubscriptionDTO
                                    {
                                        ID_Company = com.Id,
                                        ID_Subscription = (int?)sub.ID_Subscription,
                                        FirstName = usr.FirstName,
                                        LastName = usr.LastName,
                                        FullName = usr.FullName,
                                        EmailAddress = usr.Email,
                                        PhoneNumber = usr.PhoneNumber,
                                        PlanName = pl.PlanName,
                                        OpeningBalance = (decimal?)sub.Amount,
                                        TransactionRef = sub.TransactionRef,
                                        Status = sub.IsActive == true ? "Active" : "In-Active",
                                        ReferralCode = com.ReferralCode,
                                        Ref_ReferralCode = com.Ref_ReferralCode,
                                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                                        GetCapturedDate = com.CreatedOn,
                                        SubscriptionDate = sub.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        ExpiredDate = sub.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                                        GetExpiredDate = sub.ExpiredDate.Value,
                                        GetDateOfBirth = usr.DateOfBirth,
                                        DateOfBirth = usr.DateOfBirth != null ? usr.DateOfBirth.Value.ToString("dd/MMM/yyyy") : "",
                                        Month = usr.DateOfBirth.Value.ToString("MMM"),
                                        GetSubscriptionDate = sub.TransactionDate,
                                        BusinessName = com.Name,
                                        BusinessSector = com.OperatingSector,
                                        Gender = usr.Gender,
                                        TIN = com.TIN,
                                        DaysToExpired = (int?)sub.ExpiredDate.Value.Day >= 0 ? Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) : 0,
                                        ExpiredMessage = sub.ExpiredDate.Value.Day >= 0 ? EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) > 0 ? +
                                                         EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) + " day(s) to Overdue" : "Overdue by " + Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) + " day(s) " : string.Empty,
                                        BatchNo = com.BatchNo,
                                        ImportRecord = com.ImportRecord
                                    });

            subscriptionList = AddFilterOnQuery(filter, subscriptionList);

            if (paginationFilter == null)
            {
                return subscriptionList.OrderByDescending(x => x.GetCapturedDate);
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return subscriptionList.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<SubscriptionDTO> GetImportedSubscribers(string BatchNo, GetAllPostFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var subscriptionList = (from com in _context.Companies
                                    join usr in _context.Users on com.Email equals usr.Email
                                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Id.ToString() into btype
                                    //from bs in btype.DefaultIfEmpty()
                                    join sub in _context.CompanySubscriptions on com.Id equals (Guid?)sub.ID_Company into subscription
                                    from sub in subscription.DefaultIfEmpty()
                                    join pl in _context.Plans on sub.ID_Plan equals pl.PlanId into plan
                                    from pl in plan.DefaultIfEmpty()
                                    where com.BatchNo == BatchNo
                                    where com.IsDeleted == false
                                    select new SubscriptionDTO
                                    {
                                        ID_Company = com.Id,
                                        ID_Subscription = (int?)sub.ID_Subscription,
                                        FirstName = usr.FirstName,
                                        LastName = usr.LastName,
                                        FullName = usr.FullName,
                                        EmailAddress = usr.Email,
                                        PhoneNumber = usr.PhoneNumber,
                                        PlanName = pl.PlanName,
                                        BusinessType = com.BusinessType,
                                        OpeningBalance = (decimal?)sub.Amount,
                                        TransactionRef = sub.TransactionRef,
                                        Status = sub.IsActive == true ? "Active" : "In-Active",//com.ID_Subscription > 0 && sub.IsActive == true && sub.ExpiredDate >= DateTime.Now ? "Active" : "In-Active",
                                        ReferralCode = com.ReferralCode,
                                        Ref_ReferralCode = com.Ref_ReferralCode,
                                        ImportRecord = com.ImportRecord,
                                        BatchNo = com.BatchNo,
                                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                                        GetCapturedDate = com.CreatedOn,
                                        SubscriptionDate = sub.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        ExpiredDate = sub.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                                        GetExpiredDate = sub.ExpiredDate.Value,
                                        GetDateOfBirth = usr.DateOfBirth,
                                        DateOfBirth = usr.DateOfBirth != null ? usr.DateOfBirth.Value.ToString("dd/MMM/yyyy") : "",
                                        Month = usr.DateOfBirth.Value.ToString("MMM"),
                                        GetSubscriptionDate = sub.TransactionDate,
                                        BusinessName = com.Name,
                                        BusinessSector = com.OperatingSector,
                                        Gender = usr.Gender,
                                        TIN = com.TIN,
                                        DaysToExpired = (int?)sub.ExpiredDate.Value.Day >= 0 ? Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) : 0,
                                        ExpiredMessage = sub.ExpiredDate.Value.Day >= 0 ? EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) > 0 ? +
                                                         EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value) + " day(s) to Overdue" : "Overdue by " + Math.Abs(EF.Functions.DateDiffDay(DateTime.Now, sub.ExpiredDate.Value)) + " day(s) " : string.Empty,
                                        Shipping = _context.SubscriberShippings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new ShippingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Shipping = x.ID_Shipping,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                                        Billing = _context.SubscriberBillings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new BillingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Billing = x.ID_Billing,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                                    });

            subscriptionList = AddFilterOnQuery(filter, subscriptionList);
            if (paginationFilter == null)
            {
                return subscriptionList;
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return subscriptionList.Skip(skip).Take(paginationFilter.PageSize);
        }
        private static IQueryable<SubscriptionDTO> AddFilterOnQuery(GetAllPostFilter filter, IQueryable<SubscriptionDTO> queryable)
        {
            if (!string.IsNullOrEmpty(filter?.Email))
            {
                queryable = queryable.Where(x => x.EmailAddress == filter.Email);
            }
            if (!string.IsNullOrEmpty(filter?.Status))
            {
                queryable = queryable.Where(x => x.Status.Trim().ToLower() == filter.Status.Trim().ToLower());
            }
            if (filter?.CompanyId != Guid.Empty)
            {
                queryable = queryable.Where(x => x.ID_Company == filter.CompanyId);
            }
            if (filter?.daysTwoExpired > 0)
            {
                queryable = queryable.Where(x => EF.Functions.DateDiffDay(DateTime.Now, x.GetExpiredDate.Value) <= filter.daysTwoExpired);
            }
            if (!string.IsNullOrEmpty(filter?.referralcode))
            {
                queryable = queryable.Where(x => x.ReferralCode.Trim() == filter.referralcode.Trim());
            }
            if (!string.IsNullOrEmpty(filter?.Ref_ReferralCode))
            {
                queryable = queryable.Where(x => x.Ref_ReferralCode.Trim() == filter.Ref_ReferralCode.Trim());
            }
            if (!string.IsNullOrEmpty(filter?.Name))
            {
                queryable = queryable.Where(x => x.FullName.Trim() == filter.Name.Trim());
            }
            if (!string.IsNullOrEmpty(filter?.PlanName))
            {
                queryable = queryable.Where(x => x.PlanName.Trim() == filter.PlanName.Trim());
            }
            if (filter.StartDate.HasValue) queryable = queryable.Where(x => x.GetSubscriptionDate >= filter.StartDate);
            if (filter.EndDate.HasValue) queryable = queryable.Where(x => x.GetSubscriptionDate.Value.Date <= filter.EndDate);
            if (!string.IsNullOrEmpty(filter.Search)) queryable = queryable.Where(x => x.FirstName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.LastName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.FullName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.EmailAddress.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.PlanName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.PhoneNumber.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.Status.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.ReferralCode.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.Ref_ReferralCode.ToLower().Contains(filter.Search.ToLower()));

            if (string.IsNullOrEmpty(filter.SortBy)) queryable = queryable.OrderByDescending(x => x.GetCapturedDate);
            else queryable = queryable.OrderBy(filter.SortByAndOrder);
            return queryable;
        }
        public IQueryable<ReferralSubscriptionDTO> GetReferralSubscribers(GetAllPostFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var subscriptionList = (from com in _context.Companies
                                    join usr in _context.Users on com.Email equals usr.Email
                                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                                    join pl in _context.Plans on (int?)sub.ID_Plan equals pl.PlanId
                                    where com.IsDeleted == false
                                    select new ReferralSubscriptionDTO
                                    {
                                        ID_Company = com.Id,
                                        ID_Subscription = (int?)sub.ID_Subscription,
                                        FirstName = usr.FirstName,
                                        LastName = usr.LastName,
                                        FullName = usr.FullName,
                                        EmailAddress = usr.Email,
                                        PhoneNumber = usr.PhoneNumber,
                                        PlanName = pl.PlanName,
                                        BusinessType = com.BusinessType,
                                        OpeningBalance = (decimal?)sub.Amount,
                                        TransactionRef = sub.TransactionRef,
                                        Status = sub.IsActive == true ? "Active" : "In-Active", //com.ID_Subscription > 0 && sub.IsActive == true && sub.ExpiredDate >= DateTime.Now ? "Active" : "In-Active",
                                        ReferralCode = com.ReferralCode,
                                        Ref_ReferralCode = com.Ref_ReferralCode,
                                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                                        GetCapturedDate = com.CreatedOn,
                                        SubscriptionDate = sub.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        ExpiredDate = sub.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                                        GetExpiredDate = sub.ExpiredDate.Value,
                                        GetSubscriptionDate = sub.TransactionDate,
                                        BusinessName = com.BusinessType,
                                        BusinessSector = com.OperatingSector,
                                        Gender = usr.Gender,
                                        TIN = com.TIN,
                                        DaysToExpired = (int?)sub.ExpiredDate.Value.Day >= 0 ? Convert.ToInt32(sub.ExpiredDate.Value.Subtract(DateTime.Now).Days) : 0,
                                        ExpiredMessage = sub.ExpiredDate.Value.Day >= 0 ? sub.ExpiredDate.Value.Subtract(DateTime.Now).Days > 0 ? +
                                                         Convert.ToInt32(sub.ExpiredDate.Value.Subtract(DateTime.Now).Days) + " to Overdue" : " Overdue by" + Convert.ToInt32(Math.Abs(sub.ExpiredDate.Value.Subtract(DateTime.Now).Days)) + " day(s) " : string.Empty,
                                        BatchNo = com.BatchNo,
                                        ImportRecord = com.ImportRecord,
                                        NoofReferral = (from c in _context.Companies
                                                        where c.Id == com.Id && c.IsDeleted == false && c.Ref_ReferralCode == com.ReferralCode
                                                        select sub.ID_Subscription).Count()
                                    });


            if (!string.IsNullOrEmpty(filter?.Email))
            {
                subscriptionList = subscriptionList.Where(x => x.EmailAddress == filter.Email);
            }
            if (!string.IsNullOrEmpty(filter?.Status))
            {
                subscriptionList = subscriptionList.Where(x => x.Status.Trim().ToLower() == filter.Status.Trim().ToLower());
            }
            if (filter?.CompanyId != Guid.Empty)
            {
                subscriptionList = subscriptionList.Where(x => x.ID_Company == filter.CompanyId);
            }
            if (filter?.daysTwoExpired > 0)
            {
                subscriptionList = subscriptionList.Where(x => EF.Functions.DateDiffDay(DateTime.Now, x.GetExpiredDate.Value) <= filter.daysTwoExpired);
            }
            if (!string.IsNullOrEmpty(filter?.referralcode))
            {
                subscriptionList = subscriptionList.Where(x => x.ReferralCode.Trim() == filter.referralcode.Trim());
            }
            if (!string.IsNullOrEmpty(filter?.Ref_ReferralCode))
            {
                subscriptionList = subscriptionList.Where(x => x.Ref_ReferralCode.Trim() == filter.Ref_ReferralCode.Trim());
            }
            if (!string.IsNullOrEmpty(filter?.Name))
            {
                subscriptionList = subscriptionList.Where(x => x.FullName.Trim() == filter.Name.Trim());
            }
            if (!string.IsNullOrEmpty(filter?.PlanName))
            {
                subscriptionList = subscriptionList.Where(x => x.PlanName.Trim() == filter.PlanName.Trim());
            }
            if (filter.StartDate.HasValue) subscriptionList = subscriptionList.Where(x => x.GetSubscriptionDate >= filter.StartDate);
            if (filter.EndDate.HasValue) subscriptionList = subscriptionList.Where(x => x.GetSubscriptionDate.Value.Date <= filter.EndDate);
            if (!string.IsNullOrEmpty(filter.Search)) subscriptionList = subscriptionList.Where(x => x.FirstName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.LastName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.FullName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.EmailAddress.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.PlanName.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.Status.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.ReferralCode.ToLower().Contains(filter.Search.ToLower())
                                                                                    || x.Ref_ReferralCode.ToLower().Contains(filter.Search.ToLower()));

            if (string.IsNullOrEmpty(filter.SortBy)) subscriptionList = subscriptionList.OrderByDescending(x => x.GetCapturedDate);
            else subscriptionList = subscriptionList.OrderBy(filter.SortByAndOrder);

            if (paginationFilter == null)
            {
                return subscriptionList.OrderByDescending(x => x.GetCapturedDate);
            }
            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return subscriptionList.Skip(skip).Take(paginationFilter.PageSize);
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
                ImportRecord = x.ImportRecord,
                BatchNo = x.BatchNo,
                TIN = x.TIN,
                BusinessName = x.Name,
                BusinessSector = x.OperatingSector,
            });
        }
        public IQueryable<SubscriptionDTO> GetSubacriberByCompayId(Guid companyId)
        {
            return (from com in _context.Companies.Where(i=>i.Id == companyId)
                    from usr in _context.Users.Where(i=>i.Email ==com.Email)
                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                    from sub in  _context.CompanySubscriptions.Where(i=>i.ID_Company== com.Id).DefaultIfEmpty()
                    from pl in _context.Plans.Where(i=>i.PlanId ==sub.ID_Plan).DefaultIfEmpty()
                    where com.IsDeleted == false
                    where com.Id == companyId
                    select new SubscriptionDTO
                    {
                        ID_Company = com.Id,
                        ID_Subscription = (int?)sub.ID_Subscription,
                        FirstName = usr.FirstName,
                        LastName = usr.LastName,
                        FullName = usr.FullName,
                        EmailAddress = usr.Email,
                        PhoneNumber = usr.PhoneNumber,
                        PlanName = pl ==null ? "" :pl.PlanName,
                        BusinessType = com.BusinessType,
                        OpeningBalance =sub ==null ?null: (decimal?)sub.Amount,
                        TransactionRef =sub ==null? "":  sub.TransactionRef,
                        Status =sub==null? "No Subscription" : sub.IsActive == true ? "Active" : "In-Active", //com.ID_Subscription > 0 && sub.IsActive == true && sub.ExpiredDate >= DateTime.Now ? "Active" : "In-Active",
                        ReferralCode = com.ReferralCode,
                        Ref_ReferralCode = com.Ref_ReferralCode,
                        ImportRecord = com.ImportRecord,
                        BatchNo = com.BatchNo,
                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                        GetCapturedDate = com.CreatedOn,
                        SubscriptionDate =sub ==null? DateTime.MinValue.ToString("dd/MM/yyyy") : sub.TransactionDate.Value.ToString("dd/MM/yyyy"),
                        ExpiredDate = sub ==null?DateTime.Now.ToString("dd/MM/yyyy"):  sub.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                        GetExpiredDate =sub ==null ?DateTime.Now: sub.ExpiredDate.Value,
                        GetSubscriptionDate =sub == null ?DateTime.MinValue : sub.TransactionDate,
                        BusinessName = com.Name,
                        BusinessSector = com.OperatingSector,
                        DaysToExpired = sub ==null ? 0 : (int?)sub.ExpiredDate.Value.Day >= 0 ? (int?)sub.ExpiredDate.Value.Subtract(DateTime.Now).TotalDays : 0,
                        Gender = usr.Gender,
                        DateOfBirth = usr.DateOfBirth != null ? usr.DateOfBirth.Value.ToString("dd/MMM/yyyy") : "",
                        TIN = com.TIN,
                        Shipping = _context.SubscriberShippings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new ShippingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Shipping = x.ID_Shipping,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                        Billing = _context.SubscriberBillings.Where(c => c.ID_Company == com.Id)
                                                .Select(x => new BillingVM
                                                {
                                                    Address1 = x.Address1,
                                                    Address2 = x.Address2,
                                                    PostalCode = x.PostalCode,
                                                    ID_Country = x.ID_Country,
                                                    ID_Billing = x.ID_Billing,
                                                    ID_State = x.ID_State,
                                                }).ToList(),
                    });
        }
        public DashBoardViewModel DashboardList()
        {
            var model = new DashBoardViewModel();

            var subscriptionList = (from com in _context.Companies
                                    join usr in _context.Users on com.Email equals usr.Email
                                    //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                                    join pl in _context.Plans on (int?)sub.ID_Plan equals pl.PlanId
                                    where com.IsDeleted == false && sub.PaymentStatus
                                    select new DashBoardSubscriptionQuery
                                    {
                                        ID_Company = com.Id,
                                        ID_Subscription = (int?)sub.ID_Subscription,
                                        FirstName = usr.FirstName,
                                        LastName = usr.LastName,
                                        FullName = usr.FullName,
                                        EmailAddress = usr.Email,
                                        PhoneNumber = usr.PhoneNumber,
                                        PlanName = pl.PlanName,
                                        //BusinessType = bs.Type,
                                        OpeningBalance = (decimal?)sub.Amount,
                                        TransactionRef = sub.TransactionRef,
                                        Status = sub.IsActive == true ? "Active" : "In-Active",
                                        IsActive = sub.IsActive == true ? true : false,
                                        PlanIsFree = pl.IsFreePlan == true ? true : false,
                                        ReferralCode = com.ReferralCode,
                                        Ref_ReferralCode = com.Ref_ReferralCode,
                                        ImportRecord = com.ImportRecord,
                                        BatchNo = com.BatchNo,
                                        CapturedDate = com.CreatedOn.ToString("dd/MM/yyyy"),
                                        GetCapturedDate = com.CreatedOn,
                                        SubscriptionDate = sub.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        ExpiredDate = sub.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                                        GetExpiredDate = sub.ExpiredDate.Value,
                                        GetDateOfBirth = usr.DateOfBirth,
                                        DateOfBirth = usr.DateOfBirth != null ? usr.DateOfBirth.Value.ToString("dd/MMM/yyyy") : "",
                                        Month = usr.DateOfBirth.Value.ToString("MMM"),
                                        GetSubscriptionDate = sub.TransactionDate,
                                        ID_Plan = pl.PlanId
                                    });

            //var subscriptionList = (from s in _context.CompanySubscriptions
            //                        join c in _context.Companies on s.id)

            var dFirstDayOfThisMonth = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            var dLastDayOfLastMonth = dFirstDayOfThisMonth.AddDays(-1);
            var dFirstDayOfLastMonth = dFirstDayOfThisMonth.AddMonths(-1);

            decimal LastMonthSubscriber = (from sub in subscriptionList
                                           where sub.GetCapturedDate >= dFirstDayOfLastMonth && sub.GetCapturedDate <= dLastDayOfLastMonth
                                           select sub.ID_Company).Count();
            decimal CurrentMonthSubscriber = (from sub in subscriptionList
                                              where sub.GetCapturedDate >= dFirstDayOfThisMonth && sub.GetCapturedDate <= DateTime.Today
                                              select sub.ID_Company).Count();

            decimal LastMonthActiveSubscription = (from sub in subscriptionList
                                                   where sub.IsActive && sub.GetSubscriptionDate >= dFirstDayOfLastMonth && sub.GetSubscriptionDate <= dLastDayOfLastMonth
                                                   select sub.ID_Subscription).Count();
            decimal CurrentMonthActiveSubscription = (from sub in subscriptionList
                                                      where sub.IsActive && sub.GetSubscriptionDate >= dFirstDayOfThisMonth && sub.GetSubscriptionDate <= DateTime.Today
                                                      select sub.ID_Subscription).Count();

            decimal LastMonthInActiveSubscription = (from sub in subscriptionList
                                                     where !sub.IsActive && sub.GetSubscriptionDate >= dFirstDayOfLastMonth && sub.GetSubscriptionDate <= dLastDayOfLastMonth
                                                     select sub.ID_Subscription).Count();
            decimal CurrentMonthInActiveSubscription = (from sub in subscriptionList
                                                        where !sub.IsActive && sub.GetSubscriptionDate >= dFirstDayOfThisMonth && sub.GetSubscriptionDate <= DateTime.Today
                                                        select sub.ID_Subscription).Count();

            decimal LastMonthInTrialSubscription = (from sub in subscriptionList
                                                    where sub.PlanIsFree && sub.GetSubscriptionDate >= dFirstDayOfLastMonth && sub.GetSubscriptionDate <= dLastDayOfLastMonth
                                                    select sub.ID_Subscription).Count();
            decimal CurrentMonthTrialSubscription = (from sub in subscriptionList
                                                     where sub.PlanIsFree && sub.GetSubscriptionDate >= dFirstDayOfThisMonth && sub.GetSubscriptionDate <= DateTime.Today
                                                     select sub.ID_Subscription).Count();


            model.ActiveSubscriber = subscriptionList.Where(x => x.IsActive).Count();
            decimal LastMonthActiveSubscriberpersentage = LastMonthActiveSubscription > 0 ? LastMonthActiveSubscription / 100 : 0;
            decimal CurrentMonthActiveSubscriberpersentage = CurrentMonthActiveSubscription > 0 ? CurrentMonthActiveSubscription / 100 : 0;
            if (LastMonthActiveSubscriberpersentage > CurrentMonthActiveSubscriberpersentage)
            {
                var decrease = LastMonthActiveSubscriberpersentage - CurrentMonthActiveSubscriberpersentage;
                model.ActiveSubscriberpersentage = $"{decrease}% Less Than Last Month";
                model.isActiveSubscriberpersentageLess = true;
            }
            else if (LastMonthActiveSubscriberpersentage < CurrentMonthActiveSubscriberpersentage)
            {
                var increase = CurrentMonthActiveSubscriberpersentage - LastMonthActiveSubscriberpersentage;
                model.ActiveSubscriberpersentage = $"{increase}% More Than Last Month";
                model.isActiveSubscriberpersentageLess = false;
            }
            else
            {
                model.ActiveSubscriberpersentage = $"{0}% No difference";
                model.isActiveSubscriberpersentageLess = null;
            }


            model.InActiveSubscriber = subscriptionList.Where(x => !x.IsActive).Count();
            decimal LastMonthInActiveSubscriberpersentage = LastMonthInActiveSubscription > 0 ? LastMonthInActiveSubscription / 100 : 0;
            decimal CurrentMonthInActiveSubscriberpersentage = CurrentMonthInActiveSubscription > 0 ? CurrentMonthInActiveSubscription / 100 : 0;
            if (LastMonthInActiveSubscriberpersentage > CurrentMonthInActiveSubscriberpersentage)
            {
                var decrease = LastMonthInActiveSubscriberpersentage - CurrentMonthInActiveSubscriberpersentage;
                model.InActiveSubscriberpersentage = $"{decrease}% Less Than Last Month";
                model.isInActiveSubscriberpersentageLess = true;
            }
            else if (LastMonthInActiveSubscriberpersentage < CurrentMonthInActiveSubscriberpersentage)
            {
                var increase = CurrentMonthInActiveSubscriberpersentage - LastMonthInActiveSubscriberpersentage;
                model.InActiveSubscriberpersentage = $"{increase}% More Than Last Month";
                model.isInActiveSubscriberpersentageLess = false;
            }
            else
            {
                model.InActiveSubscriberpersentage = $"{0}% No difference";
                model.isInActiveSubscriberpersentageLess = null;
            }


            model.TotalUnSubscriber = (from com in _context.Companies
                                       join usr in _context.Users on com.Email equals usr.Email
                                       join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
                                       where com.IsDeleted == false && com.ID_Subscription < 1
                                       select com.Id).Count();

            model.TotalSubscriber = subscriptionList.Count();
            decimal LastMonthTotalSubscriberpersentage = LastMonthSubscriber > 0 ? LastMonthSubscriber / 100 : 0;
            decimal CurrentMonthTotalSubscriberpersentage = CurrentMonthSubscriber > 0 ? CurrentMonthSubscriber / 100 : 0;
            if (LastMonthTotalSubscriberpersentage > CurrentMonthTotalSubscriberpersentage)
            {
                var decrease = LastMonthTotalSubscriberpersentage - CurrentMonthTotalSubscriberpersentage;
                model.TotalSubscriberpersentage = $"{decrease}% Less Than Last Month";
                model.isTotalSubscriberpersentageLess = true;
            }
            else if (LastMonthTotalSubscriberpersentage < CurrentMonthTotalSubscriberpersentage)
            {
                var increase = CurrentMonthTotalSubscriberpersentage - LastMonthTotalSubscriberpersentage;
                model.TotalSubscriberpersentage = $"{increase}% More Than Last Month";
                model.isTotalSubscriberpersentageLess = false;
            }
            else
            {
                model.TotalSubscriberpersentage = $"{0}% No difference";
                model.isTotalSubscriberpersentageLess = null;
            }

            model.SubscriberOnTrial = subscriptionList.Where(x => x.PlanIsFree).Count();
            decimal LastMonthSubscriberOnTrialpersentage = LastMonthInTrialSubscription > 0 ? LastMonthInTrialSubscription / 100 : 0;
            decimal CurrentMonthSubscriberOnTrialpersentage = CurrentMonthTrialSubscription > 0 ? CurrentMonthTrialSubscription / 100 : 0;
            if (LastMonthSubscriberOnTrialpersentage > CurrentMonthSubscriberOnTrialpersentage)
            {
                var decrease = LastMonthSubscriberOnTrialpersentage - CurrentMonthSubscriberOnTrialpersentage;
                model.SubscriberOnTrialpersentage = $"{decrease}% Less Than Last Month";
                model.isSubscriberOnTrialpersentageLess = true;
            }
            else if (LastMonthSubscriberOnTrialpersentage < CurrentMonthSubscriberOnTrialpersentage)
            {
                var increase = CurrentMonthSubscriberOnTrialpersentage - LastMonthSubscriberOnTrialpersentage;
                model.SubscriberOnTrialpersentage = $"{increase}% More Than Last Month";
                model.isSubscriberOnTrialpersentageLess = false;
            }
            else
            {
                model.SubscriberOnTrialpersentage = $"{0}% No difference";
                model.isSubscriberOnTrialpersentageLess = null;
            }

            model.TotalTransactionAmount = subscriptionList.Sum(x => x.OpeningBalance.Value);

            var CountPlan = new List<SubscriberPlan>();

            var record = (from pl in _context.Plans.Where(x => x.Status.Value == true)
                          select new SubscriberPlan
                          {
                              Id = pl.PlanId,
                              PlanName = pl.PlanName,
                              Count = 0,
                          }).ToList();

            foreach (var item in record)
            {
                CountPlan.Add(new SubscriberPlan
                {
                    Id = item.Id,
                    PlanName = item.PlanName,
                    Count = (from com in _context.Companies
                             join usr in _context.Users on com.Email equals usr.Email
                             join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                             where sub.ID_Plan == item.Id && !com.IsDeleted && sub.PaymentStatus
                             select sub.ID_Subscription).Count()
                });
            }

            model.PlanStatistics.TotalPlanCount = CountPlan.Sum(x => x.Count);
            model.PlanStatistics.SubscritionPlanStatistics = CountPlan;

            //var record = (from sub in _context.CompanySubscriptions
            //              join com in _context.Companies on sub.ID_Subscription equals com.ID_Subscription
            //              join pl in _context.Plans on sub.ID_Plan equals pl.PlanId
            //              where com.IsDeleted == false
            //              select new SubscriberPlan
            //              {
            //                  Id = sub.ID_Plan,
            //                  PlanName = pl.PlanName,
            //                  Count = _context.CompanySubscriptions.Where(x => x.ID_Plan == sub.ID_Plan && sub.IsActive && x.PaymentStatus).Count()
            //              }).ToList();


            //model.PlanStatistics.TotalPlanCount = subscriptionList.Where(x => x.ID_Plan > 0).Count();

            //model.PlanStatistics.SubscritionPlanStatistics = record.GroupBy(item => item.Id)
            //     .Select(grouping => grouping.FirstOrDefault())
            //     .ToList();


            model.SubscritionsBirthDate = (from u in _context.Users
                                           where u.DateOfBirth.HasValue == true
                                           select new BirthDayVM
                                           {
                                               Name = u.FullName,
                                               GetDateofBirth = u.DateOfBirth,
                                               DateofBirth = u.DateOfBirth.Value.ToString("dd MMM yyyy")
                                           }).OrderByDescending(x => x.GetDateofBirth).Take(10).ToList();

            model.Notification = (from x in _context.SubscriberNotifications
                                  where x.IsDeleted != true
                                  select new AdminNotiVM
                                  {
                                      Id = x.ID,
                                      Description = x.Description,
                                      ReminderDate = x.ReminderDate.ToString("dd/MM/yyyy")
                                  }).OrderByDescending(x => x.Id).Take(10).ToList();


            var chart = (from sub in subscriptionList
                         orderby sub.GetSubscriptionDate
                         select new OnboardingChartVM
                         {
                             Key = sub.GetSubscriptionDate.Value.ToString("MMM"),
                             Valus = _context.CompanySubscriptions.Where(x => x.TransactionDate.Value.Month == sub.GetSubscriptionDate.Value.Month
                             && x.TransactionDate.Value.Year == sub.GetSubscriptionDate.Value.Year && sub.IsActive).Count()
                         }).ToList();

            model.OnboardingChart = chart.Where(x => x.Key != null).GroupBy(item => item.Key)
                 .Select(grouping => grouping.FirstOrDefault())
                 .ToList();

            return model;
        }
        public int TotalNumberOfSubscribers()
        {
            return (from com in _context.Companies
                    join usr in _context.Users on com.Email equals usr.Email
                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                    where com.IsDeleted == false && sub.PaymentStatus
                    select com.Id).Count();
        }
        public int TotalNumberOfActiveSubscribers()
        {
            return (from com in _context.Companies
                    join usr in _context.Users on com.Email equals usr.Email
                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                    where com.IsDeleted == false && sub.IsActive && sub.PaymentStatus
                    select com.Id).Count();
        }
        public int TotalNumberOfInActiveSubscribers()
        {
            //var query = (from com in _context.Companies
            //             join usr in _context.Users on com.Id equals usr.CompanyId
            //             join sub in _context.CompanySubscriptions on com.Id equals (Guid?)sub.ID_Company into subscription
            //             from sub in subscription.DefaultIfEmpty()
            //             where com.IsDeleted == false
            //             select new
            //             {
            //                 ID = com.Id,
            //                 Status = sub.IsActive == true ? "Active" : "In-Active",
            //             });
            //return query.Where(x => x.Status == "In-Active").Count();

            return (from com in _context.Companies
                    join usr in _context.Users on com.Email equals usr.Email
                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                    where com.IsDeleted == false && !sub.IsActive && sub.PaymentStatus
                    select com.Id).Count();
        }
        public int TotalNumberOfSubscriberByReferralCode(string referralcode)
        {
            return (from com in _context.Companies
                    join usr in _context.Users on com.Email equals usr.Email
                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                    where com.Ref_ReferralCode == referralcode
                    select sub.ID_Subscription).Count();
        }
        public int TotalNumberOfActiveSubscriberByReferralCode(string referralcode)
        {
            //var query = (from com in _context.Companies
            //             join usr in _context.Users on com.Id equals usr.CompanyId
            //             join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
            //             where com.IsDeleted == false && com.Ref_ReferralCode == referralcode
            //             select new
            //             {
            //                 ID = com.Id,
            //                 Status = sub.IsActive == true ? "Active" : "In-Active",
            //             });
            //return query.Where(x => x.Status == "Active").Count();

            return (from com in _context.Companies
                    join usr in _context.Users on com.Email equals usr.Email
                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                    where com.Ref_ReferralCode == referralcode && !com.IsDeleted && sub.IsActive
                    select sub.ID_Subscription).Count();
        }
        public int TotalNumberOfInActiveSubscriberByReferralCode(string referralcode)
        {
            //var query = (from com in _context.Companies
            //             join usr in _context.Users on com.Id equals usr.CompanyId
            //             join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
            //             where com.IsDeleted == true && com.Ref_ReferralCode == referralcode
            //             select new
            //             {
            //                 ID = com.Id,
            //                 Status = sub.IsActive == true ? "Active" : "In-Active",
            //             });
            //return query.Where(x => x.Status == "In-Active").Count();

            return (from com in _context.Companies
                    join usr in _context.Users on com.Email equals usr.Email
                    join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                    where com.Ref_ReferralCode == referralcode && !com.IsDeleted && !sub.IsActive
                    select sub.ID_Subscription).Count();
        }
        public decimal TotalTransaction()
        {
            var getAmount = (from sub in _context.CompanySubscriptions
                             join com in _context.Companies on sub.ID_Subscription equals com.ID_Subscription
                             where com.IsDeleted == false
                             select new
                             {
                                 ID = com.Id,
                                 Amount = sub.Amount
                             });
            return getAmount.Sum(x => x.Amount);
        }
        public IQueryable<BirthDayVM> GetBirthDayList(PostBirthFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var UserQuery = (from u in _context.Users
                             where u.DateOfBirth.HasValue == true
                             select new BirthDayVM
                             {
                                 Name = u.FullName,
                                 GetDateofBirth = u.DateOfBirth,
                                 DateofBirth = u.DateOfBirth.Value.ToString("dd/MM/yyyy")
                             });
            if (!string.IsNullOrEmpty(filter?.Name))
            {
                UserQuery = UserQuery.Where(x => x.Name == filter.Name);
            }
            if (!string.IsNullOrEmpty(filter?.BeginDate))
            {
                var splittedDate = filter.BeginDate.Split('/');
                var begingDate = new DateTime(int.Parse(splittedDate[2]), int.Parse(splittedDate[1]), int.Parse(splittedDate[0]));

                UserQuery = UserQuery.Where(x => x.GetDateofBirth >= begingDate);
            }

            if (!string.IsNullOrEmpty(filter?.EndDate))
            {
                var splittedDate = filter.EndDate.Split('/');
                var endDate = new DateTime(int.Parse(splittedDate[2]), int.Parse(splittedDate[1]), int.Parse(splittedDate[0]));

                UserQuery = UserQuery.Where(x => x.GetDateofBirth <= endDate);
            }
            if (string.IsNullOrEmpty(filter?.SortBy)) UserQuery = UserQuery.OrderByDescending(x => x.GetDateofBirth);
            else UserQuery = UserQuery.OrderBy(filter?.SortByAndOrder);

            if (paginationFilter == null)
            {
                return UserQuery.OrderByDescending(x => x.GetDateofBirth);
            }
            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return UserQuery.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IEnumerable<SubscriberPlan> TotalNumberOfSubscribersByPlan()
        {
            var CountPlan = new List<SubscriberPlan>();

            var record = (from pl in _context.Plans.Where(x => x.Status.Value == true)
                          select new SubscriberPlan
                          {
                              Id = pl.PlanId,
                              PlanName = pl.PlanName,
                              Count = 0,
                          }).ToList();

            foreach (var item in record)
            {
                CountPlan.Add(new SubscriberPlan
                {
                    Id = item.Id,
                    PlanName = item.PlanName,
                    Count = (from com in _context.Companies
                             join usr in _context.Users on com.Email equals usr.Email
                             join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
                             where sub.ID_Plan == item.Id && !com.IsDeleted && sub.PaymentStatus
                             select sub.ID_Subscription).Count()
                });
            }

            //_context.CompanySubscriptions.Where(x => x.ID_Plan == item.Id && x.PaymentStatus && x.ExpiredDate > DateTime.Now).Count()
            return CountPlan;

            //var record = (from com in _context.Companies
            //              join usr in _context.Users on com.Email equals usr.Email
            //              //join bs in _context.BusinessTypes on com.BusinessType equals bs.Type
            //              join sub in _context.CompanySubscriptions on com.Id equals sub.ID_Company
            //              join pl in _context.Plans on sub.ID_Plan equals pl.PlanId
            //              where com.IsDeleted == false && sub.PaymentStatus
            //              select new SubscriberPlan
            //              {
            //                  Id = sub.ID_Plan,
            //                  PlanName = pl.PlanName,
            //                  Count = _context.CompanySubscriptions.Where(x => x.ID_Plan == sub.ID_Plan && sub.IsActive && x.PaymentStatus).Count()
            //              }).ToList();

            //return record.GroupBy(item => item.Id)
            //     .Select(grouping => grouping.FirstOrDefault())
            //     .ToList();
        }
        public async Task<StatusModel> SaveSubcribers(List<CompanyUploadModel> subcribers, Guid CompanyId, Guid UserId)
        {
            var targetStatus = new StatusModel();
            try
            {
                var BatchNo = EnumExtensionHelper.GenerateRandomString(8, true, false);
                var distinctEmails = subcribers.Select(x => x.Email).ToHashSet();
                if (distinctEmails.Count != subcribers.Count) throw new("Email address cannot contain duplicates");

                var distinctBusinessName = subcribers.Select(x => x.BusinessName).ToHashSet();
                if (distinctBusinessName.Count != subcribers.Count) throw new("Business name cannot contain duplicates");

                var userEmails = _context.Companies.Where(x => !x.IsDeleted).Select(x => x.Email).ToArray();

                //var uploadEmails = subcribers.Where(x => userEmails.Contains(x.Email))
                //        .Select(x => x.Email).ToList();

                int skipped = 0;
                foreach (var model in subcribers)
                {
                    if (userEmails.Contains(model.Email))
                    {
                        skipped++;
                        continue;
                    }

                    if (await _context.Companies.AnyAsync(x => (x.Name.ToLower() == model.BusinessName.ToLower() || x.Email == model.Email.ToLower())))
                        throw new($"Business name {model.BusinessName} or email {model.Email} is already taken ");

                    if (await userManager.FindByEmailAsync(model.Email) != null)
                    {
                        throw new($"Email {model.Email} is already in use");
                    }

                    if (!string.IsNullOrEmpty(model.Ref_ReferralCode))
                    {
                        var isReferralCodeValid = _context.Companies.Where(x => x.ReferralCode.Trim().ToLower() == model.Ref_ReferralCode.Trim().ToLower())
                            .Select(x => x.ReferralCode).FirstOrDefault();
                        if (string.IsNullOrEmpty(isReferralCodeValid))
                        {
                            throw new($"Referral Code {model.Ref_ReferralCode} not valid");
                        }
                    }

                    if (!string.IsNullOrEmpty(model.DateOfBirth))
                    {
                        var splittedDate = model.DateOfBirth.Split('/', ' ');
                        var dob = new DateTime(int.Parse(splittedDate[2]), int.Parse(splittedDate[1]), int.Parse(splittedDate[0]));
                        model.DateOfBirth = dob.ToString();
                    }

                    var referralCode = EnumExtensionHelper.GenerateRandomString(8, true, false);
                    var currencyId = _context.Currencies.First(x => x.Code == Constants.NigerianCurrencyCode).Id;
                    var company = mapper.Map<Company>(model);
                    company.BaseCurrencyId = currencyId;
                    company.ImportRecord = true;
                    company.BatchNo = BatchNo;
                    company.CreatedOn = Constants.GetCurrentDateTime();
                    company.ReferralCode = referralCode;
                    company.Ref_ReferralCode = model.Ref_ReferralCode;

                    _context.Companies.Add(company);

                    _context.CompanyCurrencies.Add(new CompanyCurrency
                    {
                        Id = SequentialGuid.Create(),
                        CompanyId = company.Id,
                        ActivatedOn = Constants.GetCurrentDateTime(),
                        IsActive = true,
                        OldCurrencyId = currencyId,
                        CurrencyId = currencyId,
                        Rate = 1,
                        ActivatedBy = company.CreatedBy
                    });

                    var ownerRole = await _context.Roles.FirstOrDefaultAsync(x => x.IsOwnerRole && !x.IsDeleted);
                    var password = EnumExtensionHelper.GenerateRandomString(6, true, false) + "1@";
                    var user = mapper.Map<ApplicationUser>(model);
                    user.IsBusinessOwner = true;
                    user.CompanyId = company.Id;
                    user.SecurityStamp = Guid.NewGuid().ToString();
                    user.RoleId = ownerRole == null ? Guid.Empty : ownerRole.Id;
                    user.DateOfBirth = model.DateOfBirth != string.Empty ? Convert.ToDateTime(model.DateOfBirth) : null;
                    user.CreatedBy = user.Id;
                    user.CreatedOn = Constants.GetCurrentDateTime();
                    var create = await userManager.CreateAsync(user, password);
                    if (!create.Succeeded)
                    {
                        throw new("Unable to complete signup, Please try again");
                    }
                    if (ownerRole != null)
                    {
                        await userManager.AddToRoleAsync(user, ownerRole.Name);
                    }

                    //var billing = mapper.Map<SubscriberBilling>(model);
                    //billing.ID_Company = company.Id;
                    //billing.DateCreated = Constants.GetCurrentDateTime();
                    //var shipping = mapper.Map<SubscriberShipping>(model);
                    //shipping.ID_Company = company.Id;
                    //shipping.DateCreated = Constants.GetCurrentDateTime();

                    //_context.SubscriberShippings.Add(shipping);
                    //_context.SubscriberBillings.Add(billing);

                    //var loginToken = await JwtHelper.GenerateJWToken(userManager, roleManager, jwtSettings, user, company.LogoId);

                    var loginToken = await JwtHelper.GenerateJWToken(jwtSettings, user);

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    _context.AccountConfirmationTokens.Add(new AccountConfirmationToken
                    {
                        Email = user.Email,
                        CreatedOn = Constants.GetCurrentDateTime(),
                        Token = code,
                        Id = user.Id
                    });

                    var webUrl = configuration["SpineWeb"];
                    var emailModel = new SubscriberSignup
                    {
                        ActionLink = Constants.GetConfirmAccountLink(webUrl, code),
                        Name = model.BusinessName,
                        Date = DateTime.Now.ToString("dd/MM/yyyy"), //Constants.GetCurrentDateTime().ToLongDateString()
                        UserName = model.Email,
                        Password = password,
                        ReferralCode = referralCode
                    };

                    var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - User Signup ", EmailTemplateEnum.SubscriberSignup, emailModel);
                }
                if (await _context.SaveChangesAsync() > 0)
                {
                    var AuditLog = new AuditLog()
                    {
                        EntityType = (int)AuditLogEntityType.Admin,
                        Action = (int)AuditLogCustomerAction.Update,
                        Description = $"Upload subscriber with BatchNo {BatchNo} on  {DateTime.Now}",
                        UserId = UserId,
                        CompanyId = CompanyId,
                        CreatedOn = DateTime.Now
                    };
                    _context.AuditLogs.Add(AuditLog);

                    targetStatus.Status = "success";
                    targetStatus.Message = BatchNo;
                    return targetStatus;
                }
                targetStatus.Status = "error";
                targetStatus.Message = "Unable to save import data, Please try again";
                return targetStatus;
                //return await _context.SaveChangesAsync() > 0
                //  ? new Response(HttpStatusCode.Created, "Saved successfully with batchNo " + BatchNo)
                //  : new Response(HttpStatusCode.BadRequest, "");


            }
            catch (Exception e)
            {
                var message = e.Message;
                throw new(message);
            }
        }
        public async Task<StatusModel> SaveSubcriber(CompanyParam model, Guid CompanyId, Guid UserId)
        {
            var targetStatus = new StatusModel();

            if (await _context.Companies.AnyAsync(x => (x.Name.ToLower() == model.BusinessName.ToLower() || x.Email == model.Email.ToLower())))
                throw new($"Business name {model.BusinessName} or email {model.Email} is already taken ");

            if (await userManager.FindByEmailAsync(model.Email) != null)
            {
                throw new($"Email {model.Email} is already in use");
            }

            if (!string.IsNullOrEmpty(model.Ref_ReferralCode))
            {
                var isReferralCodeValid = _context.Companies.Where(x => x.ReferralCode.Trim().ToLower() == model.Ref_ReferralCode.Trim().ToLower())
                    .Select(x => x.ReferralCode).FirstOrDefault();
                if (string.IsNullOrEmpty(isReferralCodeValid))
                {
                    throw new($"Referral Code {model.Ref_ReferralCode} not valid");
                }
            }


            try
            {
                var referralCode = EnumExtensionHelper.GenerateRandomString(8, true, false);
                var currencyId = _context.Currencies.First(x => x.Code == Constants.NigerianCurrencyCode).Id;
                var company = mapper.Map<Company>(model);
                company.BaseCurrencyId = currencyId;
                company.ImportRecord = false;
                company.CreatedOn = Constants.GetCurrentDateTime();
                company.ReferralCode = referralCode;
                company.Ref_ReferralCode = model.Ref_ReferralCode;

                _context.Companies.Add(company);

                _context.CompanyCurrencies.Add(new CompanyCurrency
                {
                    Id = SequentialGuid.Create(),
                    CompanyId = company.Id,
                    ActivatedOn = Constants.GetCurrentDateTime(),
                    IsActive = true,
                    OldCurrencyId = currencyId,
                    CurrencyId = currencyId,
                    Rate = 1,
                    ActivatedBy = company.CreatedBy
                });

                var ownerRole = await _context.Roles.FirstOrDefaultAsync(x => x.IsOwnerRole && !x.IsDeleted);

                var password = EnumExtensionHelper.GenerateRandomString(6, true, false) + "1@";
                var user = mapper.Map<ApplicationUser>(model);
                user.IsBusinessOwner = true;
                user.CompanyId = company.Id;
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.RoleId = ownerRole == null ? Guid.Empty : ownerRole.Id;
                user.DateOfBirth = model.DateOfBirth != string.Empty ? Convert.ToDateTime(model.DateOfBirth) : null;
                user.CreatedBy = user.Id;
                user.CreatedOn = Constants.GetCurrentDateTime();
                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    throw new("Unable to complete signup, Please try again");
                }
                if (ownerRole != null)
                {
                    await userManager.AddToRoleAsync(user, ownerRole.Name);
                }

                var billing = new List<SubscriberBilling>();
                if (model.Billing.Any())
                {
                    foreach (var rec in model.Billing)
                    {
                        var bill = new SubscriberBilling()
                        {
                            ID_Company = company.Id,
                            Address1 = rec.Address1,
                            Address2 = rec.Address2,
                            ID_Country = rec.ID_Country,
                            ID_State = rec.ID_State,
                            PostalCode = rec.PostalCode,
                            DateCreated = Constants.GetCurrentDateTime(),
                        };
                        billing.Add(bill);
                    }
                }
                _context.SubscriberBillings.AddRange(billing);

                var shipping = new List<SubscriberShipping>();
                if (model.Shipping.Any())
                {
                    foreach (var rec in model.Shipping)
                    {
                        var newShipping = new SubscriberShipping()
                        {
                            ID_Company = company.Id,
                            Address1 = rec.Address1,
                            Address2 = rec.Address2,
                            ID_Country = rec.ID_Country,
                            ID_State = rec.ID_State,
                            PostalCode = rec.PostalCode,
                            DateCreated = Constants.GetCurrentDateTime(),
                        };
                        shipping.Add(newShipping);
                    }
                }
                _context.SubscriberShippings.AddRange(shipping);

                //var loginToken = await JwtHelper.GenerateJWToken(userManager, roleManager, jwtSettings, user, company.LogoId);

                var loginToken = await JwtHelper.GenerateJWToken(jwtSettings, user);

                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                //var result = await userManager.ConfirmEmailAsync(user, code);
                _context.AccountConfirmationTokens.Add(new AccountConfirmationToken
                {
                    Email = user.Email,
                    CreatedOn = Constants.GetCurrentDateTime(),
                    Token = code,
                    Id = user.Id
                });

                var webUrl = configuration["SpineWeb"];
                var emailModel = new SubscriberSignup
                {
                    ActionLink = Constants.GetConfirmAccountLink(webUrl, code),
                    Name = model.BusinessName,
                    Date = DateTime.Now.ToString("dd/MM/yyyy"), //Constants.GetCurrentDateTime().ToLongDateString(),
                    UserName = model.Email,
                    Password = password,
                    ReferralCode = referralCode
                };

                if (await _context.SaveChangesAsync() > 0)
                {
                    var AuditLog = new AuditLog()
                    {
                        EntityType = (int)AuditLogEntityType.Admin,
                        Action = (int)AuditLogCustomerAction.Create,
                        Description = $"Added subscriber with Company Id {company.Id} on  {DateTime.Now}",
                        UserId = UserId,
                        CompanyId = CompanyId,
                        CreatedOn = DateTime.Now
                    };
                    _context.AuditLogs.Add(AuditLog);

                    var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - User Signup ", EmailTemplateEnum.SubscriberSignup, emailModel);
                    targetStatus.Status = "success";
                    targetStatus.Message = new JwtSecurityTokenHandler().WriteToken(loginToken);
                    return targetStatus;
                }
                targetStatus.Status = "error";
                targetStatus.Message = "Unable to save import data, Please try again";
                return targetStatus;

                //if (await _context.SaveChangesAsync() > 0)
                //{
                //    var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - User Signup ", EmailTemplateEnum.Signup, emailModel);

                //    targetStatus.id = company.Id.ToString();
                //    targetStatus.ReferralCode = company.ReferralCode;
                //    targetStatus.Status = "success";
                //    return new Response(HttpStatusCode.Created, new JwtSecurityTokenHandler().WriteToken(loginToken));
                //}
            }
            catch (Exception e)
            {
                throw new(e.Message);
            }
        }
        public StatusModel UpdateSubcriber(Guid CompanyId, UpdateCompanyParam param, Guid UserId)
        {
            var targetStatus = new StatusModel();
            try
            {
                //var dob = new DateTime();
                //if (!string.IsNullOrEmpty(param.DateOfBirth))
                //{
                //    var splittedDate = param.DateOfBirth.Split('/');
                //    dob = new DateTime(int.Parse(splittedDate[2]), int.Parse(splittedDate[1]), int.Parse(splittedDate[0]));
                //}


                var targetRecord = _context.Companies.Where(x => x.Id == CompanyId).FirstOrDefault();
                if (targetRecord == null) throw new("User not found");

                var checkEmail = _context.Companies.Where(x => x.Id != CompanyId && x.Email == param.Email).Select(x => x.Email);
                if (checkEmail.Any())
                {
                    throw new("Email is already in use by another customer");
                }
                if (targetRecord != null)
                {
                    if (!string.IsNullOrEmpty(param.BusinessName))
                    {
                        targetRecord.Name = param.BusinessName;
                    }
                    if (!string.IsNullOrEmpty(param.PhoneNumber))
                    {
                        targetRecord.PhoneNumber = param.PhoneNumber;
                    }
                    if (!string.IsNullOrEmpty(param.BusinessType))
                    {
                        targetRecord.BusinessType = param.BusinessType;
                    }
                    if (!string.IsNullOrEmpty(param.OperatingSector))
                    {
                        targetRecord.OperatingSector = param.OperatingSector;
                    }
                    if (!string.IsNullOrEmpty(param.TIN))
                    {
                        targetRecord.TIN = param.TIN;
                    }
                    if (!string.IsNullOrEmpty(param.Ref_ReferralCode))
                    {
                        targetRecord.Ref_ReferralCode = param.Ref_ReferralCode;
                    }
                    targetRecord.ModifiedOn = Constants.GetCurrentDateTime();
                    //targetRecord.Email = param.Email;
                    //targetRecord.Ref_ReferralCode = param.ReferralCode;
                }

                var user = _context.Users.Where(x => x.CompanyId == CompanyId && x.Email == param.Email).FirstOrDefault();
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(param.FirstName))
                    {
                        user.FirstName = param.FirstName;
                    }
                    if (!string.IsNullOrEmpty(param.LastName))
                    {
                        user.LastName = param.LastName;
                    }
                    if (!string.IsNullOrEmpty(param.Gender))
                    {
                        user.Gender = param.Gender;
                    }
                    if (!string.IsNullOrEmpty(param.DateOfBirth))
                    {
                        user.DateOfBirth = param.DateOfBirth != string.Empty ? Convert.ToDateTime(param.DateOfBirth) : null;
                    }
                    user.FullName = $"{user.FirstName}{user.LastName}";
                    //user.UserName = param.Email;
                }

                var billing = new List<SubscriberBilling>();
                if (param.Billing.Any())
                {
                    foreach (var rec in param.Billing)
                    {
                        if (rec.ID_Billing > 0)
                        {
                            var previousBill = _context.SubscriberBillings.Find(rec.ID_Billing);
                            previousBill.ID_Company = targetRecord.Id;
                            previousBill.Address1 = rec.Address1;
                            previousBill.Address2 = rec.Address2;
                            previousBill.ID_Country = rec.ID_Country;
                            previousBill.ID_State = rec.ID_State;
                            previousBill.PostalCode = rec.PostalCode;

                            SaveAll();
                        }
                        else
                        {
                            var bill = new SubscriberBilling()
                            {
                                ID_Company = targetRecord.Id,
                                Address1 = rec.Address1,
                                Address2 = rec.Address2,
                                ID_Country = rec.ID_Country,
                                ID_State = rec.ID_State,
                                PostalCode = rec.PostalCode,
                                DateCreated = Constants.GetCurrentDateTime(),
                            };
                            billing.Add(bill);
                        }
                    }
                }
                _context.SubscriberBillings.AddRange(billing);

                var shipping = new List<SubscriberShipping>();
                if (param.Shipping.Any())
                {
                    foreach (var rec in param.Shipping)
                    {
                        if (rec.ID_Shipping > 0)
                        {
                            var previousShipping = _context.SubscriberShippings.Find(rec.ID_Shipping);
                            previousShipping.ID_Company = targetRecord.Id;
                            previousShipping.Address1 = rec.Address1;
                            previousShipping.Address2 = rec.Address2;
                            previousShipping.ID_Country = rec.ID_Country;
                            previousShipping.ID_State = rec.ID_State;
                            previousShipping.PostalCode = rec.PostalCode;

                            SaveAll();
                        }
                        var newShipping = new SubscriberShipping()
                        {
                            ID_Company = targetRecord.Id,
                            Address1 = rec.Address1,
                            Address2 = rec.Address2,
                            ID_Country = rec.ID_Country,
                            ID_State = rec.ID_State,
                            PostalCode = rec.PostalCode,
                            DateCreated = Constants.GetCurrentDateTime(),
                        };
                        shipping.Add(newShipping);
                    }
                }
                _context.SubscriberShippings.AddRange(shipping);

                SaveAll();

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Admin,
                    Action = (int)AuditLogCustomerAction.Update,
                    Description = $"Update User with Company Id {CompanyId} on  {DateTime.Now}",
                    UserId = UserId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);

                targetStatus.id = targetRecord.Id.ToString();
                targetStatus.ReferralCode = targetRecord.ReferralCode;
                targetStatus.Status = "success";
            }
            catch (Exception e)
            {
                targetStatus.Status = "error";
                throw;
            }
            return targetStatus;
        }
        public async Task<bool> SaveUploadSubscription(GetUploadSubscriber model, Guid CompanyId, Guid UserId)
        {
            bool result = false;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (model.SubscriberPlanList.Any())
                    {
                        var SubscriberList = new List<CompanySubscription>();
                        foreach (var item in model.SubscriberPlanList)
                        {
                            var plan = _context.Plans.Where(x => x.PlanId == item.PlanId).FirstOrDefault();
                            if (plan == null)
                            {
                                throw new("Plan not found");
                            }

                            var UpdateCompany = _context.Companies.Where(x => x.Id == item.CompanyId && !x.IsDeleted).FirstOrDefault();
                            if (UpdateCompany == null)
                            {
                                throw new("User not found");
                            }

                            if (UpdateCompany.ID_Subscription > 0)
                            {
                                var checkIfUserSubscriptionStillActive = _context.CompanySubscriptions.Where(x => x.ID_Subscription == UpdateCompany.ID_Subscription).FirstOrDefault();
                                if (checkIfUserSubscriptionStillActive != null)
                                {
                                    if (checkIfUserSubscriptionStillActive.ExpiredDate >= DateTime.Now && checkIfUserSubscriptionStillActive.PaymentStatus == true)
                                    {
                                        throw new($"User with company email {UpdateCompany.Email} still have active plan till " + checkIfUserSubscriptionStillActive.ExpiredDate.Value.ToString("dd/MM/yyyy"));
                                    }
                                }
                            }

                            //check for ReferralCode discount
                            decimal amountAfterDistcount = 0;
                            if (!string.IsNullOrWhiteSpace(UpdateCompany.Ref_ReferralCode))
                            {
                                var isReferralCodeValid = _context.Companies.Where(x => x.ReferralCode.Trim().ToLower() == UpdateCompany.Ref_ReferralCode.Trim().ToLower())
                                            .Select(x => x.ReferralCode).FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(isReferralCodeValid))
                                {
                                    var isReferralCodeEnable = _context.ReferralCodes.Select(x => x.Status).FirstOrDefault();
                                    if (isReferralCodeEnable)
                                    {
                                        var referralCode = _context.ReferralCodes.Where(x => x.Status).FirstOrDefault();

                                        decimal distcount = (referralCode.Percentage / 100) * plan.Amount;
                                        amountAfterDistcount = plan.Amount - distcount;

                                        var rowId = Guid.NewGuid();
                                        var usedReferralCode = new UsedReferralCode()
                                        {
                                            Id = rowId,
                                            ReferralCode = UpdateCompany.Ref_ReferralCode,
                                            UserId = UpdateCompany.Id,
                                            DateCreated = DateTime.Now,
                                            PercentageOffer = referralCode.Percentage,
                                            PlanId = plan.PlanId,
                                            ActualAmount = plan.Amount,
                                            AmountAfterDistcount = Math.Abs(amountAfterDistcount),
                                            IsUsed = true,
                                            TransactionRef = string.Empty,
                                        };
                                        _context.UsedReferralCodes.Add(usedReferralCode);
                                    }
                                }
                            }

                            //TODO::: check for promocode to deduct percentage from the amount

                            var companySubscription = new CompanySubscription
                            {
                                ID_Company = item.CompanyId,
                                ID_Plan = item.PlanId,
                                PlanType = plan.PlanName,
                                Amount = amountAfterDistcount,
                                PaymentStatus = true,
                                IsActive = true,
                                TransactionRef = "Admin-Upload",
                                PaymentMethod = "Admin-Upload",
                                TransactionDate = DateTime.Now,
                                ExpiredDate = plan.PlanDuration <= 1 ? DateTime.Now.AddDays(30) : DateTime.Now.AddYears(1)
                            };
                            _context.CompanySubscriptions.Add(companySubscription);
                            SaveAll();

                            //update company
                            UpdateCompany.ID_Subscription = companySubscription.ID_Subscription;

                            //send payment notification
                            if (await _context.SaveChangesAsync() > 0)
                            {
                                var AuditLog = new AuditLog()
                                {
                                    EntityType = (int)AuditLogEntityType.Admin,
                                    Action = (int)AuditLogCustomerAction.Create,
                                    Description = $"Subscription payment made for user with company {item.CompanyId} on  {DateTime.Now}",
                                    UserId = UserId,
                                    CompanyId = CompanyId,
                                    CreatedOn = DateTime.Now
                                };
                                _context.AuditLogs.Add(AuditLog);

                                var user = _context.Users.Where(x => x.CompanyId == item.CompanyId).FirstOrDefault();
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
                        }
                    }
                    result = true;
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new(ex.Message);
                }
            }
            return result;
        }
        public async Task<bool> SaveUploadSubscriptionNoPayment(GetUploadSubscriber model, Guid CompanyId, Guid UserId)
        {
            bool result = false;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (model.SubscriberPlanList.Any())
                    {
                        var SubscriberList = new List<CompanySubscription>();
                        foreach (var item in model.SubscriberPlanList)
                        {
                            var plan = _context.Plans.Where(x => x.PlanId == item.PlanId).FirstOrDefault();
                            if (plan == null)
                            {
                                throw new("Plan not found");
                            }

                            var UpdateCompany = _context.Companies.Where(x => x.Id == item.CompanyId && !x.IsDeleted).FirstOrDefault();
                            if (UpdateCompany == null)
                            {
                                throw new("User not found");
                            }


                            var companySubscription = new CompanySubscription
                            {
                                ID_Company = item.CompanyId,
                                ID_Plan = item.PlanId,
                                PlanType = plan.PlanName,
                                Amount = 0,
                                PaymentStatus = false,
                                IsActive = false,
                                TransactionRef = "Admin-Upload",
                                PaymentMethod = "Admin-Upload",
                                TransactionDate = DateTime.Now,
                                ExpiredDate = plan.PlanDuration <= 1 ? DateTime.Now.AddDays(30) : DateTime.Now.AddYears(1)
                            };
                            _context.CompanySubscriptions.Add(companySubscription);
                            SaveAll();

                            //update company
                            UpdateCompany.ID_Subscription = companySubscription.ID_Subscription;

                            //send payment notification
                            if (await _context.SaveChangesAsync() > 0)
                            {
                                var AuditLog = new AuditLog()
                                {
                                    EntityType = (int)AuditLogEntityType.Admin,
                                    Action = (int)AuditLogCustomerAction.Create,
                                    Description = $"Subscription with cash payment for company {item.CompanyId} on  {DateTime.Now}",
                                    UserId = UserId,
                                    CompanyId = CompanyId,
                                    CreatedOn = DateTime.Now
                                };
                                _context.AuditLogs.Add(AuditLog);

                                var user = _context.Users.Where(x => x.CompanyId == item.CompanyId).FirstOrDefault();
                                var emailModel = new SubscrberPaymentNotification
                                {
                                    UserName = user.FullName,
                                    UserEmail = user.Email,
                                    Plan = plan.PlanName,
                                    Duration = $"{plan.PlanDuration} Month(s)",
                                    //Amount = plan.Amount.ToString("#,##0.00;(#,##0.00)"),
                                    SubscriptionDate = DateTime.Now.ToString("dd/MM/yyyy")
                                };

                                var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - User Signup ", EmailTemplateEnum.SubscrberPaymentNotification, emailModel);

                            }
                        }
                    }
                    result = true;
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new(ex.Message);
                }
            }
            return result;
        }

        //Billing
        public IQueryable<SubscriberBillingViewModel> GetBillings()
        {
            return _context.SubscriberBillings.AsNoTracking().Select(x => new SubscriberBillingViewModel
            {
                ID_Billing = x.ID_Billing,
                ID_Company = x.ID_Company,
                Address1 = x.Address1,
                Address2 = x.Address2,
                DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                ID_Country = x.ID_Country,
                ID_State = x.ID_State,
                PostalCode = x.PostalCode,
            });
        }
        public IQueryable<SubscriberBillingViewModel> GetBillingsById(int id)
        {
            return _context.SubscriberBillings.AsNoTracking().Where(x => x.ID_Billing == id)
                .Select(x => new SubscriberBillingViewModel
                {
                    ID_Billing = x.ID_Billing,
                    ID_Company = x.ID_Company,
                    Address1 = x.Address1,
                    Address2 = x.Address2,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                    ID_Country = x.ID_Country,
                    ID_State = x.ID_State,
                    PostalCode = x.PostalCode,
                });
        }
        public bool CreateBilling(SubscriberBillingDTO model, Guid CompanyId, Guid UserId)
        {
            var user = _context.Companies.SingleOrDefaultAsync(x => x.Id == model.ID_Company && !x.IsDeleted);
            if (user == null)
            {
                throw new("User not found");
            }
            var newBilling = new SubscriberBilling()
            {
                ID_Company = model.ID_Company,
                Address1 = model.Address1,
                Address2 = model.Address2,
                ID_Country = model.ID_Country,
                ID_State = model.ID_State,
                PostalCode = model.PostalCode
            };
            _context.SubscriberBillings.Add(newBilling);
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Create Billing for company {model.ID_Company} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool UpdateBilling(SubscriberBillingDTO model, Guid CompanyId, Guid UserId)
        {
            var user = _context.Companies.SingleOrDefaultAsync(x => x.Id == model.ID_Company && !x.IsDeleted);
            if (user == null)
            {
                throw new("User not found");
            }
            var targetRecord = _context.SubscriberBillings.Find(model.ID_Billing);
            if (targetRecord == null)
            {
                throw new("Record not found");
            }
            if (targetRecord != null)
            {
                //targetRecord.ID_Company = model.ID_Company;
                targetRecord.Address1 = model.Address1;
                targetRecord.Address2 = model.Address2;
                targetRecord.ID_Country = model.ID_Country;
                targetRecord.ID_State = model.ID_State;
                targetRecord.PostalCode = model.PostalCode;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update Billing id {model.ID_Billing} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool DeleteBilling(int id, Guid CompanyId, Guid UserId)
        {
            var result = false;
            var record = _context.SubscriberBillings.Find(id);
            if (record != null)
            {

                _context.SubscriberBillings.Remove(record);

                SaveAll();

                result = true;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Delete,
                Description = $"Delete Billing id {id} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return result;
        }
        //Shipping
        public IQueryable<SubscriberShippingViewModel> GetShipping()
        {
            return _context.SubscriberShippings.AsNoTracking().Select(x => new SubscriberShippingViewModel
            {
                ID_Shipping = x.ID_Shipping,
                ID_Company = x.ID_Company,
                Address1 = x.Address1,
                Address2 = x.Address2,
                DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                ID_Country = x.ID_Country,
                ID_State = x.ID_State,
                PostalCode = x.PostalCode,
            });
        }
        public IQueryable<SubscriberShippingViewModel> GetShippingById(int id)
        {
            return _context.SubscriberShippings.AsNoTracking().Where(x => x.ID_Shipping == id)
                .Select(x => new SubscriberShippingViewModel
                {
                    ID_Shipping = x.ID_Shipping,
                    ID_Company = x.ID_Company,
                    Address1 = x.Address1,
                    Address2 = x.Address2,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                    ID_Country = x.ID_Country,
                    ID_State = x.ID_State,
                    PostalCode = x.PostalCode,
                });
        }
        public bool CreateShipping(SubscriberShippingDTO model, Guid CompanyId, Guid UserId)
        {
            var user = _context.Companies.SingleOrDefaultAsync(x => x.Id == model.ID_Company && !x.IsDeleted);
            if (user == null)
            {
                throw new("User not found");
            }
            var newShipping = new SubscriberShipping()
            {
                ID_Company = model.ID_Company,
                Address1 = model.Address1,
                Address2 = model.Address2,
                ID_Country = model.ID_Country,
                ID_State = model.ID_State,
                PostalCode = model.PostalCode
            };
            _context.SubscriberShippings.Add(newShipping);
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Create Shipping for company {model.ID_Company} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool UpdateShipping(SubscriberShippingDTO model, Guid CompanyId, Guid UserId)
        {
            var user = _context.Companies.SingleOrDefaultAsync(x => x.Id == model.ID_Company && !x.IsDeleted);
            if (user == null)
            {
                throw new("User not found");
            }
            var targetRecord = _context.SubscriberShippings.Find(model.ID_Shipping);
            if (targetRecord == null)
            {
                throw new("Record not found");
            }
            if (targetRecord != null)
            {
                //targetRecord.ID_Company = model.ID_Company;
                targetRecord.Address1 = model.Address1;
                targetRecord.Address2 = model.Address2;
                targetRecord.ID_Country = model.ID_Country;
                targetRecord.ID_State = model.ID_State;
                targetRecord.PostalCode = model.PostalCode;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update Shipping with id {model.ID_Shipping} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool DeleteShipping(int id, Guid CompanyId, Guid UserId)
        {
            var result = false;
            var record = _context.SubscriberShippings.Find(id);
            if (record != null)
            {
                _context.SubscriberShippings.Remove(record);

                SaveAll();

                result = true;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Delete,
                Description = $"Delete Shipping with id {id} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return result;
        }
        //Account enable and disable
        public bool EnableSubscriber(Guid companyId)
        {
            var result = false;
            var targetSubscriber = _context.Companies.Where(x => x.Id == companyId).FirstOrDefault();
            if (targetSubscriber.ID_Subscription > 0)
            {
                var subscriber = _context.CompanySubscriptions.Where(x => x.ID_Subscription == targetSubscriber.ID_Subscription).FirstOrDefault();
                if (subscriber.IsActive)
                {
                    subscriber.IsActive = false;
                }
                else
                {
                    subscriber.IsActive = true;
                }
                SaveAll();
                result = true;
            }
            else
            {
                result = false;
                throw new("This customer has not yet subcribe");
            }
            return result;
        }
        //Note
        public IQueryable<SubscriberNoteViewModel> GetNotes()
        {
            return _context.SubscriberNotes
                .Select(x => new SubscriberNoteViewModel
                {
                    ID_Note = x.ID_Note,
                    CompanyId = x.CompanyId,
                    Description = x.Description,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy")
                }).OrderByDescending(x => x.ID_Note);
        }
        public IQueryable<SubscriberNoteViewModel> GetNote(int noteId)
        {
            return _context.SubscriberNotes
                .Where(x => x.ID_Note == noteId)
                .Select(x => new SubscriberNoteViewModel
                {
                    ID_Note = x.ID_Note,
                    CompanyId = x.CompanyId,
                    Description = x.Description,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy")
                });
        }
        public IQueryable<SubscriberNoteViewModel> GetCompanyNote(Guid companyId)
        {
            return _context.SubscriberNotes
                .Where(x => x.CompanyId == companyId)
                .Select(x => new SubscriberNoteViewModel
                {
                    ID_Note = x.ID_Note,
                    CompanyId = x.CompanyId,
                    Description = x.Description,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy")
                }).OrderByDescending(x => x.ID_Note);
        }
        public bool AddNote(NoteRequest param, Guid CompanyId, Guid UserId)
        {
            var note = new SubscriberNote()
            {
                CompanyId = param.CompanyId,
                Description = param.Description,
                DateCreated = Constants.GetCurrentDateTime(),
            };
            _context.SubscriberNotes.Add(note);
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Create note Id {param.noteId} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool UpdateNote(NoteRequest param, Guid CompanyId, Guid UserId)
        {
            var targetNote = _context.SubscriberNotes.Find(param.noteId);
            if (targetNote != null)
            {
                targetNote.CompanyId = param.CompanyId;
                targetNote.Description = param.Description;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update note Id {param.noteId} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool DeleteNote(int id, Guid CompanyId, Guid UserId)
        {
            var result = false;
            var targetNote = _context.SubscriberNotes.Find(id);
            if (targetNote != null)
            {
                _context.SubscriberNotes.Remove(targetNote);

                SaveAll();

                result = true;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Delete,
                Description = $"Delete note Id {id} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return result;
        }
        //notification
        public IQueryable<SubscriberNotificationViewModel> GetNotefications(FilterAdminNotification filter = null, PaginationFilter paginationFilter = null)
        {
            var notification = (from no in _context.SubscriberNotifications
                                select new SubscriberNotificationViewModel
                                {
                                    ID_Notification = no.ID,
                                    CompanyId = no.CompanyId,
                                    Description = no.Description,
                                    DateCreated = no.DateCreated.ToString("dd/MM/yyyy"),
                                    TimeCreated = no.DateCreated.ToString(@"hh\:mm"),
                                    ReminderDate = no.ReminderDate.ToString("dd/MM/yyyy"),
                                    ReminderTime = no.ReminderDate.ToString(@"hh\:mm"),
                                    IsRead = no.IsRead,
                                    DateRead = no.DateRead.Value.ToString("dd/MM/yyyy"),
                                    TimeRead = no.DateRead.Value.ToString(@"hh\:mm"),
                                });

            if (string.IsNullOrEmpty(filter.SortBy)) notification = notification.OrderByDescending(x => x.ID_Notification);
            else notification = notification.OrderBy(filter.SortByAndOrder);

            if (paginationFilter == null)
            {
                return notification;
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return notification.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<SubscriberNotificationViewModel> GetNotefication(Guid notificationId)
        {
            return _context.SubscriberNotifications
                .Where(x => x.ID == notificationId)
                .Select(x => new SubscriberNotificationViewModel
                {
                    ID_Notification = x.ID,
                    CompanyId = x.CompanyId,
                    Description = x.Description,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                    TimeCreated = x.DateCreated.ToString(@"hh\:mm"),
                    ReminderDate = x.ReminderDate.ToString("dd/MM/yyyy"),
                    ReminderTime = x.ReminderDate.ToString(@"hh\:mm"),
                    IsRead = x.IsRead,
                    DateRead = x.DateRead.Value.ToString("dd/MM/yyyy"),
                    TimeRead = x.DateRead.Value.ToString(@"hh\:mm"),
                }).OrderByDescending(x => x.ID_Notification);
        }
        public IQueryable<SubscriberNotificationViewModel> GetCompanyNotefication(Guid companyId)
        {
            return _context.SubscriberNotifications
                .Where(x => x.CompanyId == companyId)
                .Select(x => new SubscriberNotificationViewModel
                {
                    ID_Notification = x.ID,
                    CompanyId = x.CompanyId,
                    Description = x.Description,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                    TimeCreated = x.DateCreated.ToString(@"hh\:mm"),
                    ReminderDate = x.ReminderDate.ToString("dd/MM/yyyy"),
                    ReminderTime = x.ReminderDate.ToString(@"hh\:mm"),
                    IsRead = x.IsRead,
                    DateRead = x.DateRead.Value.ToString("dd/MM/yyyy"),
                    TimeRead = x.DateRead.Value.ToString(@"hh\:mm"),
                }).OrderByDescending(x => x.ID_Notification);
        }
        public async Task<bool> AddNotification(NotificationRequest param, Guid CompanyId, Guid UserId)
        {
            var result = false;
            if (!string.IsNullOrEmpty(param.ReminderDate))
            {
                var ReminderDate = Convert.ToDateTime(param.ReminderDate).Date;
                var time = param.ReminderTime.TimeOfDay;
                ReminderDate = ReminderDate.Add(param.ReminderTime.TimeOfDay);
                var scheduleDateInUtc = ReminderDate; //.ToUniversalTime();
                var currentDateTimeUtc = DateTime.Now; //Constants.GetCurrentDateTime(TimeZoneInfo.Utc);
                if (scheduleDateInUtc < currentDateTimeUtc)
                    throw new("Reminder date and time must be a future date/time");

                var customer = await _context.Companies.SingleOrDefaultAsync(x => x.Id == param.CompanyId && !x.IsDeleted);

                if (customer == null)
                {
                    throw new("User not found");
                }
                var rowId = Guid.NewGuid();
                var Notification = new SubscriberNotification()
                {
                    ID = rowId,
                    CompanyId = param.CompanyId,
                    Description = param.Description,
                    IsRead = false,
                    IsDeleted = false,
                    DateCreated = Constants.GetCurrentDateTime(),
                    ReminderDate = scheduleDateInUtc,
                    CreatedBy = UserId,
                    TimeCreated = default(DateTime).Add(Constants.GetCurrentDateTime(TimeZoneInfo.Utc).TimeOfDay),
                    Comments = "Created on " + System.DateTime.Now + " " + Environment.NewLine + "==================" + Environment.NewLine,
                };
                _context.SubscriberNotifications.Add(Notification);

                //schedule reminder
                TimeSpan span = scheduleDateInUtc - currentDateTimeUtc;
                double totalMinutes = span.TotalMinutes;
                _scheduler.Schedule(new NotificationParam
                {
                    CompanyId = customer.Id,
                    ReminderId = Notification.ID
                }, TimeSpan.FromMinutes(totalMinutes)
                    , $"Customer Reminder {customer.Email}");

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Subscription,
                    Action = (int)AuditLogCustomerAction.Create,
                    Description = $"Create notification with id {rowId} at  {DateTime.Now}",
                    UserId = UserId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);

                result = SaveAll();

            }
            return result;
        }
        public async Task<bool> UpdateNotification(NotificationRequest param, Guid CompanyId, Guid UserId)
        {
            var result = false;
            if (!string.IsNullOrEmpty(param.ReminderDate))
            {
                var ReminderDate = Convert.ToDateTime(param.ReminderDate).Date;
                var time = param.ReminderTime.TimeOfDay;
                ReminderDate = ReminderDate.Add(param.ReminderTime.TimeOfDay);
                var scheduleDateInUtc = ReminderDate; //.ToUniversalTime();
                var currentDateTimeUtc = DateTime.Now; //Constants.GetCurrentDateTime(TimeZoneInfo.Utc);
                if (scheduleDateInUtc < currentDateTimeUtc)
                    throw new("Reminder date and time must be a future date/time");

                var reminder = await _context.Companies.SingleOrDefaultAsync(x => x.Id == param.CompanyId && !x.IsDeleted);

                if (reminder == null)
                {
                    throw new("Reminder not found");
                }

                TimeSpan span = scheduleDateInUtc - currentDateTimeUtc;
                double totalMinutes = span.TotalMinutes;

                var targetNotification = _context.SubscriberNotifications.Find(param.NotificationId);
                var prComment = targetNotification.Comments;
                if (targetNotification != null)
                {
                    targetNotification.CompanyId = param.CompanyId;
                    targetNotification.Description = param.Description;
                    targetNotification.ReminderDate = scheduleDateInUtc;
                    targetNotification.Comments = Environment.NewLine + "modify on " + Constants.GetCurrentDateTime(TimeZoneInfo.Utc) + " " + Environment.NewLine + "==================" + Environment.NewLine + prComment;
                }
                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Subscription,
                    Action = (int)AuditLogCustomerAction.Update,
                    Description = $"Update notification id {param.NotificationId} at  {DateTime.Now}",
                    UserId = UserId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);
                result = SaveAll();
            }
            return result;
        }

        public int CountNotifications()
        {
            var nonTriggercount = _context.SubscriberNotifications
                .Where(m => !m.IsRead).Count();

            return nonTriggercount;
        }
        public bool DeleteNotification(int id, Guid CompanyId, Guid UserId)
        {
            var result = false;
            var targetNotification = _context.SubscriberNotifications.Find(id);
            if (targetNotification != null)
            {
                _context.SubscriberNotifications.Remove(targetNotification);

                SaveAll();

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Subscription,
                    Action = (int)AuditLogCustomerAction.Delete,
                    Description = $"Delete notification id {id} at  {DateTime.Now}",
                    UserId = UserId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);

                result = true;
            }
            return result;
        }

        //Plan
        public IQueryable<PlanViewModel> GetPlans(FilterPlan filter = null, PaginationFilter paginationFilter = null)
        {
            var Query = _context.Plans.AsNoTracking().Select(x => new PlanViewModel
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

            if (!string.IsNullOrEmpty(filter.Search)) Query = Query.Where(x => x.PlanName.ToLower().Contains(filter.Search.ToLower())
                                                                                   || x.Description.ToLower().Contains(filter.Search.ToLower()));

            if (string.IsNullOrEmpty(filter.SortBy)) Query = Query.OrderByDescending(x => x.PlanId);
            else Query = Query.OrderBy(filter.SortByAndOrder);

            if (paginationFilter == null)
            {
                return Query;
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return Query.Skip(skip).Take(paginationFilter.PageSize);
        }

        public IQueryable<PlanViewModel> GetPlanById(int Id)
        {
            return _context.Plans.AsNoTracking()
                .Where(x => x.PlanId == Id)
                .Select(x => new PlanViewModel
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
        public bool AddPlan(AddPlanViewModel model, Guid CompanyId, Guid UserId)
        {
            var newPlan = new Plan()
            {
                PlanName = model.PlanName,
                Description = model.Description,
                Amount = model.Amount,
                PlanDuration = model.PlanDuration == null ? 0 : model.PlanDuration.Value,
                IsFreePlan = model.IsFreePlan,
                IncludePromotion = model.IncludePromotion,
                Status = model.Status,
            };

            _context.Plans.Add(newPlan);

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Create new Plan {model.Description} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();
        }
        public bool UpdatePlan(int PlanId, AddPlanViewModel model, Guid CompanyId, Guid UserId)
        {
            bool result = false;

            if (PlanId > 0)
            {
                var targetPlan = _context.Plans.Find(PlanId);

                if (targetPlan != null)
                {
                    targetPlan.PlanName = model.PlanName;
                    targetPlan.Amount = model.Amount;
                    targetPlan.PlanDuration = model.PlanDuration == null ? 0 : model.PlanDuration.Value;
                    targetPlan.IsFreePlan = model.IsFreePlan;
                    targetPlan.Status = model.Status;
                    targetPlan.IncludePromotion = model.IncludePromotion;
                    targetPlan.Description = model.Description;

                    SaveAll();

                    result = true;
                }
            }

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update Plan Id {PlanId} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return result;
        }
        public bool DeletePlan(int id, Guid CompanyId, Guid UserId)
        {
            var result = false;

            if (_context.CompanySubscriptions.Where(x => x.ID_Plan == id).Any())
            {
                throw new("Plan already in use");
            }
            var record = _context.Plans.Find(id);
            if (record != null)
            {

                _context.Plans.Remove(record);

                SaveAll();

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Admin,
                    Action = (int)AuditLogCustomerAction.Delete,
                    Description = $"Delete Plan Id {id} on  {DateTime.Now}",
                    UserId = UserId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);

                result = true;
            }

            return result;
        }

        public bool TogglePlan(int Id)
        {
            var targetRecord = _context.Plans.Where(x => x.PlanId == Id).FirstOrDefault();
            if (targetRecord == null)
            {
                throw new("Id not found");
            }
            if (targetRecord.Status.HasValue)
            {
                if (targetRecord.Status == false)
                {
                    targetRecord.Status = true;
                }
                else
                {
                    targetRecord.Status = false;
                }

            }
            else
            {
                targetRecord.Status = true;
            }
            return SaveAll();
        }

        //Permission
        public List<RoleClaimsViewModel> GetPermissions()//GroupedModel //UserPermissionsViewModel
        {
            //var allPermissions = mapper.Map<List<ModelPermissions>>(permissionHelper.GetAllPermissions());

            //var desc = allPermissions.Select(x => x.Description).ToList();

            var allPermissions = new List<RoleClaimsViewModel>();

            allPermissions.GetPermissions(typeof(Permissions), "");



            return allPermissions;
        }


        //Role
        public async Task<IQueryable<ListRoleViewModel>> GetRoles(RolePostFilter filter = null)
        {
            var Roles = new List<ListRoleViewModel>();
            var allPermissions = new List<RoleClaimsViewModel>();

            var Ids = _context.Roles.Select(x => x.Id).ToArray();
            foreach (var id in Ids)
            {
                var role = await roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    continue;
                }

                var model = new ListRoleViewModel
                {
                    ID = role.Id,
                    Role = role.Name,
                    Description = role.Description,
                    Expose = role.Expose == null ? false : role.Expose.Value,
                    IsSubscriberRole = role.IsSystemDefined
                };

                //allPermissions.GetPermissions(typeof(Permissions), role.ToString());

                //var claims = await roleManager.GetClaimsAsync(role);
                //var allClaimValues = allPermissions.Select(a => a.Value).ToList();
                //var roleClaimValues = claims.Select(a => a.Value).ToList();
                //var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
                //foreach (var permission in allPermissions)
                //{
                //    if (authorizedClaims.Any(a => a == permission.Value))
                //    {
                //        permission.Selected = true;
                //    }
                //    //model.permission = allPermissions;

                //}
                //model.permission = allPermissions.Where(x=>x.Selected == true).ToList();


                Roles.Add(model);

            }
            var RoleQuery = Roles.AsQueryable();
            if (string.IsNullOrEmpty(filter.SortBy)) RoleQuery = RoleQuery.OrderByDescending(x => x.ID);
            else RoleQuery = RoleQuery.OrderBy(filter.SortByAndOrder);
            return RoleQuery;
        }
        public async Task<UserRoleViewModel> GetRoleById(Guid roleId)
        {
            var allPermissions = new List<RoleClaimsViewModel>();

            var role = await roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                throw new($"Role with Id = {roleId} cannot be found");
            }
            var existingRoleClaims = await roleManager.GetClaimsAsync(role);

            var model = new UserRoleViewModel
            {
                ID = roleId,
                Role = role.Name,
                Description = role.Description,
                IsSubscriberRole = role.IsSystemDefined
            };


            allPermissions.GetPermissions(typeof(Permissions), roleId.ToString());

            var claims = await roleManager.GetClaimsAsync(role);
            var allClaimValues = allPermissions.Select(a => a.value.ToString()).ToList();
            var roleClaimValues = claims.Select(a => a.Value).ToList();
            var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
            foreach (var permission in allPermissions)
            {
                if (authorizedClaims.Any(a => a == permission.value.ToString()))
                {
                    permission.selected = true;
                }
            }
            model.permission = allPermissions.Where(x => x.selected == true).ToList();

            return model;
        }
        public async Task<IQueryable<GetDropDowmRole>> GetSlimRoles()
        {
            var RoleList = new List<GetDropDowmRole>();
            var roles = await roleManager.Roles.ToListAsync();
            if (roles == null)
            {
                return RoleList.AsQueryable();
            }
            var record = (from com in roles
                          select new GetDropDowmRole
                          {
                              roleId = com.Id,
                              roleName = com.Name
                          });

            var RoleQuery = record.AsQueryable();
            return RoleQuery;
        }
        public async Task<IQueryable<GetDropDowmRole>> GetSubscriberRoles()
        {
            var RoleList = new List<GetDropDowmRole>();
            var roles = await roleManager.Roles.Where(x => x.IsSystemDefined).ToListAsync();
            if (roles == null)
            {
                return RoleList.AsQueryable();
            }
            var record = (from com in roles
                          select new GetDropDowmRole
                          {
                              roleId = com.Id,
                              roleName = com.Name
                          });

            var RoleQuery = record.AsQueryable();
            return RoleQuery;
        }

        public async Task<IQueryable<GetDropDowmRole>> GetAdminRoles()
        {
            var RoleList = new List<GetDropDowmRole>();
            var roles = await roleManager.Roles.Where(x => !x.IsSystemDefined).ToListAsync();
            if (roles == null)
            {
                return RoleList.AsQueryable();
            }
            var record = (from com in roles
                          select new GetDropDowmRole
                          {
                              roleId = com.Id,
                              roleName = com.Name
                          });

            var RoleQuery = record.AsQueryable();
            return RoleQuery;
        }
        public async Task<bool> AddRole(RoleViewModel model, Guid CompanyId, Guid UserId)
        {
            bool response = false;
            try
            {
                if (await roleManager.RoleExistsAsync(model.Role))
                {
                    throw new("Role already exist");
                }

                //Check permission
                var allPermissions = mapper.Map<List<ModelPermissions>>(permissionHelper.GetAllPermissions());


                var role = mapper.Map<ApplicationRole>(model);
                role.CreatedOn = Constants.GetCurrentDateTime();
                role.CreatedBy = CompanyId;
                role.Description = model.Description;
                _context.Roles.Add(role);
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new("Unable to add role. Please try again");
                }

                var selectedClaims = model.RoleClaims.Where(a => a.selected).ToList();
                foreach (var claim in selectedClaims)
                {
                    var allClaims = await roleManager.GetClaimsAsync(role);
                    if (!allClaims.Any(a => a.Type == Constants.PermissionClaim && a.Value == claim.value.ToString()))
                    {
                        await roleManager.AddClaimAsync(role, new Claim(Constants.PermissionClaim, claim.value.ToString()));
                    }
                }
                response = true;

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Admin,
                    Action = (int)AuditLogCustomerAction.Update,
                    Description = $"Create {role.Name} role on  {DateTime.Now}",
                    UserId = CompanyId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);
            }
            catch (Exception)
            {
                response = false;
            }
            return response;
        }
        public async Task<bool> UpdateRole(Guid Id, RoleViewModel model, Guid CompanyId, Guid UserId)
        {
            bool successResponse = false;
            try
            {
                //Check permission
                var allPermissions = mapper.Map<List<ModelPermissions>>(permissionHelper.GetAllPermissions());

                var role = await roleManager.FindByIdAsync(Id.ToString());
                if (role == null)
                {
                    throw new("Row not fund");
                }

                role.Name = model.Role;
                role.Description = model.Description;
                role.IsSystemDefined = model.IsSubscriberRole;
                role.ModifiedOn = Constants.GetCurrentDateTime();
                var result = await roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    throw new("Unable to update role. Please try again");
                }


                var claims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    await roleManager.RemoveClaimAsync(role, claim);
                }
                var selectedClaims = model.RoleClaims.Where(a => a.selected).ToList();
                foreach (var claim in selectedClaims)
                {
                    var allClaims = await roleManager.GetClaimsAsync(role);
                    if (!allClaims.Any(a => a.Type == Constants.PermissionClaim && a.Value == claim.value.ToString()))
                    {
                        await roleManager.AddClaimAsync(role, new Claim(Constants.PermissionClaim, claim.value.ToString()));
                    }
                }
                successResponse = true;

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Admin,
                    Action = (int)AuditLogCustomerAction.Update,
                    Description = $"Update {role.Name} role on  {DateTime.Now}",
                    UserId = CompanyId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);
            }
            catch (Exception)
            {
                successResponse = false;
            }
            return successResponse;
        }
        public async Task<bool> DeleteRole(Guid id, Guid CompanyId, Guid UserId)
        {
            bool result = false;
            var role = await roleManager.FindByIdAsync(id.ToString());
            var existingRoleClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claims in existingRoleClaims)
            {
                var response = await roleManager.RemoveClaimAsync(role, claims);
                if (!response.Succeeded)
                {
                    throw new($"Cannot remove existing claims for {role.Name} role ");
                }
            }

            var getRole = _context.Roles.Where(x => x.Id == id).FirstOrDefault();
            if (getRole != null)
            {
                _context.Roles.Remove(getRole);
                _context.SaveChanges();
                result = true;
            }

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Delete,
                Description = $"Delete {getRole.Name}crole on  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return result;
        }
        public bool ToggleRole(Guid Id)
        {
            var targetRecord = _context.Roles.Where(x => x.Id == Id).FirstOrDefault();
            if (targetRecord == null)
            {
                throw new("Id not found");
            }
            if (targetRecord.Expose.HasValue)
            {
                if (targetRecord.Expose == false)
                {
                    targetRecord.Expose = true;
                }
                else
                {
                    targetRecord.Expose = false;
                }

            }
            else
            {
                targetRecord.Expose = true;
            }
            return SaveAll();
        }



        //User
        public async Task<Response> CreateUser(UserVM model, Guid CompanyId, Guid UserId)
        {
            try
            {
                if (await _context.Users.AnyAsync(x => (x.UserName.ToLower() == model.UserName.ToLower() || x.Email.ToLower() == model.Email.ToLower())))
                    throw new("UserName or email is already taken");

                if (await userManager.FindByEmailAsync(model.Email) != null)
                {
                    throw new("Email is already in use");
                }

                //check if role is expose to user
                var role = await roleManager.FindByIdAsync(model.Role.ToString());
                if (role == null)
                {
                    throw new("Role does not exist");
                }
                if (role.Expose.HasValue)
                {
                    if (!role.Expose.Value)
                    {
                        throw new("Role not expose to user. Please contact administrator");
                    }
                }
                else
                {
                    throw new("Role not expose to user. Please contact administrator");
                }
                

                var password = EnumExtensionHelper.GenerateRandomString(6, true, false) + "6@";
                var user = mapper.Map<ApplicationUser>(model);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.IsActive = true;
                user.IsBusinessOwner = false;
                user.CreatedBy = user.Id;
                user.CreatedOn = Constants.GetCurrentDateTime();
                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    throw new("Unable to complete signup, Please try again");
                }
                var _role = user.RoleId;
                var ownerRole = _context.Roles.Where(x => x.Id == user.RoleId && !x.IsDeleted).FirstOrDefault();
                if (ownerRole != null)
                {
                    await userManager.AddToRoleAsync(user, ownerRole.Name);
                }
                var loginToken = await JwtHelper.GenerateJWToken(jwtSettings, user); //JwtHelper.GenerateJWToken(userManager, roleManager, jwtSettings, user, "");
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var webUrl = configuration["SpineAdmin"];

                var emailModel = new UserSignup
                {
                    ActionLink = Constants.GetConfirmAccountLink(webUrl, code),
                    Date = DateTime.Now.ToString("dd/MM/yyyy"), //Constants.GetCurrentDateTime().ToLongDateString(),
                    Name = model.UserName,
                    UserName = model.Email,
                    Password = password,
                };
                var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - User Signup ", EmailTemplateEnum.UserSignup, emailModel);

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Admin,
                    Action = (int)AuditLogCustomerAction.Create,
                    Description = $"Add new user {model.FirstName}{model.LastName} on  {DateTime.Now}",
                    UserId = CompanyId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);

                return new Response(HttpStatusCode.Created, new JwtSecurityTokenHandler().WriteToken(loginToken));
            }
            catch (Exception ex)
            {
                throw new(ex.Message);
            }
        }
        public bool UpdateUser(Guid Id, UpdateUserVM model, Guid CompanyId, Guid UserId)
        {

            var previousRecord = _context.Users.Where(x => x.Id == Id).FirstOrDefault();
            if (previousRecord == null)
            {
                throw new("No record found for this user");
            }
            var usrRole = _context.Roles.Where(x => x.Id == model.Role).FirstOrDefault();
            if (usrRole == null)
            {
                throw new("Role does not exist");
            }
            //check if role is expose to user
            if (usrRole.Expose.HasValue)
            {
                if (!usrRole.Expose.Value)
                {
                    throw new("Role not expose to user. Please contact administrator");
                }
            }
            else
            {
                throw new("Role not expose to user. Please contact administrator");
            }
            if (previousRecord != null)
            {
                if (!string.IsNullOrEmpty(model.FirstName))
                {
                    previousRecord.FirstName = model.FirstName;
                }
                if (!string.IsNullOrEmpty(model.LastName))
                {
                    previousRecord.LastName = model.LastName;
                }
                previousRecord.IsActive = model.IsActive;

                previousRecord.RoleId = model.Role;
            }

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update user {model.FirstName}{model.LastName} on  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();

        }
        public bool RoleTransfer(RoleTransferVM model, Guid CompanyId, Guid UserId)
        {
            var FromUser = _context.Users.Where(x => x.Id == model.FromUserId).FirstOrDefault();
            if (FromUser == null)
            {
                throw new($"No record found for this user {model.FromUserId}");
            }
            var ToUser = _context.Users.Where(x => x.Id == model.ToUserId).FirstOrDefault();
            if (ToUser == null)
            {
                throw new($"No record found for this user {model.ToUserId}");
            }
            var usrRole = _context.Roles.Where(x => x.Id == FromUser.RoleId).FirstOrDefault();
            if (usrRole == null)
            {
                throw new($"Role does not exist for {model.FromUserId}");
            }
            if (ToUser != null)
            {
                ToUser.RoleId = FromUser.RoleId;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Transfer role from {FromUser.UserName} to {ToUser.UserName} on  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public IQueryable<ListUserVM> GetUsers(PostUserFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var Query = (from u in _context.Users
                         join r in _context.Roles on u.RoleId equals r.Id
                         select new ListUserVM
                         {
                             ID_User = u.Id,
                             FirstName = u.FirstName,
                             UserName = u.UserName,
                             LastName = u.LastName,
                             Email = u.Email,
                             CreatedOn = u.CreatedOn.ToString("dd/MM/yyyy"),
                             GetCreatedOn = u.CreatedOn,
                             RoleId = u.RoleId,
                             Status = u.IsActive == false ? "In-Active" : "Active",
                             Role = r.Name,
                         });
            if (!string.IsNullOrEmpty(filter?.UserName))
            {
                Query = Query.Where(x => x.UserName == filter.UserName);
            }
            if (!string.IsNullOrEmpty(filter?.Role))
            {
                Query = Query.Where(x => x.Role.Trim().ToLower() == filter.Role.Trim().ToLower());
            }
            if (!string.IsNullOrEmpty(filter?.CreatedOn))
            {
                Query = Query.Where(x => x.CreatedOn == filter.CreatedOn);
            }
            if (!string.IsNullOrEmpty(filter?.Status))
            {
                Query = Query.Where(x => x.Status == filter.Status);
            }
            if (string.IsNullOrEmpty(filter?.SortBy)) Query = Query.OrderByDescending(x => x.GetCreatedOn);
            else Query = Query.OrderBy(filter?.SortByAndOrder);

            if (paginationFilter == null)
            {
                return Query.OrderByDescending(x => x.GetCreatedOn);
            }
            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return Query.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<ListUserVM> GetUser(Guid Id)
        {
            return (from u in _context.Users
                    join r in _context.Roles on u.RoleId equals r.Id
                    where u.Id == Id
                    select new ListUserVM
                    {
                        ID_User = u.Id,
                        FirstName = u.FirstName,
                        UserName = u.UserName,
                        LastName = u.LastName,
                        Email = u.Email,
                        CreatedOn = u.CreatedOn.ToString("dd/MM/yyyy"),
                        RoleId = u.RoleId,
                        Status = u.IsActive == false ? "In-Active" : "Active",
                        Role = r.Name
                    });
        }
        public bool DeleteUser(Guid id, Guid CompanyId, Guid UserId)
        {
            bool result = false;

            var getUser = _context.Users.Where(x => x.Id == id).FirstOrDefault();
            if (getUser != null)
            {
                _context.Users.Remove(getUser);
                _context.SaveChanges();
                result = true;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Delete,
                Description = $"Delete user {getUser.FirstName}{getUser.LastName} on  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return result;
        }

        public bool ToggleUser(Guid Id)
        {
            var targetRecord = _context.Users.Where(x => x.Id == Id).FirstOrDefault();
            if (targetRecord == null)
            {
                throw new("Id not found");
            }
            if (targetRecord.IsActive)
            {
                targetRecord.IsActive = false;
            }
            else
            {
                targetRecord.IsActive = true;
            }
            return SaveAll();
        }
        //Audit log
        public IQueryable<AuditLogViewModel> GetAuditLog(FilterAuditLog filter = null, PaginationFilter paginationFilter = null)
        {
            var Query = _context.AuditLogs.AsNoTracking().Select(x => new AuditLogViewModel
            {
                Id = x.Id,
                UserId = x.UserId,
                Username = _context.Users.Where(z => z.Id == x.UserId).Select(z => z.UserName).FirstOrDefault(),
                Email = _context.Users.Where(z => z.Id == x.UserId).Select(z => z.Email).FirstOrDefault(),
                Action = x.Action,
                CompanyId = x.CompanyId,
                Description = x.Description,
                Device = x.Device,
                EntityType = x.EntityType,
                MACAddress = x.MACAddress,
                Time = x.CreatedOn.ToString(@"hh\:mm"),
                CreatedOn = x.CreatedOn.ToString("dd/MM/yyyy"),
                GetCreatedOn = x.CreatedOn,
            });

            if (!string.IsNullOrEmpty(filter?.Username))
            {
                Query = Query.Where(x => x.Username.Trim() == filter.Username.Trim().ToLower());
            }
            if (!string.IsNullOrEmpty(filter?.Device))
            {
                Query = Query.Where(x => x.Device.Trim() == filter.Device.Trim().ToLower());
            }
            if (filter.StartDate.HasValue) Query = Query.Where(x => x.GetCreatedOn >= filter.StartDate);
            if (filter.EndDate.HasValue) Query = Query.Where(x => x.GetCreatedOn.Date <= filter.EndDate);
            if (!string.IsNullOrEmpty(filter.Search)) Query = Query.Where(x => x.Username.ToLower().Contains(filter.Search.ToLower())
                                                                                   || x.Device.ToLower().Contains(filter.Search.ToLower())
                                                                                   || x.Description.ToLower().Contains(filter.Search.ToLower()));

            if (string.IsNullOrEmpty(filter.SortBy)) Query = Query.OrderByDescending(x => x.GetCreatedOn);
            else Query = Query.OrderBy(filter.SortByAndOrder);

            if (paginationFilter == null)
            {
                return Query.OrderByDescending(x => x.GetCreatedOn);
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return Query.Skip(skip).Take(paginationFilter.PageSize);
        }

        //Document template
        public IQueryable<DocumentTemplateViewModel> GetTemplates()
        {
            return _context.Templates.AsNoTracking().Select(x => new DocumentTemplateViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Subject = x.Subject,
                Body = x.Body,
                CreatedOn = x.CreatedOn.ToString("dd/MM/yyyy")
            });
        }
        public bool CreateTemplate(CreateTemplateViewModel model)
        {
            var template = new DocumentTemplate()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Subject = model.Subject,
                Body = model.Body,
                CreatedOn = DateTime.Now,
            };
            _context.Templates.Add(template);

            return SaveAll();
        }
        public bool UpdateTemplate(Guid Id, CreateTemplateViewModel model)
        {
            var template = _context.Templates.Where(x => x.Id == Id).FirstOrDefault();
            if (template == null)
            {
                throw new("Template Id not found");
            }
            template.Name = model.Name;
            template.Subject = model.Subject;
            template.Body = model.Body;

            return SaveAll();
        }
        public bool DeleteTemplate(Guid Id)
        {
            var result = false;
            var template = _context.Templates.Where(x => x.Id == Id).FirstOrDefault();
            if (template == null)
            {
                throw new("Template Id not found");
            }
            if (template != null)
            {

                _context.Templates.Remove(template);

                SaveAll();

                result = true;
            }

            return result;
        }

        //Notification Path
        public IQueryable<NotificationPathViewModel> GetNotificationPath()
        {
            return _context.NotificationPaths.AsNoTracking().Select(x => new NotificationPathViewModel
            {
                Id = x.Id,
                PathDesscription = x.PathDesscription,
                IsActive = x.IsActive
            });
        }
        public bool CreateNotificationPath(CreateNotificationPathViewModel model, Guid CompanyId, Guid UserId)
        {
            var notificationPath = new NotificationPath()
            {
                Id = Guid.NewGuid(),
                PathDesscription = model.PathName,
                IsActive = model.IsActive
            };
            _context.NotificationPaths.Add(notificationPath);
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Create new Notification Path {model.PathName} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool UpdateNotificationPath(Guid Id, CreateNotificationPathViewModel model, Guid CompanyId, Guid UserId)
        {
            var notification = _context.NotificationPaths.Where(x => x.Id == Id).FirstOrDefault();
            if (notification == null)
            {
                throw new("Id not found");
            }
            notification.PathDesscription = model.PathName;
            notification.IsActive = model.IsActive;
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update Notification Path Id {Id} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }
        public bool DeleteNotificationPath(Guid Id, Guid CompanyId, Guid UserId)
        {
            var result = false;
            var notification = _context.NotificationPaths.Where(x => x.Id == Id).FirstOrDefault();
            if (notification == null)
            {
                throw new("Id not found");
            }
            if (notification != null)
            {

                _context.NotificationPaths.Remove(notification);

                SaveAll();

                result = true;
            }
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Delete,
                Description = $"Delete Notification Path Id {Id} on  {DateTime.Now}",
                UserId = UserId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return result;
        }

        public bool ToggleNotificationPath(Guid Id)
        {
            var targetRecord = _context.NotificationPaths.Where(x => x.Id == Id).FirstOrDefault();
            if (targetRecord == null)
            {
                throw new("Id not found");
            }
            if (targetRecord.IsActive)
            {
                targetRecord.IsActive = false;
            }
            else
            {
                targetRecord.IsActive = true;
            }
            return SaveAll();
        }


        //admin notification
        public IQueryable<AdminNotificationVM> GetAdminNotification(FilterAdminNotification filter = null, PaginationFilter paginationFilter = null)
        {
            var notification = (from no in _context.SubscriberNotifications
                                join np in _context.NotificationPaths on (Guid?)no.NotificationPath equals np.Id into notificationPath
                                from np in notificationPath.DefaultIfEmpty()
                                select new AdminNotificationVM
                                {
                                    Id = no.ID,
                                    Description = no.Description,
                                    ReminderDate = no.ReminderDate.ToString("dd/MM/yyyy"),
                                    ReminderTime = no.ReminderDate.ToString(@"hh\:mm"),
                                    CreatedOn = no.DateCreated,
                                    IsActive = no.IsDeleted == false ? true : false,
                                    NotificationPath = np.PathDesscription == null ? string.Empty : np.PathDesscription
                                });

            if (string.IsNullOrEmpty(filter.SortBy)) notification = notification.OrderByDescending(x => x.CreatedOn);
            else notification = notification.OrderBy(filter.SortByAndOrder);

            if (paginationFilter == null)
            {
                return notification;
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return notification.Skip(skip).Take(paginationFilter.PageSize);
        }

        public IQueryable<AdminNotificationVM> GetAdminNotificationById(Guid Id)
        {
            return (from no in _context.SubscriberNotifications
                    join np in _context.NotificationPaths on (Guid?)no.NotificationPath equals np.Id into notificationPath
                    from np in notificationPath.DefaultIfEmpty()
                    where no.ID == Id
                    select new AdminNotificationVM
                    {
                        Id = no.ID,
                        Description = no.Description,
                        ReminderDate = no.ReminderDate.ToString("dd/MM/yyyy"),
                        ReminderTime = no.ReminderDate.ToString(@"hh\:mm"),
                        IsActive = no.IsDeleted == false ? true : false,
                        NotificationPath = np.PathDesscription == null ? string.Empty : np.PathDesscription
                    }).OrderByDescending(x => x.Id);
        }

        public bool CreateAdmiNotification(AdminNotificationDTO model, Guid CompanyId)
        {
            var ReminderDate = Convert.ToDateTime(model.ReminderDate).Date;
            var time = model.ReminderTime.TimeOfDay;
            ReminderDate = ReminderDate.Add(model.ReminderTime.TimeOfDay);
            var scheduleDateInUtc = ReminderDate; //.ToUniversalTime();
            var currentDateTimeUtc = DateTime.Now; //Constants.GetCurrentDateTime(TimeZoneInfo.Utc);
            if (scheduleDateInUtc < currentDateTimeUtc)
                throw new("Reminder date and time must be a future date/time");

            //CompanyId = Guid.Parse("3a002c17-3a44-d263-640a-72989d4aabc8");

            var user = _context.Users.SingleOrDefault(x => x.CompanyId == CompanyId && !x.IsDeleted);
            if (user == null)
            {
                throw new("User not found");
            }

            var rowId = Guid.NewGuid();
            var notification = new SubscriberNotification()
            {
                ID = rowId,
                CompanyId = CompanyId,
                Description = model.Description,
                IsRead = false,
                IsDeleted = false,
                NotificationPath = model.NotificationPathId,
                DateCreated = Constants.GetCurrentDateTime(),
                CreatedBy = CompanyId,
                ReminderDate = scheduleDateInUtc,
                TimeCreated = DateTime.Now, //default(DateTime).Add(Constants.GetCurrentDateTime(TimeZoneInfo.Utc).TimeOfDay),
                Comments = "Created on " + System.DateTime.Now + " " + Environment.NewLine + "==================" + Environment.NewLine,
            };
            _context.SubscriberNotifications.Add(notification);

            //schedule reminder
            TimeSpan span = scheduleDateInUtc - currentDateTimeUtc;
            double totalMinutes = span.TotalMinutes;
            _scheduler.Schedule(new NotificationParam
            {
                CompanyId = CompanyId,
                ReminderId = notification.ID
            }, TimeSpan.FromMinutes(totalMinutes)
                , $"Customer Reminder {user.Email}");

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Create new Admin notification with id {rowId} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();
        }

        public bool UpdateAdminNotification(Guid Id, AdminNotificationDTO model, Guid CompanyId)
        {
            var notification = _context.SubscriberNotifications.Where(x => x.ID == Id).FirstOrDefault();
            if (notification == null)
            {
                throw new($"notifiation with id {Id} not found");
            }

            var ReminderDate = Convert.ToDateTime(model.ReminderDate).Date;
            var time = model.ReminderTime.TimeOfDay;
            ReminderDate = ReminderDate.Add(model.ReminderTime.TimeOfDay);
            var scheduleDateInUtc = ReminderDate; //.ToUniversalTime();
            var currentDateTimeUtc = DateTime.Now; //Constants.GetCurrentDateTime(TimeZoneInfo.Utc);
            if (scheduleDateInUtc < currentDateTimeUtc)
                throw new("Reminder date and time must be a future date/time");

            var user = _context.Users.SingleOrDefault(x => x.CompanyId == CompanyId && !x.IsDeleted);
            if (user == null)
            {
                throw new("User not found");
            }

            notification.Description = model.Description;
            notification.NotificationPath = model.NotificationPathId;
            notification.ReminderDate = scheduleDateInUtc;
            notification.ModifiedOn = DateTime.Now;

            //schedule reminder
            TimeSpan span = scheduleDateInUtc - currentDateTimeUtc;
            double totalMinutes = span.TotalMinutes;
            _scheduler.Schedule(new NotificationParam
            {
                CompanyId = CompanyId,
                ReminderId = notification.ID
            }, TimeSpan.FromMinutes(totalMinutes)
                , $"Customer Reminder {user.Email}");

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update new Admin notification for id {Id} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }

        public bool RemindmeLater(Guid Id, NotificationReminder model, Guid CompanyId)
        {
            var notification = _context.SubscriberNotifications.Where(x => x.ID == Id).FirstOrDefault();
            if (notification == null)
            {
                throw new("Id not found");
            }

            var ReminderDate = Convert.ToDateTime(model.ReminderDate).Date;
            var time = model.ReminderTime.TimeOfDay;
            ReminderDate = ReminderDate.Add(model.ReminderTime.TimeOfDay);
            var scheduleDateInUtc = ReminderDate; //.ToUniversalTime();
            var currentDateTimeUtc = DateTime.Now; //Constants.GetCurrentDateTime(TimeZoneInfo.Utc);
            if (scheduleDateInUtc < currentDateTimeUtc)
                throw new("Reminder date and time must be a future date/time");

            notification.ReminderDate = scheduleDateInUtc;
            notification.ModifiedOn = DateTime.Now;

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.SetReminder,
                Description = $"Reset Admin notification time for id {Id} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }



        public bool DeleteAdmiNotification(Guid Id, Guid CompanyId)
        {
            var notification = _context.SubscriberNotifications.Where(x => x.ID == Id).FirstOrDefault();
            if (notification == null)
            {
                throw new("Id not found");
            }

            _context.SubscriberNotifications.Remove(notification);

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Delete,
                Description = $"Delete Admin notification id {Id} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);



            return SaveAll();
        }

        public bool ToggleNotification(Guid Id)
        {
            var targetRecord = _context.SubscriberNotifications.Where(x => x.ID == Id).FirstOrDefault();
            if (targetRecord == null)
            {
                throw new("Id not found");
            }
            if (targetRecord.IsDeleted)
            {
                targetRecord.IsDeleted = false;
            }
            else
            {
                targetRecord.IsDeleted = true;
            }
            return SaveAll();
        }


        //OfferPromotion
        public IQueryable<OfferPromotionViewModel> GetOfferPromotion(FilterPromotion filter = null, PaginationFilter paginationFilter = null)
        {
            var Query = _context.OfferPromotions.AsNoTracking().Select(x => new OfferPromotionViewModel
            {
                Id = x.Id,
                Status = x.EnablePromotion == false ? "InActive" : "Active",
                Percentage = x.Percentage.ToString("#,##0.00000000"),
                PromotionCode = x.PromotionCode,
                DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                CreatedOn = x.DateCreated,
            });


            if (filter.CreatedOn.HasValue) Query = Query.Where(x => x.CreatedOn >= filter.CreatedOn);
            if (!string.IsNullOrEmpty(filter.Search)) Query = Query.Where(x => x.PromotionCode.ToLower().Contains(filter.Search.ToLower())
                                                                                   || x.Status.ToLower().Contains(filter.Search.ToLower()));

            if (string.IsNullOrEmpty(filter.SortBy)) Query = Query.OrderByDescending(x => x.CreatedOn);
            else Query = Query.OrderBy(filter.SortByAndOrder);

            if (paginationFilter == null)
            {
                return Query;
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return Query.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<OfferPromotionViewModel> GetOfferPromotionById(Guid Id)
        {
            return _context.OfferPromotions.AsNoTracking()
                .Where(x => x.Id == Id)
                .Select(x => new OfferPromotionViewModel
                {
                    Id = x.Id,
                    Status = x.EnablePromotion == false ? "InActive" : "Active",
                    Percentage = x.Percentage.ToString("#,##0.00000000"),
                    PromotionCode = x.PromotionCode,
                    DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
                    CreatedOn = x.DateCreated,
                });

        }
        public bool CreateOfferPromotion(CreatePromotionViewModel model, Guid CompanyId)
        {
            var checkIfCodeExist = _context.OfferPromotions.Where(x => x.PromotionCode == model.PromotionCode).Select(x => x.PromotionCode);
            if (checkIfCodeExist.Any())
            {
                throw new("Promotion Code already exist");
            }
            var offerprom = new OfferPromotion()
            {
                Id = Guid.NewGuid(),
                EnablePromotion = model.Status,
                Percentage = model.Percentage,
                PromotionCode = model.PromotionCode,
                DateCreated = DateTime.Now
            };
            _context.OfferPromotions.Add(offerprom);

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Create Promotion Offer Code {model.PromotionCode} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();
        }
        public bool UpdateOfferPromotion(Guid Id, CreatePromotionViewModel model, Guid CompanyId)
        {
            var promotion = _context.OfferPromotions.Where(x => x.Id == Id).FirstOrDefault();
            if (promotion == null)
            {
                throw new("Id not found");
            }

            var offerPromotions = _context.OfferPromotions.Where(x => x.PromotionCode == model.PromotionCode && x.Id != Id).Select(x => x.PromotionCode);
            if (offerPromotions.Any())
            {
                throw new("Promotion Code already exist");
            }

            promotion.EnablePromotion = model.Status;
            promotion.Percentage = model.Percentage;
            promotion.PromotionCode = model.PromotionCode;
            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update Promotion Offer Code {promotion.PromotionCode} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();
        }
        public bool DeleteOfferPromotion(Guid Id, Guid CompanyId)
        {
            var result = false;
            var promotion = _context.OfferPromotions.Where(x => x.Id == Id).FirstOrDefault();
            if (promotion == null)
            {
                throw new("Id not found");
            }
            if (promotion != null)
            {

                _context.OfferPromotions.Remove(promotion);


                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Admin,
                    Action = (int)AuditLogCustomerAction.Delete,
                    Description = $"Delete Promotion Offer Code {promotion.PromotionCode} at  {DateTime.Now}",
                    UserId = CompanyId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);
                SaveAll();

                result = true;
            }

            return result;
        }

        public bool TogglePromotion(Guid Id)
        {
            var targetRecord = _context.OfferPromotions.Where(x => x.Id == Id).FirstOrDefault();
            if (targetRecord == null)
            {
                throw new("Id not found");
            }
            if (targetRecord.EnablePromotion)
            {
                targetRecord.EnablePromotion = false;
            }
            else
            {
                targetRecord.EnablePromotion = true;
            }
            return SaveAll();
        }

        //promotionalCode
        public IQueryable<PromoViewModel> GetPromoCode(FilterPromotionalCode filter = null, PaginationFilter paginationFilter = null)
        {
            var Query = (from p in _context.PromotionalCodes
                         join u in _context.Users on p.UserId equals u.Id
                         select new PromoViewModel
                         {
                             Id = p.Id,
                             PromotionCode = p.PromoCode,
                             DateCreated = p.DateCreated.ToString("dd/MM/yyyy"),
                             CreatedOn = p.DateCreated,
                             UserName = u.UserName
                         });


            if (filter.CreatedOn.HasValue) Query = Query.Where(x => x.CreatedOn >= filter.CreatedOn);
            if (!string.IsNullOrEmpty(filter.Search)) Query = Query.Where(x => x.PromotionCode.ToLower().Contains(filter.Search.ToLower())
                                                                                   || x.UserName.ToLower().Contains(filter.Search.ToLower()));

            if (string.IsNullOrEmpty(filter.SortBy)) Query = Query.OrderByDescending(x => x.Id);
            else Query = Query.OrderBy(filter.SortByAndOrder);

            if (paginationFilter == null)
            {
                return Query;
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return Query.Skip(skip).Take(paginationFilter.PageSize);
        }
        public IQueryable<PromoViewModel> GetPromoCodeById(Guid Id)
        {
            return (from p in _context.PromotionalCodes
                    join u in _context.Users on p.UserId equals u.Id
                    where p.Id == Id
                    select new PromoViewModel
                    {
                        Id = p.Id,
                        PromotionCode = p.PromoCode,
                        DateCreated = p.DateCreated.ToString("dd/MM/yyyy"),
                        CreatedOn = p.DateCreated,
                        UserName = u.UserName
                    });

        }
        public bool CreatePromoCode(CreatePromoViewModel model, Guid CompanyId)
        {
            var validateCode = _context.OfferPromotions.Where(x => x.PromotionCode == model.PromoCode).FirstOrDefault();
            if (validateCode == null)
            {
                throw new("Please use a valide promo code");
            }
            if (!validateCode.EnablePromotion)
            {
                throw new("Promo Code is disable");
            }

            var rowId = Guid.NewGuid();
            var promotionalCode = new PromotionalCode()
            {
                Id = rowId,
                PromoCode = model.PromoCode,
                UserId = CompanyId,
                DateCreated = DateTime.Now
            };
            _context.PromotionalCodes.Add(promotionalCode);

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Subscription,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Use promotional Code {model.PromoCode} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);
            return SaveAll();
        }

        public bool UpdatePromoCode(Guid Id, CreatePromoViewModel model, Guid CompanyId)
        {
            var promo = _context.PromotionalCodes.Where(x => x.Id == Id).FirstOrDefault();
            if (promo == null)
            {
                throw new("Id not found");
            }

            var validateCode = _context.OfferPromotions.Where(x => x.PromotionCode == model.PromoCode).FirstOrDefault();
            if (validateCode == null)
            {
                throw new("Please use a valide promo code");
            }
            if (!validateCode.EnablePromotion)
            {
                throw new("Promo Code is disable");
            }
            promo.PromoCode = model.PromoCode;

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Subscription,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update promotional Code {model.PromoCode} at  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();
        }
        public bool DeletePromoCode(Guid Id, Guid CompanyId)
        {
            var result = false;
            var promo = _context.PromotionalCodes.Where(x => x.Id == Id).FirstOrDefault();
            if (promo == null)
            {
                throw new("Id not found");
            }
            if (promo != null)
            {

                _context.PromotionalCodes.Remove(promo);

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Subscription,
                    Action = (int)AuditLogCustomerAction.Delete,
                    Description = $"Delete promotional Code {promo.PromoCode} at  {DateTime.Now}",
                    UserId = CompanyId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);

                SaveAll();

                result = true;
            }

            return result;
        }

        //ReferralCode
        public IQueryable<ReferralCodeViewModel> GetReferralCode()
        {
            return _context.ReferralCodes.AsNoTracking().Select(x => new ReferralCodeViewModel
            {
                Id = x.Id,
                Status = x.Status == false ? "InActive" : "Active",
                Percentage = x.Percentage.ToString("#,##0.00000000"),
                DateCreated = x.DateCreated.ToString("dd/MM/yyyy"),
            });
        }
        public bool CreateReferralCode(CreateReferralCodeViewModel model, Guid CompanyId, Guid UserId)
        {
            var referralcode = new ReferralCode()
            {
                Id = Guid.NewGuid(),
                Status = model.Status,
                Percentage = model.Percentage,
                DateCreated = DateTime.Now
            };
            _context.ReferralCodes.Add(referralcode);

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Create,
                Description = $"Create new Referralcode with  {model.Percentage} %  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();
        }

        public bool AnyRecordInReferralCode()
        {
            return _context.ReferralCodes.Any();
        }
        public bool UpdateReferralCode(Guid Id, CreateReferralCodeViewModel model, Guid CompanyId, Guid UserId)
        {
            var referralcode = _context.ReferralCodes.Where(x => x.Id == Id).FirstOrDefault();
            if (referralcode == null)
            {
                throw new("Referral not found");
            }
            referralcode.Status = model.Status;
            referralcode.Percentage = model.Percentage;

            var AuditLog = new AuditLog()
            {
                EntityType = (int)AuditLogEntityType.Admin,
                Action = (int)AuditLogCustomerAction.Update,
                Description = $"Update Referralcode id  {Id}  {DateTime.Now}",
                UserId = CompanyId,
                CompanyId = CompanyId,
                CreatedOn = DateTime.Now
            };
            _context.AuditLogs.Add(AuditLog);

            return SaveAll();
        }
        public bool DeleteReferralCode(Guid Id, Guid CompanyId, Guid UserId)
        {
            var result = false;
            var referralcode = _context.ReferralCodes.Where(x => x.Id == Id).FirstOrDefault();
            if (referralcode == null)
            {
                throw new("Referral not found");
            }
            if (referralcode != null)
            {

                _context.ReferralCodes.Remove(referralcode);


                SaveAll();

                var AuditLog = new AuditLog()
                {
                    EntityType = (int)AuditLogEntityType.Admin,
                    Action = (int)AuditLogCustomerAction.Delete,
                    Description = $"Delete Referralcode id  {Id}  {DateTime.Now}",
                    UserId = CompanyId,
                    CompanyId = CompanyId,
                    CreatedOn = DateTime.Now
                };
                _context.AuditLogs.Add(AuditLog);

                result = true;
            }

            return result;
        }
        public async Task<StatusModel> Adminlogin(AdminLogin request)
        {
            var targetStatus = new StatusModel();
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) throw new($"Invalid credentials");

            if (await userManager.CheckPasswordAsync(user, request.Password))
            {
                if (user.IsDeleted) throw new("Your user account has been disabled, Contact your administrator");
                if (!user.EmailConfirmed) throw new($"Your user account has not been verified. Click on the verify link sent to your mail on {user.CreatedOn}");


                var token = "";
                //  var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                if (user.TwoFactorEnabled)
                {
                    var code = await userManager.GenerateTwoFactorTokenAsync(user, Constants.OtpProvider);

                    var emailModel = new TwoFactorAuthentication
                    {
                        Name = user.FullName,
                        Date = Constants.GetCurrentDateTime().ToLongDateString(),
                        OTP = code
                    };

                    var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - Login OTP", EmailTemplateEnum.LoginOTP, emailModel);
                    if (!emailSent)
                    {
                        throw new("Unable to send login OTP at this time. Please try again..");
                    }
                }
                else
                {
                    var jwtSecurityToken = await JwtHelper.GenerateJWToken(jwtSettings, user); //JwtHelper.GenerateJWToken(userManager, roleManager, jwtSettings, user, "");
                    token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                    targetStatus.Status = "success";
                    targetStatus.Message = token;
                    return targetStatus;
                }
            }
            targetStatus.Status = "failed";
            targetStatus.Message = $"Incorrect credentials";
            return targetStatus;
        }
        public async Task<bool> ForgotPassword(ForgotPasswordVM request)
        {
            var SaveResult = false;
            try
            {
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    throw new("No account with this email address");
                }

                string code = await userManager.GeneratePasswordResetTokenAsync(user);

                _context.PasswordResetTokens.Add(new PasswordResetToken
                {
                    Email = user.Email,
                    Token = code,
                    Id = SequentialGuid.Create()
                });

                var webUrl = configuration["SpineWeb"];
                var emailModel = new ResetPassword
                {
                    ActionLink = Constants.GetResetPasswordLink(webUrl, code),
                    Name = user.FullName,
                };

                if (await _context.SaveChangesAsync() > 0)
                {
                    var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - Reset Password", EmailTemplateEnum.ResetPassword, emailModel);

                    SaveResult = true;

                    return SaveResult;
                }

                return SaveResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> ResetPassword(ResetPasswordVM request)
        {
            var SaveResult = false;
            try
            {
                var resetData = await _context.PasswordResetTokens.FirstOrDefaultAsync(x => x.Token == request.ResetCode);
                if (resetData == null)
                {
                    throw new($"Reset Code is invalid or has expired. Reset your password from the login page");
                }

                var user = await userManager.FindByEmailAsync(resetData.Email);
                if (user == null)
                {
                    throw new($"An error occured");
                }

                var supperAdminEmail = configuration["Supper-Admin:Email"];
                if (string.IsNullOrWhiteSpace(user.Email) == string.IsNullOrWhiteSpace(supperAdminEmail))
                {
                    throw new($"You can not change upper admin password");
                }

                var validators = userManager.PasswordValidators;
                foreach (var validator in validators)
                {
                    var check = await validator.ValidateAsync(userManager, user, request.Password);
                    if (!check.Succeeded) throw new(check.Errors.FirstOrDefault()?.Description);
                }

                var result = await userManager.ResetPasswordAsync(user, resetData.Token, request.Password);
                if (result.Succeeded)
                {
                    _context.PasswordResetTokens.Remove(resetData);
                    await _context.SaveChangesAsync();
                    SaveResult = true;
                    return SaveResult;
                }

                return SaveResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> ChangePassword(ChangePassword request)
        {
            var SaveResult = false;
            try
            {
                var user = await userManager.FindByIdAsync(request.UserId.ToString());
                if (user == null)
                {
                    throw new($"User not found");
                }

                var supperAdminEmail = configuration["Supper-Admin:Email"];
                if (string.IsNullOrWhiteSpace(user.Email) == string.IsNullOrWhiteSpace(supperAdminEmail))
                {
                    throw new($"You can not change upper admin password");
                }

                var validators = userManager.PasswordValidators;
                foreach (var validator in validators)
                {
                    var check = await validator.ValidateAsync(userManager, user, request.NewPassword);
                    if (!check.Succeeded) throw new(check.Errors.FirstOrDefault()?.Description);
                }

                if (await userManager.CheckPasswordAsync(user, request.OldPassword))
                {
                    await userManager.RemovePasswordAsync(user);
                    var result = await userManager.AddPasswordAsync(user, request.NewPassword);
                    return SaveResult = result.Succeeded;
                }

                throw new("Current password is invalid");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
