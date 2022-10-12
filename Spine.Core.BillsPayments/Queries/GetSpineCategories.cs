using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.BillsPayments.Queries
{
    public static class GetSpineCategories
    {
        public class Query : IRequest<Response>
        {
        }

        public class Response : List<Model>
        {

        }

        public class Model
        {
            public string CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string Description { get; set; }

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
                var cats = await _dbContext.BillCategories.Select(x =>
                new Model { CategoryId = x.CategoryId, CategoryName = x.CategoryName, Description = x.Description })
                    .ToListAsync();

                return _mapper.Map<Response>(cats);

            }
        }

    }
}
