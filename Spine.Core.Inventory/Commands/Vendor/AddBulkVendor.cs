using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Entities.Vendor;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Vendor
{
    public static class AddBulkVendor
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            public List<VendorModel> Vendors { get; set; }

        }

        public class VendorModel
        {
            [Required]
            [Description("Full Name")]
            public string FullName { get; set; }
            
            [Required]
            [Description("Display Name")]
            public string DisplayName { get; set; }

            [Required]
            [Description("Email Address")]
            public string EmailAddress { get; set; }

            [Required]
            [Description("Phone Number Country Code")]
            public string PhoneNumberCountryCode { get; set; }
            
            [Required]
            [Description("Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Description("Operating Sector")]
            public string OperatingSector { get; set; }
            
            // [Required]
            [Description("Business Name")]
            public string BusinessName { get; set; }
            
            [Description("Tax Identification Number")]
            public string TaxIdentificationNumber { get; set; }
            
            [Required]
            [Description("RC Number")]
            public string RCNumber { get; set; }
            
            [Description("Website")]
            public string Website { get; set; }

            [Description("Billing Address Line 1")]
            public string BillingAddressLine1 { get; set; }
            [Description("Billing Address Line 2")]
            public string BillingAddressLine2 { get; set; }
            [Description("Billing State")]
            public string BillingState { get; set; }
            [Description("Billing Country")]
            public string BillingCountry { get; set; }
            [Description("Billing PostalCode")]
            public string BillingPostalCode { get; set; }


            [Description("Shipping Address Line 1")]
            public string ShippingAddressLine1 { get; set; }
            [Description("Shipping Address Line 2")]
            public string ShippingAddressLine2 { get; set; }
            [Description("Shipping State")]
            public string ShippingState { get; set; }
            [Description("Shipping Country")]
            public string ShippingCountry { get; set; }
            [Description("Shipping PostalCode")]
            public string ShippingPostalCode { get; set; }
            
                
            [Description("Contact Person Name")]
            public string ContactPersonName { get; set; }
            [Description("Contact Person Role")]
            public string ContactPersonRole { get; set; }
            [Description("Contact Person Email")]
            public string ContactPersonEmail { get; set; }
            
            [Required]
            [Description("Contact Person Phone Country Code")]
            public string ContactPersonPhoneCountryCode { get; set; }
            
            [Description("Contact Person Phone")]
            public string ContactPersonPhone { get; set; }
            
            [Description("Bank Name")]
            public string BankName { get; set; }
            [Description("Account Name")]
            public string AccountName { get; set; }
            [Description("Account Number")]
            public string AccountNumber { get; set; }

        }

        public class AddressModel
        {
            public VendorAddress ShippingAddress { get; set; }
            public VendorAddress BillingAddress { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
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
                var distinctEmails = request.Vendors.Select(x => x.EmailAddress).ToHashSet();
                if (distinctEmails.Count != request.Vendors.Count) return new Response("Email address cannot contain duplicates");

                var vendorEmails = await _dbContext.Vendors.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.Email).ToListAsync();

                int skipped = 0;
                var banks = await _dbContext.Banks
                    .Select(x => new {x.BankName, x.BankCode})
                    .ToDictionaryAsync(x => x.BankName, y => y.BankCode);
                
                foreach (var item in request.Vendors)
                {
                    if (vendorEmails.Contains(item.EmailAddress))
                    {
                        skipped++;
                        continue;
                    }

                    item.PhoneNumber = item.PhoneNumberCountryCode + item.PhoneNumber;
                    var vendor = _mapper.Map<Data.Entities.Vendor.Vendor>(item);
                    vendor.CreatedBy = request.UserId;
                    vendor.CompanyId = request.CompanyId;

                    vendor.VendorType = item.BusinessName.IsNullOrEmpty()
                        ? TypeOfVendor.Individual
                        : TypeOfVendor.Business;

                    var address = _mapper.Map<AddressModel>(item);
                    if (address.BillingAddress != null)
                    {
                        address.BillingAddress.CompanyId = request.CompanyId;
                        address.BillingAddress.CreatedBy = request.UserId;
                        address.BillingAddress.VendorId = vendor.Id;
                        address.BillingAddress.IsBilling = true;
                        address.BillingAddress.IsPrimary = true;

                        _dbContext.VendorAddresses.Add(address.BillingAddress);
                    }

                    if (address.ShippingAddress != null)
                    {
                        address.ShippingAddress.CompanyId = request.CompanyId;
                        address.ShippingAddress.CreatedBy = request.UserId;
                        address.ShippingAddress.VendorId = vendor.Id;
                        address.ShippingAddress.IsBilling = false;
                        address.ShippingAddress.IsPrimary = true;

                        _dbContext.VendorAddresses.Add(address.ShippingAddress);
                    }

                    _dbContext.Vendors.Add(vendor);

                    var contactPerson = new VendorContactPerson
                    {
                        CompanyId = vendor.CompanyId,
                        VendorId = vendor.Id,
                        FullName = "",
                        Role = "",
                        EmailAddress = "",
                        PhoneNumber = ""
                    };

                    if (!item.ContactPersonName.IsNullOrEmpty())
                    {
                        contactPerson.FullName = item.ContactPersonName;
                        contactPerson.Role = item.ContactPersonRole;
                        contactPerson.EmailAddress = item.ContactPersonEmail;
                        contactPerson.CreatedBy = request.UserId;
                        contactPerson.PhoneNumber = item.ContactPersonPhoneCountryCode + item.ContactPersonPhone;
                    }

                    _dbContext.VendorContactPersons.Add(contactPerson);

                    var bankAccount = new VendorBankAccount
                    {
                        VendorId = vendor.Id,
                        CompanyId = vendor.CompanyId,
                        AccountName = "",
                        AccountNumber = "",
                        BankCode = "",
                        BankName = ""
                    };
                    
                    if (!item.BankName.IsNullOrEmpty() && !item.AccountNumber.IsNullOrEmpty())
                    {
                        bankAccount.AccountName = item.AccountName;
                        bankAccount.BankName = item.BankName;
                        bankAccount.AccountNumber = item.AccountNumber;
                        bankAccount.BankCode = "";
                        contactPerson.CreatedBy = request.UserId;
                        
                        if(banks.TryGetValue(item.BankName, out var bankCode))
                        {
                            bankAccount.BankCode = bankCode;
                        }
                    }
                    _dbContext.VendorBankAccounts.Add(bankAccount);

                    _auditHelper.SaveAction(_dbContext, request.CompanyId,
                      new AuditModel
                      {
                          EntityType = (int)AuditLogEntityType.Vendor,
                          Action = (int)AuditLogVendorAction.Create,
                          Description = $"Added new vendor {vendor.Name} with {vendor.Id}",
                          UserId = request.UserId
                      });
                }

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response("No records saved");
            }
        }

    }
}
