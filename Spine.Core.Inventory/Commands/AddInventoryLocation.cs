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
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands
{
    public static class AddInventoryLocation
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }


            [Required]
            public string Name { get; set; }
            [Required]
            public string PhoneNumber { get; set; }
            [Required]
            public string Address { get; set; }
            [Required]
            public string State { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.OK;
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
                if (await _dbContext.InventoryLocations.AnyAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Name.ToLower() == request.Name.ToLower()))
                {
                    return new Response("A location with this name exists");
                }

                var location = _mapper.Map<InventoryLocation>(request);
                _dbContext.InventoryLocations.Add(location);

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.CreateInventoryLocation,
                        Description = $"Added new inventory location {request.Name}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.Created)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
