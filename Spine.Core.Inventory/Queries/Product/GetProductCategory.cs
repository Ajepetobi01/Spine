using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Product
{
    public static class GetProductCategory
    {
        public class Query : IRequest<Response>
        {
            public Guid CompanyId { get; set; }
            public Guid Id { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Guid? InventoryAccountId { get; set; }
            public Guid? SalesAccountId { get; set; }
            public Guid? CostOfSalesAccountId { get; set; }
            public bool ApplyTaxOnPO { get; set; }
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
                var item = await _dbContext.ProductCategories
                    .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id)
                    .Select(x => new Response
                    {
                        Id = x.Id, Name = x.Name, SalesAccountId = x.SalesAccountId,
                        CostOfSalesAccountId = x.CostOfSalesAccountId, InventoryAccountId = x.InventoryAccountId,
                        ApplyTaxOnPO = x.ApplyTaxOnPO
                    }).SingleOrDefaultAsync();

                return item;
            }
        }
    }
}
