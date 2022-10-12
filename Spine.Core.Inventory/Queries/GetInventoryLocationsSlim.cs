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

namespace Spine.Core.Inventories.Queries
{
    public static class GetInventoryLocationsSlim
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response : List<Model>
        {
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
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
                var items = await _dbContext.InventoryLocations
                    .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                    && x.Status == Common.Enums.Status.Active)
                    .Select(x => new Model { Id = x.Id, Name = x.Name }).ToListAsync();

                return _mapper.Map<Response>(items);
            }
        }
    }
}
