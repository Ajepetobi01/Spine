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
    public static class GetProductCategories
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Name),
                nameof(Model.CreatedOn),
                nameof(Model.Status)
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "asc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            public Guid Id { get; set; }
            [Sortable("Name", IsDefault = true)]
            public string Name { get; set; }

            [Sortable("Status")]
            public Status StatusEnum { get; set; }
            public string Status { get; set; }
            
            public string InventoryAccount { get; set; }
            public Guid? InventoryAccountId { get; set; }
           
            public string SalesAccount { get; set; }
            public Guid? SalesAccountId { get; set; }
            
            public string CostOfSalesAccount { get; set; }
            public Guid? CostOfSalesAccountId { get; set; }
            
            public bool ApplyTaxOnPO { get; set; }

            [Sortable("CreatedOn")]
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
                var query = from cat in _dbContext.ProductCategories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    join inventory in _dbContext.LedgerAccounts on cat.InventoryAccountId equals inventory.Id
                    join sales in _dbContext.LedgerAccounts on cat.SalesAccountId equals sales.Id
                    join costOfSales in _dbContext.LedgerAccounts on cat.CostOfSalesAccountId equals costOfSales.Id
                    select new Model
                            {
                                Id = cat.Id,
                                Name = cat.Name,
                                ApplyTaxOnPO = cat.ApplyTaxOnPO,
                                CreatedOn = cat.CreatedOn,
                                InventoryAccount = inventory.AccountName,
                                InventoryAccountId = inventory.Id,
                                SalesAccount = sales.AccountName,
                                SalesAccountId = sales.Id,
                                CostOfSalesAccount = costOfSales.AccountName,
                                CostOfSalesAccountId = costOfSales.Id,
                                Status = cat.Status.GetDescription(),
                                StatusEnum = cat.Status,
                            };

                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.Search));

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
