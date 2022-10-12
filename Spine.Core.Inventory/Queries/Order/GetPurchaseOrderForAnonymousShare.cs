using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Order
{
    public static class GetPurchaseOrderForAnonymousShare
    {
        public class Query : IRequest<Response>
        {
            public Guid OrderId { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public string VendorEmail { get; set; }
            public string VendorName { get; set; }
            public string CompanyAddress { get; set; }
            public string CompanyPhone { get; set; }
            public string CompanyLogoId { get; set; }
            public decimal Amount { get; set; }

            public DateTime OrderDate { get; set; }
            public DateTime? ExpectedDate { get; set; }
            public string Status { get; set; }

            public string AdditionalNote { get; set; }
            public List<GetPurchaseOrder.LineItemModel> LineItems { get; set; }

            [JsonIgnore]
            public Guid CompanyId { get; set; }
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
                var item = await (from order in _dbContext.PurchaseOrders.Where(x => !x.IsDeleted && x.Id == request.OrderId)
                                  join company in _dbContext.Companies on order.CompanyId equals company.Id
                                  select new Response
                                  {
                                      Id = order.Id,
                                      CompanyLogoId = company.LogoId,
                                      CompanyId = company.Id,
                                      CompanyAddress = company.Address,
                                      CompanyPhone = company.PhoneNumber,
                                      VendorEmail = order.VendorEmail,
                                      VendorName = order.VendorName,
                                      AdditionalNote = order.AdditionalNote,
                                      Status = order.Status.GetDescription(),
                                      ExpectedDate = order.ExpectedDate,
                                      OrderDate = order.OrderDate,
                                      Amount = order.OrderAmount,
                                  }).SingleOrDefaultAsync();

                if (item == null) return null;

                var lineItems = await _dbContext.LineItems.Where(x => x.ParentItemId == item.Id)
                    .OrderBy(x => x.CreatedOn)
                    .Select(d => new GetPurchaseOrder.LineItemModel
                    {
                        Description = d.Description,
                        Item = d.Item,
                        Amount = d.Amount,
                        Quantity = d.Quantity,
                        Rate = d.Rate,
                        Id = d.Id,
                        TaxAmount = d.TaxAmount,
                        TaxId = d.TaxId,
                        TaxLabel = d.TaxLabel,
                        TaxRate = d.TaxRate
                    }).ToListAsync();

                item.LineItems = lineItems;

                return item;
            }
        }
    }
}
