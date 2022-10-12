using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Attributes;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Inventories.Queries.Order
{
    public static class GetReceivedGoodsForReturn
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore] public Guid CompanyId { get; set; }

            [JsonIgnore] public Guid? GoodsReceivedId { get; set; }

            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[]
            {
                nameof(Model.Vendor),
                nameof(Model.Product),
                nameof(Model.Quantity),
                nameof(Model.QuantityReturned),
                nameof(Model.CreatedOn),
            })]
            public string SortBy { get; set; }

            [StringRange(new[] {"asc", "ascending", "desc", "descending"})]
            public string Order { get; set; } = "desc";

            [JsonIgnore] public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid LineItemId { get; set; }
            [Sortable("Vendor")] public string Vendor { get; set; }

            [Sortable("Product")] public string Product { get; set; }

            public string GoodsReceivedNo { get; set; }
            
            [Sortable("Quantity")] public int Quantity { get; set; }
            [Sortable("QuantityReturned")] public int QuantityReturned { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }
            
        }

        public class Response : Spine.Common.Models.PagedResult<Model>
        {
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
                var query = from lineItem in _dbContext.ReceivedGoodsLineItems.Where(x =>
                        x.CompanyId == request.CompanyId && x.GoodReceivedId == request.GoodsReceivedId)
                    join good in _dbContext.ReceivedGoods on lineItem.GoodReceivedId equals good.Id
                    join vendor in _dbContext.Vendors on good.VendorId equals vendor.Id
                    join inv in _dbContext.Inventories on lineItem.InventoryId equals inv.Id
                    select new Model
                    {
                        Id = good.Id,
                        LineItemId = lineItem.Id,
                        Vendor = vendor.Name,
                        Product = inv.Name,
                        Quantity = lineItem.Quantity,
                        QuantityReturned = lineItem.ReturnedQuantity,
                        GoodsReceivedNo = good.GoodReceivedNo,
                        CreatedOn = good.CreatedOn,
                    };

                if (request.StartDate.HasValue) query = query.Where(x => x.CreatedOn >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.CreatedOn.Date <= request.EndDate);

                if (!request.Search.IsNullOrEmpty())
                {
                    query = query.Where(x => x.Vendor.Contains(request.Search)
                                             || x.Product.Contains(request.Search) ||
                                             x.GoodsReceivedNo.Contains(request.Search));
                }

                query = query.OrderBy(request.SortByAndOrder);
                Response items;
                if (request.Page == 0)
                    items = _mapper.Map<Response>(await query.ToListAsync());

                else
                    items = await query.ToPageResultsAsync<Model, Response>(request);

                return items;
            }
        }
    }
}
