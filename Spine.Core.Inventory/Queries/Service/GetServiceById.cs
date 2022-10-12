using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Service
{
    public static class GetServiceById
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            public Guid Id { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal SalesPrice { get; set; }
            public string InventoryStatus { get; set; }
            public DateTime CreatedOn { get; set; }

            public List<NoteModel> Notes { get; set; }
        }

        public class NoteModel
        {
            public Guid Id { get; set; }
            public string Note { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var item = await (from inv in _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId && x.InventoryType == InventoryType.Service
                                  && !x.IsDeleted && x.Id == request.Id)
                                  select new Response
                                  {
                                      Id = inv.Id,
                                      Name = inv.Name,
                                      InventoryStatus = inv.Status.GetDescription(),
                                      CreatedOn = inv.CreatedOn,
                                      SalesPrice = inv.UnitSalesPrice,
                                      Description = inv.Description,
                                  }).SingleOrDefaultAsync();

                if (item != null)
                {
                    item.Notes = await _dbContext.InventoryNotes.Where(x => x.CompanyId == request.CompanyId
                                                                                                                             && !x.IsDeleted && x.InventoryId == item.Id)
                                                                                                                             .Select(x => new NoteModel { Note = x.Note, Id = x.Id })
                                                                                                                             .ToListAsync();
                }

                return item;
            }
        }

    }
}
