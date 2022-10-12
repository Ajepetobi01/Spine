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

namespace Spine.Core.Inventories.Queries.Product
{
    public static class GetProducts
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }
            public string ProductName { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public bool? BelowThreshold { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinPurchasePrice { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxPurchasePrice { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinSalesPrice { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxSalesPrice { get; set; }

            public int? MinQuantity { get; set; }
            public int? MaxQuantity { get; set; }

            public InventoryStatus? Status { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Name),
               // nameof(Model.Description),
                nameof(Model.Quantity),
                nameof(Model.SalesPrice),
                nameof(Model.PurchasePrice),
                   nameof(Model.Quantity),
                nameof(Model.InventoryDate),
                 nameof(Model.LastRestockDate),
                nameof(Model.CreatedOn),
                nameof(Model.InventoryStatus)
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "desc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            public Guid Id { get; set; }
            [Sortable("Name")]
            public string Name { get; set; }
            //   [Sortable("Description")]
            public string Description { get; set; }
            [Sortable("Quantity")]
            public int Quantity { get; set; }

            [Sortable("InventoryDate")]
            public DateTime InventoryDate { get; set; }

            [Sortable("LastRestockDate")]
            public DateTime LastRestockDate { get; set; }

            [Sortable("PurchasePrice")]
            public decimal PurchasePrice { get; set; }
            [Sortable("SalesPrice")]
            public decimal SalesPrice { get; set; }

            [Sortable("InventoryStatus")]
            public InventoryStatus InventoryStatusEnum { get; set; }
            public string InventoryStatus { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }

            public int ReorderLevel { get; set; }
            public string SerialNo { get; set; }
            public string SKU { get; set; }
            public string Category { get; set; }
            public string MeasurementUnit { get; set; }
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
                var query = from inv in _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId && x.InventoryType == InventoryType.Product && !x.IsDeleted)
                            join cat in _dbContext.ProductCategories on inv.CategoryId equals cat.Id
                            where !cat.IsDeleted
                            join unit in _dbContext.MeasurementUnits on inv.MeasurementUnitId equals unit.Id into invUnit
                            from unit in invUnit.DefaultIfEmpty()
                            select new Model
                            {
                                Id = inv.Id,
                                Name = inv.Name,
                                LastRestockDate = inv.LastRestockDate,
                                InventoryDate = inv.InventoryDate,
                                CreatedOn = inv.CreatedOn,
                                SalesPrice = inv.UnitSalesPrice,
                                PurchasePrice = inv.UnitCostPrice,
                                InventoryStatus = inv.Status.GetDescription(),
                                InventoryStatusEnum = inv.Status,
                                Quantity = inv.QuantityInStock,
                                Description = inv.Description,
                                SKU = inv.SKU,
                                Category = cat.Name ?? "",
                                MeasurementUnit = unit.Name ?? "",
                                SerialNo = inv.SerialNo,
                                ReorderLevel = inv.ReorderLevel
                            };

                if (request.StartDate.HasValue) query = query.Where(x => x.InventoryDate >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.InventoryDate.Date <= request.EndDate);
                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.Search)
                                                                                                                        || x.Description.Contains(request.Search));

                else
                {
                    if (!request.ProductName.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.ProductName));
                }

                if (request.MinPurchasePrice != null) query = query.Where(x => x.PurchasePrice >= request.MinPurchasePrice);
                if (request.MaxPurchasePrice != null) query = query.Where(x => x.PurchasePrice <= request.MaxPurchasePrice);

                if (request.MinSalesPrice != null) query = query.Where(x => x.SalesPrice >= request.MinSalesPrice);
                if (request.MaxSalesPrice != null) query = query.Where(x => x.SalesPrice <= request.MaxSalesPrice);

                if (request.MinQuantity != null) query = query.Where(x => x.Quantity >= request.MinQuantity);
                if (request.MaxQuantity != null) query = query.Where(x => x.Quantity <= request.MaxQuantity);

                if (request.Status.HasValue) query = query.Where(x => x.InventoryStatusEnum == request.Status);

                if (request.BelowThreshold.HasValue)
                {
                    query = request.BelowThreshold.Value
                        ? query.Where(x => x.Quantity < x.ReorderLevel)
                        : query.Where(x => x.Quantity >= x.ReorderLevel);
                }

                query = query.OrderBy(request.SortByAndOrder);
                if (request.Page == 0)
                {
                    return _mapper.Map<Response>(await query.ToListAsync());
                }
                
                return await query.ToPageResultsAsync<Model, Response>(request);
            }
        }
    }
}
