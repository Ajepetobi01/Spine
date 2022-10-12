using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Order
{
    public static class GetPurchaseOrderWithReceivedItems
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
            
            public int Quantity { get; set; }
            public decimal Rate { get; set; }
            public decimal Amount { get; set; }
            public decimal TaxAmount { get; set; }
            
            public string TaxLabel { get; set; }
            public decimal TaxRate { get; set; }
            
            public Guid? TaxId { get; set; }
            
            public List<ReceivedItemModel> ReceivedItems { get; set; }

        }
        public class ReceivedItemModel
        {
            public Guid LineItemId { get; set; }
            public DateTime DateReceived { get; set; }
            public string ReceivedBy { get; set; }
            public string Item { get; set; }
            public string Description { get; set; }
            public int OrderQuantity { get; set; }
            public int Quantity { get; set; }
            public decimal Rate { get; set; }
            public decimal Amount { get; set; }
            public decimal Balance { get; set; }
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
                        Quantity = d.Quantity,
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
                            select new ReceivedItemModel
                            {
                                Amount = goodItem.Amount,
                                Balance = goodItem.Balance,
                                LineItemId = goodItem.OrderLineItemId.Value,
                                DateReceived = good.DateReceived,
                                ReceivedBy = user.FullName,
                                Quantity = goodItem.Quantity,
                                Rate = goodItem.Rate,
                                Item = goodItem.Item,
                                Description = goodItem.Description,
                                TaxAmount = goodItem.TaxAmount,
                                TaxId = goodItem.TaxId,
                                TaxLabel = goodItem.TaxLabel,
                                TaxRate = goodItem.TaxRate
                            }).OrderBy(x => x.LineItemId)
                        .ToListAsync())
                    .ToLookup(x => x.LineItemId);

                foreach (var item in lineItems)
                {
                    item.ReceivedItems = itemsReceived[item.LineItemId].ToList();
                }

                order.LineItems = lineItems;
                return order;

            }
        }
    }
}
