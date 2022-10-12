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

namespace Spine.Core.Inventories.Queries.Service
{
    public static class GetServices
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinSalesPrice { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxSalesPrice { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Name),
               // nameof(Model.Description),
                nameof(Model.SalesPrice),
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

            [Sortable("SalesPrice")]
            public decimal SalesPrice { get; set; }

            [Sortable("Status")]
            public InventoryStatus StatusEnum { get; set; }
            public string InventoryStatus { get; set; }

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
                var query = from inv in _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId && x.InventoryType == InventoryType.Service && !x.IsDeleted)
                            join cat in _dbContext.TransactionCategories on inv.CategoryId equals cat.Id into transCat
                            from cat in transCat.DefaultIfEmpty()
                            where cat == null || !cat.IsDeleted
                            select new Model
                            {
                                Id = inv.Id,
                                Name = inv.Name,
                                CreatedOn = inv.CreatedOn,
                                SalesPrice = inv.UnitSalesPrice,
                                InventoryStatus = inv.Status.GetDescription(),
                                StatusEnum = inv.Status,
                                Description = inv.Description,
                            };

                if (request.StartDate.HasValue) query = query.Where(x => x.CreatedOn >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.CreatedOn.Date <= request.EndDate);
                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.Search)
                                                                                                                        || x.Description.Contains(request.Search));

                if (request.MinSalesPrice != null) query = query.Where(x => x.SalesPrice >= request.MinSalesPrice);
                if (request.MaxSalesPrice != null) query = query.Where(x => x.SalesPrice <= request.MaxSalesPrice);

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
