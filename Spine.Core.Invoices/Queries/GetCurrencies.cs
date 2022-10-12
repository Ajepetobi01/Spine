using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetCurrencies
    {
        public class Query : IRequest<List<Model>>
        {
        }

        public class Response : List<Model>
        {
        }

        public class Model
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public string Code { get; set; }
        }

        public class Handler : IRequestHandler<Query, List<Model>>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<List<Model>> Handle(Query request, CancellationToken token)
            {
                var items = await _dbContext.Currencies.Select(x => new Model
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Symbol = x.Symbol
                }).ToListAsync();

                return items;
            }
        }
    }
}
