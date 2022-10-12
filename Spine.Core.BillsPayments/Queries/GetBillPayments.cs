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

namespace Spine.Core.BillsPayments.Queries
{
    public static class GetBillPayments
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid UserId { get; set; }

            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Date),
                nameof(Model.Amount),
                nameof(Model.Status),
                nameof(Model.PaymentItem),
                   nameof(Model.Description)
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "desc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            [Sortable("PaymentItem")]
            public string PaymentItem { get; set; }
            public string TransactionReference { get; set; }
            [Sortable("Amount")]
            public decimal Amount { get; set; }
            [Sortable("Status")]
            public string Status { get; set; }
            [Sortable("Description")]
            public string Description { get; set; }
            [Sortable("Date", IsDefault = true)]
            public DateTime Date { get; set; }
        }

        public class Response : Common.Models.PagedResult<Model>
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
                var query = from ben in _dbContext.BillPayments.Where(x => x.CompanyId == request.CompanyId)
                            select new Model
                            {
                                PaymentItem = ben.PaymentItemName,
                                Amount = ben.AmountToPay,
                                Status = ben.TransactionStatus,
                                TransactionReference = ben.TransactionReference,
                                Description = "",
                                Date = ben.DateCreated
                            };

                if (request.StartDate.HasValue) query = query.Where(x => x.Date >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.Date <= request.EndDate);
                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.TransactionReference.Contains(request.Search)
                                                                                                                        || x.Description.Contains(request.Search)
                                                                                                                          || x.Status.Contains(request.Search)
                                                                                                                        || x.PaymentItem.Contains(request.Search));

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
