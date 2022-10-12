using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Attributes;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Inventories.Queries.Order
{
    public static class GetReceivedGoods
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            
            [JsonIgnore]
            public Guid? VendorId { get; set; }
            
            [JsonIgnore]
            public Guid? OrderId { get; set; }

            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            // [Column(TypeName = "decimal(18,2")]
            // public decimal? MinAmount { get; set; }
            // [Column(TypeName = "decimal(18,2")]
            // public decimal? MaxAmount { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Vendor),
                nameof(Model.Product),
                nameof(Model.DateReceived),
                nameof(Model.Quantity),
                   nameof(Model.Amount),
                nameof(Model.CreatedOn),
                nameof(Model.Balance),
                nameof(Model.PaymentDueDate),
                nameof(Model.GoodsReceivedNo),
                nameof(Model.PurchaseOrderNo),
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "desc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid GoodsReceivedId { get; set; }
            [Sortable("Vendor")] public string Vendor { get; set; }

            public string VendorEmail { get; set; }
            public string VendorPhone { get; set; }

            [Sortable("Product")] public string Product { get; set; }

            [Sortable("DateReceived")] public DateTime DateReceived { get; set; }
            [Sortable("Quantity")] public int Quantity { get; set; }

            [Sortable("Amount")] public decimal Amount { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }
            
            public bool IsAttachedToOrder { get; set; }
            
            [Sortable("Balance")] public decimal Balance { get; set; }
            [Sortable("PurchaseOrderNo")] public string PurchaseOrderNo { get; set; }
            [Sortable("GoodsReceivedNo")] public string GoodsReceivedNo { get; set; }
            [Sortable("PaymentDueDate")] public DateTime? PaymentDueDate { get; set; }
            
            public string PaymentStatus { get; set; }
            [JsonIgnore] public Guid VendorId { get; set; }
            [JsonIgnore] public Guid? OrderId { get; set; }
        }

        public class Response : Spine.Common.Models.PagedResult<Model>
        {
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
                var query = from lineItem in _dbContext.ReceivedGoodsLineItems.Where(x =>
                        x.CompanyId == request.CompanyId)
                    join received in _dbContext.ReceivedGoods.Where(x =>
                        x.CompanyId == request.CompanyId) on lineItem.GoodReceivedId equals received.Id
                    join po in _dbContext.PurchaseOrders on received.PurchaseOrderId equals po.Id into orders
                    from po in orders.DefaultIfEmpty()
                    join vendor in _dbContext.Vendors on received.VendorId equals vendor.Id
                    join inv in _dbContext.Inventories on lineItem.InventoryId equals inv.Id
                    select new Model
                    {
                        Id = lineItem.Id,
                        GoodsReceivedId = lineItem.GoodReceivedId,
                        Vendor = vendor.Name,
                        VendorEmail = vendor.Email,
                        VendorPhone = vendor.PhoneNumber,
                        Product = inv.Name,
                        DateReceived = received.DateReceived,
                        Quantity = lineItem.Quantity,
                        Amount = lineItem.Amount,
                        Balance = lineItem.Balance,
                        IsAttachedToOrder = po != null,
                        PaymentDueDate = received.PaymentDueDate,
                        PaymentStatus = received.PaymentStatus.GetDescription(),
                        GoodsReceivedNo = received.GoodReceivedNo,
                        PurchaseOrderNo = po.OrderNo ?? "",
                        CreatedOn = received.CreatedOn,
                        VendorId = vendor.Id,
                        OrderId = received.PurchaseOrderId
                    };

                if (request.VendorId.HasValue) query = query.Where(x => x.VendorId == request.VendorId);
                if (request.OrderId.HasValue) query = query.Where(x => x.OrderId == request.OrderId);
                
                if (request.StartDate.HasValue) query = query.Where(x => x.DateReceived >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.DateReceived.Date <= request.EndDate);
                
                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.Vendor.Contains(request.Search) 
                                                                              || x.Product.Contains(request.Search));

                // if (request.MinAmount != null) query = query.Where(x => x.Amount >= request.MinAmount);
                // if (request.MaxAmount != null) query = query.Where(x => x.Amount <= request.MaxAmount);

                query = query.OrderBy(request.SortByAndOrder);
                Response items;
                if (request.Page == 0)
                    items = _mapper.Map<Response>(await query.ToListAsync());
                
                else
                    items = await query.ToPageResultsAsync<Model, Response>(request);
                
                foreach (var item in items.Items.Where(x=>x.Balance > 0.0m && x.Amount != x.Balance))
                {
                    if (!item.PaymentDueDate.HasValue) continue;
                    if (item.PaymentDueDate.Value >= DateTime.Today) continue;
                    
                    var dateDiff = (DateTime.Today - item.PaymentDueDate.Value.Date).Duration().Days;
                    item.PaymentStatus = $"Overdue by {dateDiff} day(s)";
                }
                
                return items;
            }
        }
    }
}
