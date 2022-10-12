using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Companies
{
    public static class GetCompanyFinancial
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public decimal Sale { get; set; }
            public decimal Gross { get; set; }
            public decimal NetProfitBeforeTax { get; set; }
            public decimal NetProfitAfterTax { get; set; }
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
                //var data = await _dbContext.CompanyFinancials.SingleOrDefaultAsync(x => x.Id == request.CompanyId );

                //if (data == null) return null;

                return new Response
                {
                };
            }
        }
    }
}
