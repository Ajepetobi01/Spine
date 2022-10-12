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
using Spine.Data.Entities.Vendor;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Vendor
{
    public static class AddVendorAddress
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid VendorId { get; set; }

            public bool IsPrimary { get; set; }
            public bool IsBilling { get; set; }

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
            public Guid Id { get; set; }
            public Response(Guid id)
            {
                Id = id;
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper, IMapper mapper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var vendor = await _dbContext.Vendors.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.VendorId && !x.IsDeleted);

                if (vendor == null) return new Response("Vendor not found");

                var add = _mapper.Map<VendorAddress>(request);
                _dbContext.VendorAddresses.Add(add);

                if (add.IsPrimary)
                {
                    var others = await _dbContext.VendorAddresses.Where(x => x.CompanyId == request.CompanyId && x.VendorId == request.VendorId
                                            && x.IsBilling == request.IsBilling && x.IsPrimary).ToListAsync();
                    others.ForEach(x => x.IsPrimary = false);
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
               new AuditModel
               {
                   EntityType = (int)AuditLogEntityType.Vendor,
                   Action = (int)AuditLogVendorAction.Update,
                   Description = $"Added new address for vendor with email {vendor.Email} and id {vendor.Id}",
                   UserId = request.UserId
               });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(add.Id)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
