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
    public static class GetPurchaseOrderForConfirmReceipt
    {
        public class Query : IRequest<Response>
        {
            public Guid CompanyId { get; set; }

            public Guid Id { get; set; }

        }

        public class Response
        {
            public Guid Id { get; set; }

            public string VendorName { get; set; }
            public string VendorEmail { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? ExpectedDate { get; set; }
            public decimal OrderValue { get; set; }
            public string AdditionalNote { get; set; }
            public PurchaseOrderStatus OrderStatus { get; set; }
            public DateTime CreatedOn { get; set; }

            public List<LineItemModel> LineItems { get; set; }

        }

        public class LineItemModel
        {
            public Guid LineItemId { get; set; }
            public Guid InventoryId { get; set; }
            public string Item { get; set; }
            public string Description { get; set; }
            public int OrderQuantity { get; set; }
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
                var order = await (from ord in _dbContext.PurchaseOrders.Where(x =>
                        x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted)
                    select new Response
                    {
                        Id = ord.Id,
                        VendorName = ord.VendorName,
                        VendorEmail = ord.VendorEmail,
                        AdditionalNote = ord.AdditionalNote,
                        OrderDate = ord.OrderDate,
                        ExpectedDate = ord.ExpectedDate,
                        OrderValue = ord.OrderAmount,
                        CreatedOn = ord.CreatedOn,
                        OrderStatus = ord.Status,
                    }).SingleOrDefaultAsync();

                if (order == null) return null;

                var lineItems = await _dbContext.LineItems
                    .Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == order.Id)
                    .OrderBy(x => x.CreatedOn)
                    .Select(d => new LineItemModel
                    {
                        Description = d.Description,
                        InventoryId = d.ItemId.Value,
                        Item = d.Item,
                        Amount = d.Amount,
                        OrderQuantity = d.Quantity,
                        Rate = d.Rate,
                        LineItemId = d.Id,
                        TaxAmount = d.TaxAmount,
                        TaxId = d.TaxId,
                        TaxLabel = d.TaxLabel,
                        TaxRate = d.TaxRate
                    }).ToListAsync();

                    var itemsReceived = (await (from good in _dbContext.ReceivedGoods
                            .Where(x => x.CompanyId == request.CompanyId && x.PurchaseOrderId == order.Id) 
                        join goodItem in _dbContext.ReceivedGoodsLineItems on good.Id equals goodItem.GoodReceivedId
                        join user in _dbContext.Users on good.ReceivedBy equals user.Id
                        where user.CompanyId == request.CompanyId
                        select new 
                        {
                            LineItem = goodItem.OrderLineItemId.Value,
                            goodItem.Id,
                           goodItem.Quantity,
                        }).ToListAsync()).ToLookup(x=>x.LineItem);

                    foreach (var item in lineItems)
                    {
                        var received = itemsReceived[item.LineItemId].ToList();
                        if (received.IsNullOrEmpty()) continue;
                        
                        var totalReceived = received.Sum(x => x.Quantity);
                        item.Quantity = item.OrderQuantity > totalReceived ? item.OrderQuantity - totalReceived : 0;
                        item.Amount = item.Rate * item.Quantity;
                    }
                    
                order.LineItems = lineItems;
                return order;

            }
        }
    }
}
