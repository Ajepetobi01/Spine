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
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Service
{
    public static class UpdateService
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
            public string Name { get; set; }

            public string Description { get; set; }

            [Required]
            public decimal UnitSalesPrice { get; set; }
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
                var inventories = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted &&
                                                 (x.Id == request.Id || x.Name.ToLower() == request.Name.ToLower())).ToListAsync();

                if (inventories.Count == 0) return new Response("Service not found");
                if (inventories.Any(x => x.Id != request.Id))
                {
                    return new Response("Another service with this name exists");
                }

                var inventory = inventories.First();

                if (inventory.UnitSalesPrice != request.UnitSalesPrice)
                {
                    _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                    {
                        CompanyId = request.CompanyId,
                        CreatedBy = request.UserId,
                        InventoryId = inventory.Id,
                        UnitCostPrice = 0.00m,
                        UnitSalesPrice = request.UnitSalesPrice,
                        RestockDate = DateTime.Today,
                        CreatedOn = DateTime.Today
                    });
                }
                
                inventory.Name = request.Name;
                inventory.Description = request.Description;
                inventory.UnitSalesPrice = request.UnitSalesPrice;
                inventory.LastModifiedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.Update,
                        Description = $"Updated service inventory {inventory.Id} with {request.Name }",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
