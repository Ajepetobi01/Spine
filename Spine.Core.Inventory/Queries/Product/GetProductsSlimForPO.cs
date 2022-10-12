using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Product
{
    public static class GetProductsSlimForPO
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            //   [Required]
            //  [MinLength(3)]
            public string Search { get; set; }
        }

        public class Response : List<Model>
        {
        }

        public class Model
        {
            public Guid Id { get; set; }
            
            [JsonIgnore]
            public Guid? CategoryId { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Rate { get; set; }            
            
            public bool ApplyTaxOnPO { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IMapper _mapper;

            public Handler(SpineContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var dontFilter = request.Search.IsNullOrEmpty();
                var items = await _dbContext.Inventories
                    .Where(x => x.CompanyId == request.CompanyId && x.InventoryType == InventoryType.Product &&
                                !x.IsDeleted
                                && x.Status == InventoryStatus.Active
                                && (dontFilter || x.Name.Contains(request.Search)))
                    .Select(x => new Model
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        Name = x.Name, 
                        Rate = x.UnitSalesPrice, 
                        Quantity = x.QuantityInStock
                    })
                    .ToListAsync();

                var categoryIds = items.Where(x=>x.CategoryId.HasValue).Select(x => x.CategoryId.Value).ToHashSet();

                var taxSettings = await _dbContext.ProductCategories
                    .Where(x => x.CompanyId == request.CompanyId && categoryIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, y => y.ApplyTaxOnPO);

                foreach (var item in items)
                {
                    // will be false if it's not found in the dictionary or the value is false
                    item.ApplyTaxOnPO = taxSettings.TryGetValue(item.CategoryId.Value, out var value) && value; 
                }
                
                return _mapper.Map<Response>(items);
            }
        }
    }
}
