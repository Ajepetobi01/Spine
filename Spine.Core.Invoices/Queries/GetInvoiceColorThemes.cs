using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoiceColorThemes
    {
        public class Query : IRequest<List<Model>>
        {
        }

        public class Response : List<Model>
        {
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Theme { get; set; }
            public string TextColor { get; set; }
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
                var items = await _dbContext.InvoiceColorThemes.Select(x => new Model
                {
                    Id = x.Id,
                    TextColor = x.TextColor,
                    Name = x.Name,
                    Theme = x.Theme
                }).ToListAsync();

                return items;
            }
        }
    }
}
