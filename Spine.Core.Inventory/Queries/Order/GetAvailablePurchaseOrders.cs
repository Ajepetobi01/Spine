using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Order
{
    public static class GetAvailablePurchaseOrders
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            //   [Required]
            //  [MinLength(3)]
            public string Search { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string VendorName { get; set; }

            public decimal OrderValue { get; set; }

            public PurchaseOrderStatus OrderStatusEnum { get; set; }
            public string OrderStatus { get; set; }

            public string PurchaseOrderNo { get; set; }
        }

        public class Response : List<Model>
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
                var dontFilter = request.Search.IsNullOrEmpty();
                var query = (from order in _dbContext.PurchaseOrders.Where(x => x.CompanyId == request.CompanyId
                        && x.Status != PurchaseOrderStatus.Closed
                        && !x.IsDeleted)
                    select new Model
                    {
                        Id = order.Id,
                        PurchaseOrderNo = order.OrderNo,
                        VendorName = order.VendorName,
                        OrderValue = order.OrderAmount,
                        OrderStatus = order.Status.GetDescription(),
                        OrderStatusEnum = order.Status
                    });

                var items = await query.Where(x => dontFilter || x.PurchaseOrderNo.Contains(request.Search)
                                                              || x.VendorName.Contains(request.Search)).ToListAsync();
                
                return _mapper.Map<Response>(items);
            }
        }
    }
}
