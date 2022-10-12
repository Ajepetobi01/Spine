using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Order
{
    public static class GetPurchaseOrder
    {
        public class Query : IRequest<Response>
        {
            public Guid CompanyId { get; set; }

            public Guid Id { get; set; }

        }

        public class Response
        {
            public Guid Id { get; set; }
            public Guid? VendorId { get; set; }
            public string VendorName { get; set; }
            public string VendorEmail { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? ExpectedDate { get; set; }
            public decimal OrderValue { get; set; }
            public string AdditionalNote { get; set; }
            public PurchaseOrderStatus OrderStatusEnum { get; set; }
            public string OrderStatus { get; set; }
            public DateTime CreatedOn { get; set; }

            public List<LineItemModel> LineItems { get; set; }
        }

        public class LineItemModel
        {
            public Guid Id { get; set; }
            public string Item { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public decimal Rate { get; set; }
            public decimal Amount { get; set; }
            
            public decimal TaxAmount { get; set; }
            
            public string TaxLabel { get; set; }
            public decimal TaxRate { get; set; }
            
            public Guid? TaxId { get; set; }
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
                var item = await (from order in _dbContext.PurchaseOrders.Where(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted)
                                  select new Response
                                  {
                                      Id = order.Id,
                                      VendorId = order.VendorId,
                                      VendorName = order.VendorName,
                                      VendorEmail = order.VendorEmail,
                                      AdditionalNote = order.AdditionalNote,
                                      OrderDate = order.OrderDate,
                                      ExpectedDate = order.ExpectedDate,
                                      OrderValue = order.OrderAmount,
                                      CreatedOn = order.CreatedOn,
                                      OrderStatus = order.Status.GetDescription(),
                                      OrderStatusEnum = order.Status,
                                  }).SingleOrDefaultAsync();

                if (item == null) return null;

                var lineItems = await _dbContext.LineItems.Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == item.Id)
                    .OrderBy(x => x.CreatedOn)
                    .Select(d => new LineItemModel
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
