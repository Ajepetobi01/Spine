using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoiceTypes
    {
        public class Query : IRequest<Response>
        {
        }

        public class Response : List<Model>
        {
        }

        public class Model
        {
            public int Id { get; set; }
            public string Type { get; set; }
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
                var items = await _dbContext.InvoiceTypes.Select(x => new Model { Id = x.Id, Type = x.Type }).ToListAsync();

                return _mapper.Map<Response>(items);
            }
        }
    }
}
