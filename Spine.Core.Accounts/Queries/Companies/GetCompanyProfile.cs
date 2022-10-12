using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Companies
{
    public static class GetCompanyProfile
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            
            [JsonIgnore]
            public Guid UserId { get; set; }
        }

        public class Response
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            
            public string PersonalEmail { get; set; }
            public string PersonalPhone { get; set; }
            public string Gender { get; set; }
            
            [JsonIgnore]
            public Guid Id { get; set; }
            public string BusinessName { get; set; }
            public string BusinessEmail { get; set; }
            public string BusinessPhone { get; set; }
            public string BusinessType { get; set; }
            public string OperatingSector { get; set; }

            public string TIN { get; set; }
            public string SubscriptionPlan { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public DateTime? OnboardingDate { get; set; }
            public string CacRegNo { get; set; }
            public int? EmployeeCount { get; set; }
            public string Website { get; set; }
            public DateTime? DateEstablished { get; set; }
            public string Description { get; set; }
            public string LogoId { get; set; }
            public string BusinessAddress { get; set; }
            public string Motto { get; set; }
            public string FacebookProfile { get; set; }
            public string InstagramProfile { get; set; }
            public string TwitterProfile { get; set; }
            public string ReferralCode { get; set; }
            public AddressModel Shipping { get; set; }
            public AddressModel Billing { get; set; }
            
            public List<DocumentModel> Documents { get; set; }

        }

        public class DocumentModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class AddressModel
        {
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Country { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public int AddressId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IMapper _mapper;

            public Handler(SpineContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var data = await (from comp in _dbContext.Companies.Where(
                        x => x.Id == request.CompanyId && !x.IsDeleted)
                    join user in _dbContext.Users.Where(x => x.Id == request.UserId) on comp.Id equals user.CompanyId
                    join sub in _dbContext.CompanySubscriptions.Where(x => x.IsActive) on comp.Id equals sub.ID_Company
                        into subscription
                    from sub in subscription.DefaultIfEmpty()
                    join plan in _dbContext.Plans on sub.ID_Plan equals plan.PlanId into plans
                    from plan in plans.DefaultIfEmpty()
                    select new Response
                    {
                        Id = comp.Id,
                        SubscriptionPlan = plan.PlanName,
                        BusinessName = comp.Name,
                        BusinessEmail = comp.Email,
                        BusinessType = comp.BusinessType,
                        OperatingSector = comp.OperatingSector,
                        OnboardingDate = comp.CreatedOn,
                        TIN = comp.TIN,
                        CacRegNo = comp.CacRegNo,
                        BusinessPhone = comp.PhoneNumber,
                        EmployeeCount = comp.EmployeeCount,
                        DateEstablished = comp.DateEstablished,
                        BusinessAddress = comp.Address,
                        Website = comp.Website,
                        Motto = comp.Motto,
                        Description = comp.Description,
                        LogoId = comp.LogoId,
                        FacebookProfile = comp.FacebookProfile,
                        InstagramProfile = comp.InstagramProfile,
                        TwitterProfile = comp.TwitterProfile,
                        ReferralCode = comp.ReferralCode,

                        PersonalEmail = user.Email,
                        PersonalPhone = user.PhoneNumber,
                        FullName = user.FullName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DateOfBirth = user.DateOfBirth,
                        Gender = user.Gender
                    }).SingleOrDefaultAsync();

                if (data == null) return null;

                data.Documents = await _dbContext.CompanyDocuments.Where(x => x.CompanyId == data.Id)
                    .Select(x => new DocumentModel
                    {
                        Id = x.Id, Name = x.Name
                    }).ToListAsync();

                data.Shipping = await _dbContext.SubscriberShippings.Where(c => c.ID_Company == data.Id)
                    .Select(x => new AddressModel
                    {
                        Address1 = x.Address1,
                        Address2 = x.Address2,
                        PostalCode = x.PostalCode,
                        Country = x.ID_Country,
                        AddressId = x.ID_Shipping,
                        State = x.ID_State,
                    }).FirstOrDefaultAsync();

                data.Billing = await _dbContext.SubscriberBillings.Where(c => c.ID_Company == data.Id)
                    .Select(x => new AddressModel
                    {
                        Address1 = x.Address1,
                        Address2 = x.Address2,
                        PostalCode = x.PostalCode,
                        Country = x.ID_Country,
                        AddressId = x.ID_Billing,
                        State = x.ID_State,
                    }).FirstOrDefaultAsync();

                return data;
            }
        }
    }
}
