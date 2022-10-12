using System;
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
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Inventories.Queries.Vendor
{
    public static class GetVendorPayments
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore] public Guid CompanyId { get; set; }

            public Guid? VendorId { get; set; }
            public Guid? GoodsReceivedId { get; set; }
            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            // [Column(TypeName = "decimal(18,2")]
            // public decimal? MinAmount { get; set; }
            // [Column(TypeName = "decimal(18,2")]
            // public decimal? MaxAmount { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[]
            {
                nameof(Model.AccountName),
                nameof(Model.PaymentDate),
                nameof(Model.GoodsReceivedNo),
                nameof(Model.AmountPaid),
                nameof(Model.Outstanding),
                nameof(Model.Item),
                nameof(Model.CreatedOn)
            })]
            public string SortBy { get; set; }

            [StringRange(new[] {"asc", "ascending", "desc", "descending"})]
            public string Order { get; set; } = "desc";

            [JsonIgnore] public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            [Sortable("AccountName")] public string AccountName { get; set; }

            [Sortable("PaymentDate")] public DateTime PaymentDate { get; set; }
            [Sortable("GoodsReceivedNo")] public string GoodsReceivedNo { get; set; }

            [Sortable("AmountPaid")] public decimal AmountPaid { get; set; }
            [Sortable("Outstanding")] public decimal Outstanding { get; set; }

            public string PaymentMode { get; set; }
            [Sortable("Item")] public string Item { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }
            
            [JsonIgnore] public Guid? VendorId { get; set; }
            [JsonIgnore] public Guid? GoodsReceivedId { get; set; }
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
                var query = from paym in _dbContext.VendorPayments.Where(x => x.CompanyId == request.CompanyId)
                    join bank in _dbContext.BankAccounts on paym.BankAccountId equals bank.Id
                    join goodItem in _dbContext.ReceivedGoodsLineItems on paym.ReceivedGoodItemId equals goodItem.Id
                    join good in _dbContext.ReceivedGoods on goodItem.GoodReceivedId equals good.Id
                            select new Model
                            {
                                AccountName = bank.AccountName + " - " + bank.AccountNumber,
                                PaymentDate = paym.PaymentDate,
                                PaymentMode = paym.PaymentSource.GetDescription(),
                                AmountPaid = paym.AmountPaid,
                                Outstanding = paym.RemainingBalance.Value,
                                Item = goodItem.Item,
                                GoodsReceivedNo = good.GoodReceivedNo,
                                CreatedOn = good.CreatedOn,
                                VendorId = good.VendorId,
                                GoodsReceivedId = paym.ReceivedGoodId
                            };

                if (request.VendorId.HasValue) query = query.Where(x => x.VendorId == request.VendorId);
                if (request.GoodsReceivedId.HasValue) query = query.Where(x => x.GoodsReceivedId == request.GoodsReceivedId);
                
                if (request.StartDate.HasValue) query = query.Where(x => x.PaymentDate >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.PaymentDate.Date <= request.EndDate);
                
                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => 
                    x.AccountName.Contains(request.Search) || x.GoodsReceivedNo.Contains(request.Search));

                // if (request.MinAmount != null) query = query.Where(x => x.AmountPaid >= request.MinAmount);
                // if (request.MaxAmount != null) query = query.Where(x => x.AmountPaid <= request.MaxAmount);

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
