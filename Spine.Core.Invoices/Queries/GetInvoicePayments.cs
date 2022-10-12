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

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoicePayments
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            public Guid? CustomerId { get; set; }

            public Guid? InvoiceId { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }


            public string InvoiceNo { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
                nameof(Model.Recipient),
                nameof(Model.InvoiceNo),
                nameof(Model.Amount),
             //   nameof(Model.PaymentSource),
                nameof(Model.PaymentDate),
                nameof(Model.BalanceDue),
                nameof(Model.CreatedOn),
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "asc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid InvoiceId { get; set; }
            [Sortable("Recipient")]
            public string Recipient { get; set; }
            [Sortable("InvoiceNo")]
            public string InvoiceNo { get; set; }
            [Sortable("Amount")]
            public decimal Amount { get; set; }
            [Sortable("BalanceDue")]
            public decimal BalanceDue { get; set; }

            [Sortable("PaymentDate")]
            public DateTime PaymentDate { get; set; }

            //[Sortable("PaymentSource")]
            public string PaymentSource { get; set; }

            [JsonIgnore]
            public Guid CustomerId { get; set; }
            [JsonIgnore]
            public Guid CreatedBy { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }
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
                if (request.StartDate == null) request.StartDate = DateTime.MinValue;
                if (request.EndDate == null) request.EndDate = DateTime.MaxValue;

                var query = from payment in _dbContext.InvoicePayments.Where(x => x.CompanyId == request.CompanyId
                                                                               && request.StartDate <= x.PaymentDate
                                                                               && x.PaymentDate <= request.EndDate.GetValueOrDefault().ToEndOfDay())
                            join invoice in _dbContext.Invoices on payment.InvoiceId equals invoice.Id
                            where invoice.CompanyId == request.CompanyId && !invoice.IsDeleted
                            select new Model
                            {
                                CreatedBy = payment.CreatedBy,
                                PaymentSource = payment.PaymentSource.GetDescription(),
                                CreatedOn = payment.CreatedOn,
                                PaymentDate = payment.PaymentDate,
                                Amount = payment.AmountPaid,
                                Id = payment.Id,

                                BalanceDue = invoice.InvoiceBalance,
                                CustomerId = invoice.CustomerId,
                                Recipient = invoice.CustomerName,
                                InvoiceId = invoice.Id,
                                InvoiceNo = invoice.InvoiceNoString,
                            };

                if (request.InvoiceId.HasValue) query = query.Where(x => x.InvoiceId == request.InvoiceId);
                if (request.CustomerId.HasValue) query = query.Where(x => x.CustomerId == request.CustomerId);
                if (!request.InvoiceNo.IsNullOrEmpty()) query = query.Where(x => x.InvoiceNo.Contains(request.InvoiceNo));

                if (request.SortBy.IsNullOrEmpty()) query = query.OrderByDescending(x => x.CreatedOn);
                else query = query.OrderBy(request.SortByAndOrder);

                if (request.Page == 0)
                {
                    return _mapper.Map<Response>(await query.ToListAsync());
                }
                
                return await query.ToPageResultsAsync<Model, Response>(request);
            }
        }

    }
}
