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

namespace Spine.Core.Inventories.Commands.Product
{
    public static class UpdateProduct
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }
            //    public Guid? ParentInventoryId { get; set; }

            [Required]
            public string Name { get; set; }
            [Required]
            public Guid? CategoryId { get; set; }
            [Required]
            public DateTime? InventoryDate { get; set; }

            [Required]
            public string SerialNo { get; set; }

            public string SKU { get; set; }

            [Required]
            public int ReorderLevel { get; set; }

            public string Description { get; set; }

            public int? MeasurementUnitId { get; set; }

            // [Required]
            // public decimal UnitCostPrice { get; set; }
            [Required]
            public decimal UnitSalesPrice { get; set; }
            public bool NotifyOutOfStock { get; set; }

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
                                                 (x.Id == request.Id || x.Name.ToLower() == request.Name.ToLower()
                                                    || x.SerialNo.ToLower() == request.SerialNo.ToLower())).ToListAsync();

                if (inventories.Count == 0) return new Response("Product not found");
                if (inventories.Any(x => x.Id != request.Id))
                {
                    return new Response("Another product with this name/serial number exists");
                }

                var inventory = inventories.First();

                if (inventory.UnitSalesPrice != request.UnitSalesPrice)
                {
                    _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                    {
                        CompanyId = request.CompanyId,
                        CreatedBy = request.UserId,
                        InventoryId = inventory.Id,
                        UnitCostPrice = inventory.UnitCostPrice,
                        UnitSalesPrice = request.UnitSalesPrice,
                        RestockDate = DateTime.Today,
                        CreatedOn = DateTime.Today
                    });
                }

                _mapper.Map(request, inventory);

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.Update,
                        Description = $"Updated product inventory {inventory.Id} with {request.Name } - {request.SerialNo}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
