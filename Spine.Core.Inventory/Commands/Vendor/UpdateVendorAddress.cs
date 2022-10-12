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

namespace Spine.Core.Inventories.Commands.Vendor
{
    public static class UpdateVendorAddress
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

            public bool IsPrimary { get; set; }

            [Required]
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }

            [Required]
            public string Country { get; set; }
            public string PostalCode { get; set; }

            [Required]
            public string State { get; set; }

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
                var existingItem = await _dbContext.VendorAddresses.Where(x =>
                        x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted)
                    .SingleOrDefaultAsync();

                if (existingItem == null) return new Response("Address not found");

                if (request.IsPrimary)
                {
                    var others = await _dbContext.VendorAddresses.Where(x => x.CompanyId == request.CompanyId
                                    && x.Id == request.Id && x.IsBilling == existingItem.IsBilling && x.IsPrimary).ToListAsync();
                    others.ForEach(x => x.IsPrimary = false);
                }

                existingItem.IsPrimary = request.IsPrimary;
                existingItem.AddressLine1 = request.AddressLine1;
                existingItem.AddressLine2 = request.AddressLine2;
               // existingItem.City = request.City;
                existingItem.State = request.State;
                existingItem.Country = request.Country;
                existingItem.PostalCode = request.PostalCode;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
               new AuditModel
               {
                   EntityType = (int)AuditLogEntityType.Vendor,
                   Action = (int)AuditLogVendorAction.Update,
                   Description = $"Updated vendor address of vendor with id {request.Id}",
                   UserId = request.UserId
               });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
