using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class CheckInvoiceSettings
    {
        public class Query : IRequest<bool>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }
        public class Handler : IRequestHandler<Query, bool>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<bool> Handle(Query request, CancellationToken token)
            {
                return await _dbContext.InvoicePreferences.AnyAsync(x => x.CompanyId == request.CompanyId);

            }
        }

    }
}
