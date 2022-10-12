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

namespace Spine.Core.Customers.Commands
{
    public static class UpdateCustomer
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

            [Required]
            public string Email { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string PhoneNumber { get; set; }
            //    [Required]
            public string BusinessName { get; set; }
            public string BusinessType { get; set; }
            public string OperatingSector { get; set; }
            public string Gender { get; set; }
            public string TIN { get; set; }

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

                var customers = await _dbContext.Customers.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted &&
                                              (x.Id == request.Id || x.Email.ToLower() == request.Email.ToLower())).ToListAsync();

                if (customers.Count == 0) return new Response("Customer not found");
                if (customers.Any(x => x.Id != request.Id))
                {
                    return new Response("Email is already in use by another customer");
                }

                var customer = customers.First();

                _mapper.Map(request, customer);

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
               new AuditModel
               {
                   EntityType = (int)AuditLogEntityType.Customer,
                   Action = (int)AuditLogCustomerAction.Update,
                   Description = $"Updated information of customer with id {request.Id}",
                   UserId = request.UserId
               });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
