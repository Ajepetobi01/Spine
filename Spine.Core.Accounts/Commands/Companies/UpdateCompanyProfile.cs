using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.Companies
{
    public static class UpdateCompanyProfile
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required(ErrorMessage = "Business name is required")]
            public string BusinessName { get; set; }

            [Required(ErrorMessage = "Business email is required")]
            public string BusinessEmail { get; set; }
            [Required(ErrorMessage = "Business phone number is required")]
            public string BusinessPhone { get; set; }
            public string BusinessType { get; set; }
            public string OperatingSector { get; set; }

            public string TIN { get; set; }
            public string CacRegNo { get; set; }
            public int? EmployeeCount { get; set; }
            public string Website { get; set; }
            public DateTime? DateEstablished { get; set; }
            public string Description { get; set; }
            public string LogoId { get; set; }
            public string BusinessAddress { get; set; }
            public string Motto { get; set; }

            public AddressModel Shipping { get; set; }
            public AddressModel Billing { get; set; }
            
            
            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }
            
            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            public string PersonalPhone { get; set; }
            public string Gender { get; set; }
           // public DateTime? DateOfBirth { get; set; }
           
           public string FacebookProfile { get; set; }
           public string InstagramProfile { get; set; }
           public string TwitterProfile { get; set; }

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

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.NoContent;
            }

            public Response(HttpStatusCode statusCode)
            {
                Status = statusCode;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IAuditLogHelper _auditHelper;
            private readonly IMapper _mapper;

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var company = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted).SingleOrDefaultAsync();

                if (company == null)
                {
                    return new Response("Company not found");
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.IsBusinessOwner && !x.IsDeleted);
                if (user != null)
                {
                    user.FirstName = request.FirstName;
                    user.LastName = request.LastName;
                    user.Gender = request.Gender;
                    user.PhoneNumber = request.PersonalPhone;
                   // user.DateOfBirth = request.DateOfBirth;
                    user.FullName = $"{request.FirstName} {request.LastName}";
                }
                
                if (request.Billing != null)
                {
                    var address = await _dbContext.SubscriberBillings.SingleOrDefaultAsync(x => x.ID_Billing == request.Billing.AddressId);
                    if (address != null)
                    {
                        address.Address1 = request.Billing.Address1;
                        address.Address2 = request.Billing.Address2;
                        address.ID_Country = request.Billing.Country;
                        address.ID_State = request.Billing.State;
                        address.PostalCode = request.Billing.PostalCode;
                    }
                }

                if (request.Shipping != null)
                {
                    var address = await _dbContext.SubscriberShippings.SingleOrDefaultAsync(x => x.ID_Shipping == request.Shipping.AddressId);
                    if (address != null)
                    {
                        address.Address1 = request.Shipping.Address1;
                        address.Address2 = request.Shipping.Address2;
                        address.ID_Country = request.Shipping.Country;
                        address.ID_State = request.Shipping.State;
                        address.PostalCode = request.Shipping.PostalCode;
                    }
                }

                company.Name = request.BusinessName;
                company.BusinessType = request.BusinessType;
                company.OperatingSector = request.OperatingSector;
                company.Email = request.BusinessEmail;
                company.PhoneNumber = request.BusinessPhone;
                company.TIN = request.TIN;
                company.EmployeeCount = request.EmployeeCount;
                company.Website = request.Website;
                company.CacRegNo = request.CacRegNo;
                company.DateEstablished = request.DateEstablished;
                company.Description = request.Description;
                company.LogoId = request.LogoId;
                company.Address = request.BusinessAddress;
                company.Motto = request.Motto;
                company.FacebookProfile = request.FacebookProfile;
                company.InstagramProfile = request.InstagramProfile;
                company.TwitterProfile = request.TwitterProfile;
                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Company,
                        Action = (int)AuditLogCompanyAction.UpdateInfo,
                        Description = $"Updated business profile",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.NoContent)
                    : new Response(HttpStatusCode.BadRequest);
            }
        }
    }
}