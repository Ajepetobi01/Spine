using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Data;

namespace Spine.Core.Inventories.Queries
{
    public static class GetInventoryStat
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public int Products { get; set; }
            public int Services { get; set; }
            public int BelowThreshold { get; set; }
            public int WithinThreshold { get; set; }

        }


        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var inventories = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    .Select(x => new { x.InventoryType, x.ReorderLevel, x.QuantityInStock }).ToListAsync();

                return new Response
                {
                    Products = inventories.Count(x => x.InventoryType == InventoryType.Product),
                    Services = inventories.Count(x => x.InventoryType == InventoryType.Service),
                    WithinThreshold = inventories.Count(x => x.InventoryType == InventoryType.Product && x.QuantityInStock >= x.ReorderLevel),
                    BelowThreshold = inventories.Count(x => x.InventoryType == InventoryType.Product && x.QuantityInStock < x.ReorderLevel)

                };
            }
        }

    }
}
