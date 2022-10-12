using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Transactions.Queries
{
    public static class GetTransactionCategory
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
            public bool IsInflow { get; set; }
            public Guid? ParentCategoryId { get; set; }

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
                var category = await (from cat in _dbContext.TransactionCategories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id)
                                      select new Response
                                      {
                                          Id = cat.Id,
                                          IsInflow = cat.IsInflow,
                                          Name = cat.Name,
                                          ParentCategoryId = cat.ParentCategoryId,
                                      }).SingleOrDefaultAsync();

                return category;

            }
        }

    }
}
