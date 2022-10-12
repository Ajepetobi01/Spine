using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Transactions.Queries
{
    public static class GetTransactionCategories
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public bool IsInflow { get; set; }
            public Guid? ParentCategoryId { get; set; }

        }

        public class Response : List<Model>
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
                var categories = await (from cat in _dbContext.TransactionCategories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                                        select new Model
                                        {
                                            Id = cat.Id,
                                            IsInflow = cat.IsInflow,
                                            Name = cat.Name,
                                            ParentCategoryId = cat.ParentCategoryId,
                                        }).ToListAsync();

                return _mapper.Map<Response>(categories);

            }
        }

    }
}
