using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Customers.Queries
{
    public static class GetCustomerCount
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public int CustomerCount { get; set; }
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
                var itemCount = await _dbContext.Customers.CountAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted);

                return new Response { CustomerCount = itemCount };
            }
        }

    }
}
