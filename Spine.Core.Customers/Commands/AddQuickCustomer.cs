using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;

namespace Spine.Core.Customers.Commands
{
    public static class AddQuickCustomer
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public string Email { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string PhoneNumber { get; set; }
            //   [Required]
            public string BusinessName { get; set; }

            [Required]
            public AddressModel BillingAddress { get; set; }
        }

        public class AddressModel
        {
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Country { get; set; }
            //  public string City { get; set; }
            public string PostalCode { get; set; }
            public string State { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Guid CustomerId { get; set; }
            public Response(Guid id)
            {
                CustomerId = id;
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
                if (await _dbContext.Customers.AnyAsync(x => x.CompanyId == request.CompanyId && x.Email == request.Email.Trim().ToLower() && !x.IsDeleted))
                {
                    return new Response("Email is already in use by another customer");
                }

                var customer = new Customer
                {
                    Id = SequentialGuid.Create(),
                    Name = request.Name,
                    BusinessName = request.BusinessName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId
                };

                var item = request.BillingAddress;
                _dbContext.Customers.Add(customer);
                _dbContext.CustomerAddresses.Add(new CustomerAddress
                {
                    CompanyId = request.CompanyId,
                    AddressLine1 = item.AddressLine1,
                    AddressLine2 = item.AddressLine2,
                    State = item.State,
                    //  City = item.City,
                    Country = item.Country,
                    PostalCode = item.PostalCode,
                    CreatedBy = request.UserId,
                    CustomerId = customer.Id,
                    IsBilling = true,
                    IsPrimary = true
                });

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                  new AuditModel
                  {
                      EntityType = (int)AuditLogEntityType.Customer,
                      Action = (int)AuditLogCustomerAction.Create,
                      Description = $"Added new customer  {customer.Name} with {customer.Id}",
                      UserId = request.UserId
                  });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(customer.Id)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
