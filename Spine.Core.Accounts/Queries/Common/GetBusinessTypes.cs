using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Common
{
    public static class GetBusinessTypes
    {
        public class Query : IRequest<List<string>>
        {
        }

        public class Response : List<string>
        {
        }

        public class Handler : IRequestHandler<Query, List<string>>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<List<string>> Handle(Query request, CancellationToken token)
            {
                var items = await _dbContext.BusinessTypes.Select(x => x.Type).ToListAsync();
                return items;
            }
        }
    }
}
