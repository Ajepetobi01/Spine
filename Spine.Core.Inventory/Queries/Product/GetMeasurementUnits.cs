using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Product
{
    public static class GetMeasurementUnits
    {
        public class Query : IRequest<List<Model>>
        {
        }

        public class Model
        {
            public int Id { get; set; }
            public string Unit { get; set; }
            public string Name { get; set; }
        }

        public class Response : List<Model>
        {
        }

        public class Handler : IRequestHandler<Query, List<Model>>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<List<Model>> Handle(Query request, CancellationToken token)
            {
                var items = await _dbContext.MeasurementUnits.Select(x => new Model { Id = x.Id, Name = x.Name, Unit = x.Unit }).ToListAsync();
                return items;
            }
        }
    }
}
