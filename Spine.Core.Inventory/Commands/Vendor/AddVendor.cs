using System;
using System.Collections.Generic;
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
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Data.Entities.Vendor;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Vendor
{
    public static class AddVendor
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public string Name { get; set; }
            [Required] public string Email { get; set; }
            
            public string PhoneNumber { get; set; }
            [RequiredIf(nameof(VendorType), TypeOfVendor.Business, ErrorMessage = "Business name is required")]
            public string BusinessName { get; set; }
            
            [Required] public TypeOfVendor? VendorType { get; set; }

            public string DisplayName { get; set; }
            public string OperatingSector { get; set; }
            public string RcNumber { get; set; }
            public string Website { get; set; }
            public string TIN { get; set; }

            public List<AddressModel> BillingAddress { get; set; }
            public List<AddressModel> ShippingAddress { get; set; }
            
            [RequiredNonDefault] public BankAccountModel BankAccount { get; set; }
            [RequiredNonDefault] public ContactPersonModel ContactPerson { get; set; }
            
        }

        public class BankAccountModel
        {
            public string BankName { get; set; }
            public string BankCode { get; set; }
            public string AccountName { get; set; }
            public string AccountNumber { get; set; }
        }
        
        public class ContactPersonModel
        {
            public string Role { get; set; }
            public string FullName { get; set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
        }
        
        public class AddressModel
        {
            public bool IsPrimary { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
            public string State { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Guid VendorId { get; set; }
            public Response(Guid id)
            {
                VendorId = id;
                Status = HttpStatusCode.Created;
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
                if (request.ContactPerson == null) return new Response("Contact person is required");
                if (request.BankAccount == null) return new Response("Bank account is required");
                
                if (request.BillingAddress.Count(x => x.IsPrimary) > 1) return new Response("You cannot have more than one primary billing address");
                if (request.ShippingAddress.Count(x => x.IsPrimary) > 1) return new Response("You cannot have more than one primary shipping address");

                if (await _dbContext.Vendors.AnyAsync(x => x.CompanyId == request.CompanyId && x.Email == request.Email.Trim().ToLower() && !x.IsDeleted))
                {
                    return new Response("Email is already in use by another vendor");
                }

                var vendor = _mapper.Map<Data.Entities.Vendor.Vendor>(request);
                _dbContext.Vendors.Add(vendor);
                
                _dbContext.VendorContactPersons.Add(new VendorContactPerson
                {
                    CompanyId = vendor.CompanyId,
                    VendorId = vendor.Id,
                    FullName = request.ContactPerson.FullName,
                    Role = request.ContactPerson.Role,
                    EmailAddress = request.ContactPerson.EmailAddress,
                    PhoneNumber = request.ContactPerson.PhoneNumber,
                    CreatedBy = request.UserId
                    
                });
                
                _dbContext.VendorBankAccounts.Add(new VendorBankAccount
                {
                    CompanyId = vendor.CompanyId,
                    VendorId = vendor.Id,
                    AccountName = request.BankAccount.AccountName,
                    AccountNumber = request.BankAccount.AccountNumber,
                    BankCode = request.BankAccount.BankCode,
                    BankName = request.BankAccount.BankName,
                    CreatedBy = request.UserId
                });
                
                foreach (var item in request.BillingAddress)
                {
                    _dbContext.VendorAddresses.Add(new VendorAddress()
                    {
                        CompanyId = request.CompanyId, 
                        AddressLine1 = item.AddressLine1,
                        AddressLine2 = item.AddressLine2,
                        State = item.State,
                        //  City = item.City,
                        Country = item.Country,
                        PostalCode = item.PostalCode,
                        CreatedBy = request.UserId,
                        VendorId = vendor.Id,
                        IsBilling = true,
                        IsPrimary = item.IsPrimary,
                    });
                }

                foreach (var item in request.ShippingAddress)
                {
                    _dbContext.VendorAddresses.Add(new VendorAddress
                    {
                        CompanyId = request.CompanyId,
                        AddressLine1 = item.AddressLine1,
                        AddressLine2 = item.AddressLine2,
                        State = item.State,
                        //  City = item.City,
                        Country = item.Country,
                        PostalCode = item.PostalCode,
                        CreatedBy = request.UserId,
                        VendorId = vendor.Id,
                        IsBilling = false,
                        IsPrimary = item.IsPrimary
                    });
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                  new AuditModel
                  {
                      EntityType = (int)AuditLogEntityType.Vendor,
                      Action = (int)AuditLogVendorAction.Create,
                      Description = $"Added new vendor  {vendor.Name} with {vendor.Id}",
                      UserId = request.UserId
                  });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(vendor.Id)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
