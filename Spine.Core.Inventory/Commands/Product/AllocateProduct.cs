using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Product
{
    public static class AllocateProduct
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid InventoryId { get; set; }

            public List<Allocation> Allocations { get; set; }

        }

        public class Allocation
        {
            public Guid LocationId { get; set; }
            public int Quantity { get; set; }
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var product = await _dbContext.Inventories.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId
                        && x.InventoryType == InventoryType.Product && x.Id == request.InventoryId && !x.IsDeleted);

                if (product == null) return new Response("Product not found");

                if (product.Status != InventoryStatus.Active) return new Response("Product is not in active status");

                if (product.QuantityInStock < request.Allocations.Sum(x => x.Quantity)) return new Response("Total allocations cannot be more than the quantity in stock");

                var allocations = new List<ProductLocation>();
                foreach (var item in request.Allocations)
                {
                    allocations.Add(new ProductLocation
                    {
                        CompanyId = request.CompanyId,
                        CreatedBy = request.UserId,
                        InventoryId = request.InventoryId,
                        LocationId = item.LocationId,
                        QuantityInStock = item.Quantity,
                        DateAdded = DateTime.Today
                    });
                }

                _dbContext.ProductLocations.AddRange(allocations);

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.Allocate,
                        Description = $"Allocated inventory  {product.Name} - {product.SerialNo} to locations {string.Join(",", request.Allocations.Select(x => x.LocationId))}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
