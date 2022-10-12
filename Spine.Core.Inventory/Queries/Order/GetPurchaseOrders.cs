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
    public static class GetPurchaseOrders
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid? VendorId { get; set; }
            
            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinAmount { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxAmount { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.VendorName),
                nameof(Model.OrderDate),
                nameof(Model.ExpectedDate),
                nameof(Model.OrderValue),
                   nameof(Model.OrderStatus),
                nameof(Model.CreatedOn),
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
            [Sortable("VendorName")] public string VendorName { get; set; }

            public string VendorEmail { get; set; }

            [Sortable("OrderDate")] public DateTime OrderDate { get; set; }
            [Sortable("ExpectedDate")] public DateTime? ExpectedDate { get; set; }

            [Sortable("OrderValue")] public decimal OrderValue { get; set; }

            [Sortable("OrderStatus")] public PurchaseOrderStatus OrderStatusEnum { get; set; }
            public string OrderStatus { get; set; }

            [Sortable("PurchaseOrderNo")] public string PurchaseOrderNo { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }

            public Guid? VendorId { get; set; }
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
                var query = from order in _dbContext.PurchaseOrders.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                            select new Model
                            {
                                Id = order.Id,
                                VendorId = order.VendorId,
                                VendorName = order.VendorName,
                                VendorEmail = order.VendorEmail,
                                OrderDate = order.OrderDate,
                                ExpectedDate = order.ExpectedDate,
                                OrderValue = order.OrderAmount,
                                CreatedOn = order.CreatedOn,
                                PurchaseOrderNo = order.OrderNo,
                                OrderStatus = order.Status.GetDescription(),
                                OrderStatusEnum = order.Status
                            };

                if (request.VendorId.HasValue) query = query.Where(x => x.VendorId == request.VendorId);
                if (request.StartDate.HasValue) query = query.Where(x => x.OrderDate >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.OrderDate.Date <= request.EndDate);
                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.VendorName.Contains(request.Search) || x.VendorEmail.Contains(request.Search));

                if (request.MinAmount != null) query = query.Where(x => x.OrderValue >= request.MinAmount);
                if (request.MaxAmount != null) query = query.Where(x => x.OrderValue <= request.MaxAmount);

                query = query.OrderBy(request.SortByAndOrder);
                
                if (request.Page == 0)
                {
                    return _mapper.Map<Response>(await query.ToListAsync());
                }
                return await query.ToPageResultsAsync<Model, Response>(request);
            }
        }
    }
}
