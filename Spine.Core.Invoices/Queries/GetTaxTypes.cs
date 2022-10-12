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

namespace Spine.Core.Invoices.Queries
{
    public static class GetTaxTypes
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Tax { get; set; }
            public double TaxRate { get; set; }
            public bool IsCompund { get; set; }
            public bool IsActive { get; set; }
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
                var taxes = await (from cat in _dbContext.TaxTypes.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                                   select new Model
                                   {
                                       Id = cat.Id,
                                       Tax = cat.Tax,
                                       TaxRate = cat.TaxRate,
                                       IsCompund = cat.IsCompound,
                                       IsActive = cat.IsActive,
                                   }).ToListAsync();

                return _mapper.Map<Response>(taxes);

            }
        }

    }
}
