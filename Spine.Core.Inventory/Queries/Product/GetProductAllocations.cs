using System;
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

namespace Spine.Core.Inventories.Queries.Product
{
    public static class GetProductAllocations
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid? ProductId { get; set; }

            public string Search { get; set; }

            public string ProductName { get; set; }
            public string LocationName { get; set; }
            public string State { get; set; }

            public InventoryStatus? Status { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.ProductName),
                nameof(Model.Location),
                nameof(Model.State),
                nameof(Model.Quantity),
                nameof(Model.LastAllocationDate)
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "asc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            [Sortable("ProductName", IsDefault = true)]
            public string ProductName { get; set; }

            [Sortable("Location")]
            public string Location { get; set; }

            [Sortable("State")]
            public string State { get; set; }

            [Sortable("Quantity")]
            public int Quantity { get; set; }

            public string Status { get; set; }

            [JsonIgnore]
            public InventoryStatus StatusEnum { get; set; }

            [Sortable("LastAllocationDate")]
            public DateTime LastAllocationDate { get; set; }
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
                var query = from loc in _dbContext.ProductLocations.Where(x => x.CompanyId == request.CompanyId
                                                        && (!request.ProductId.HasValue || request.ProductId == x.InventoryId))
                            join location in _dbContext.InventoryLocations.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted) on loc.LocationId equals location.Id
                            join prod in _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                && x.Status == InventoryStatus.Active && x.InventoryType == InventoryType.Product) on loc.InventoryId equals prod.Id
                            select new
                            {
                                ProductName = prod.Name,
                                prod.Status,
                                loc.DateAdded,
                                Location = location.Name,
                                location.State,
                                loc.QuantityInStock,
                            };

                var groupedQuery = query.GroupBy(x =>
                        new {x.ProductName, x.Status, x.Location, x.State})
                    .Select(x =>
                        new Model
                        {
                            ProductName = x.Key.ProductName,
                            State = x.Key.State,
                            Location = x.Key.Location,
                            LastAllocationDate = x.Max(d => d.DateAdded),
                            Quantity = x.Sum(d => d.QuantityInStock),
                            StatusEnum = x.Key.Status
                        });

                if (!request.Search.IsNullOrEmpty()) groupedQuery = groupedQuery.Where(x => x.ProductName.Contains(request.Search)
                        || x.Location.Contains(request.Search) || x.State.Contains(request.Search));

                else
                {
                    if (!request.ProductName.IsNullOrEmpty()) groupedQuery = groupedQuery.Where(x => x.ProductName.Contains(request.ProductName));
                    if (!request.LocationName.IsNullOrEmpty()) groupedQuery = groupedQuery.Where(x => x.Location.Contains(request.LocationName));
                    if (!request.State.IsNullOrEmpty()) groupedQuery = groupedQuery.Where(x => x.State.Contains(request.State));

                    if (request.StartDate.HasValue) groupedQuery = groupedQuery.Where(x => x.LastAllocationDate >= request.StartDate);
                    if (request.EndDate.HasValue) groupedQuery = groupedQuery.Where(x => x.LastAllocationDate.Date <= request.EndDate);

                    if (request.Status.HasValue) groupedQuery = groupedQuery.Where(x => x.StatusEnum == request.Status);
                }

                groupedQuery = groupedQuery.OrderBy(request.SortByAndOrder);
                Response items;
                if (request.Page == 0)
                    items = _mapper.Map<Response>(await groupedQuery.ToListAsync());

                else
                    items = await groupedQuery.ToPageResultsAsync<Model, Response>(request);

                foreach (var item in items.Items)
                {
                    item.Status = item.StatusEnum.GetDescription();
                }

                return items;
            }
        }

    }
}
