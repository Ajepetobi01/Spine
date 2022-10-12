using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetTaxType
    {
        public class Query : IRequest<Model>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }
        }

        public class Model
        {
            public string Tax { get; set; }
            public double TaxRate { get; set; }
            public bool IsCompund { get; set; }
            public bool IsActive { get; set; }
        }

        public class Response : Model
        {
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Model> Handle(Query request, CancellationToken token)
            {
                var tax = await (from cat in _dbContext.TaxTypes.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                 && x.Id == request.Id)
                                 select new Model
                                 {
                                     Tax = cat.Tax,
                                     TaxRate = cat.TaxRate,
                                     IsCompund = cat.IsCompound,
                                     IsActive = cat.IsActive
                                 }).SingleOrDefaultAsync();

                return tax;

            }
        }

    }
}
