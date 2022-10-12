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
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;

namespace Spine.Core.Customers.Commands
{
    public static class AddBulkCustomer
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            public List<CustomerModel> Customers { get; set; }

        }

        public class CustomerModel
        {
            [Required]
            [Description("Customer Name")]
            public string CustomerName { get; set; }

            [Required]
            [Description("Email Address")]
            public string EmailAddress { get; set; }

            [Required]
            [Description("Phone Number Country Code")]
            public string PhoneNumberCountryCode { get; set; }
            
            [Required]
            [Description("Phone Number")]
            public string PhoneNumber { get; set; }

            [Description("Gender")]
            public string Gender { get; set; }

            [Description("Tax Identification Number")]
            public string TaxIdentificationNumber { get; set; }

            [Required]
            [Description("Business Name")]
            public string BusinessName { get; set; }
            //[Description("Business Type")]
            //public string BusinessType { get; set; }
            [Description("Operating Sector")]
            public string OperatingSector { get; set; }

            //    [Required]
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

        }

        public class AddressModel
        {
            public CustomerAddress ShippingAddress { get; set; }
            public CustomerAddress BillingAddress { get; set; }
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
                var distinctEmails = request.Customers.Select(x => x.EmailAddress).ToHashSet();
                if (distinctEmails.Count != request.Customers.Count) return new Response("Email address cannot contain duplicates");

                var customerEmails = await _dbContext.Customers.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.Email).ToListAsync();

                int skipped = 0;
                foreach (var item in request.Customers)
                {
                    if (customerEmails.Contains(item.EmailAddress))
                    {
                        skipped++;
                        continue;
                    }

                    item.PhoneNumber = item.PhoneNumberCountryCode + item.PhoneNumber;
                    var customer = _mapper.Map<Customer>(item);
                    customer.CreatedBy = request.UserId;
                    customer.CompanyId = request.CompanyId;

                    var address = _mapper.Map<AddressModel>(item);
                    if (address.BillingAddress != null)
                    {
                        address.BillingAddress.CompanyId = request.CompanyId;
                        address.BillingAddress.CreatedBy = request.UserId;
                        address.BillingAddress.CustomerId = customer.Id;
                        address.BillingAddress.IsBilling = true;
                        address.BillingAddress.IsPrimary = true;

                        _dbContext.CustomerAddresses.Add(address.BillingAddress);
                    }

                    if (address.ShippingAddress != null)
                    {
                        address.ShippingAddress.CompanyId = request.CompanyId;
                        address.ShippingAddress.CreatedBy = request.UserId;
                        address.ShippingAddress.CustomerId = customer.Id;
                        address.ShippingAddress.IsBilling = false;
                        address.ShippingAddress.IsPrimary = true;

                        _dbContext.CustomerAddresses.Add(address.ShippingAddress);
                    }

                    _dbContext.Customers.Add(customer);

                    _auditHelper.SaveAction(_dbContext, request.CompanyId,
                      new AuditModel
                      {
                          EntityType = (int)AuditLogEntityType.Customer,
                          Action = (int)AuditLogCustomerAction.Create,
                          Description = $"Added new customer  {customer.Name} with {customer.Id}",
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
