using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Companies
{
    public static class GetCompanyBaseCurrency
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public string Currency { get; set; }
            public string CurrencyCode { get; set; }
            public string Symbol { get; set; }
            
            public int Id { get; set; }

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
                var data = await (from comp in _dbContext.Companies.Where(
                        x => x.Id == request.CompanyId && !x.IsDeleted)
                    join currency in _dbContext.Currencies on comp.BaseCurrencyId equals currency.Id
                   
                    select new Response
                    {
                        Id = currency.Id,
                        Currency = currency.Name,
                        CurrencyCode = currency.Code,
                        Symbol = currency.Symbol
                    }).SingleOrDefaultAsync();

                return data;
            }
        }
    }
}
