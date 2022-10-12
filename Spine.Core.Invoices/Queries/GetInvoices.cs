using System;
using System.Collections.Generic;
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
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoices
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid? CustomerId { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinAmount { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxAmount { get; set; }

            public string CustomerName { get; set; }
            public string InvoiceNo { get; set; }
            public string Subject { get; set; }

            public bool OnlyMine { get; set; }

            //public string ItemName { get; set; }
            //public string ItemDescription { get; set; }
            public PaymentStatusFilter? Status { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
                nameof(Model.Recipient),
                nameof(Model.InvoiceNo),
                nameof(Model.Amount),
                nameof(Model.DueDate),
                nameof(Model.InvoiceDate),
                nameof(Model.BalanceDue),
                nameof(Model.CreatedOn),
              //  nameof(Model.Status),
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
            public string Email { get; set; }
            [Sortable("Recipient")]
            public string Recipient { get; set; }
            public string Subject { get; set; }

            [Sortable("InvoiceNo")]
            public string InvoiceNo { get; set; }

            [Sortable("Amount")]
            public decimal Amount { get; set; }

            [Sortable("BalanceDue")]
            public decimal BalanceDue { get; set; }

            [Sortable("InvoiceDate")]
            public DateTime InvoiceDate { get; set; }

            [Sortable("DueDate")]
            public DateTime? DueDate { get; set; }

            // public string PaymentSource { get; set; }

            //   [Sortable("Status")]
            public string Status { get; set; }

            public CurrencyModel Currency { get; set; }
            public List<Guid> Documents { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }

            [JsonIgnore]
            public Guid CustomerId { get; set; }
            [JsonIgnore]
            public Guid CreatedBy { get; set; }
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

                var query = from invoice in _dbContext.Invoices.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                                                               && request.StartDate <= x.InvoiceDate && x.InvoiceDate <= request.EndDate.GetValueOrDefault().ToEndOfDay())
                            join cur in _dbContext.Currencies on invoice.CurrencyId equals cur.Id
                            select new Model
                            {
                                Id = invoice.Id,
                                CreatedBy = invoice.CreatedBy,
                                CreatedOn = invoice.CreatedOn,
                                CustomerId = invoice.CustomerId,
                                Email = invoice.CustomerEmail,
                                Recipient = invoice.CustomerName,
                                Subject = invoice.Subject,
                                InvoiceDate = invoice.InvoiceDate,
                                DueDate = invoice.DueDate != DateTime.MinValue ? invoice.DueDate : null,

                                Amount = invoice.InvoiceTotalAmount / invoice.RateToBaseCurrency,
                                BalanceDue = invoice.InvoiceBalance / invoice.RateToBaseCurrency,
                                InvoiceNo = invoice.InvoiceNoString,
                                Currency = new CurrencyModel
                                {
                                    Id = cur.Id,
                                    Code = cur.Code,
                                    Name = cur.Name,
                                    Symbol = cur.Symbol
                                }
                            };

                var today = Constants.GetCurrentDateTime().Date;
                if (request.OnlyMine) query = query.Where(x => x.CreatedBy == request.UserId);
                if (request.MinAmount != null) query = query.Where(x => x.Amount >= request.MinAmount);
                if (request.MaxAmount != null) query = query.Where(x => x.Amount <= request.MaxAmount);
                if (request.CustomerId.HasValue) query = query.Where(x => x.CustomerId == request.CustomerId);
                if (!request.InvoiceNo.IsNullOrEmpty()) query = query.Where(x => x.InvoiceNo.Contains(request.InvoiceNo));
                if (!request.CustomerName.IsNullOrEmpty()) query = query.Where(x => x.Recipient.Contains(request.CustomerName));
                if (!request.Subject.IsNullOrEmpty()) query = query.Where(x => x.Subject.Contains(request.Subject));
                if (request.Status != null)
                {
                    switch (request.Status)
                    {
                        case PaymentStatusFilter.Paid:
                            query = query.Where(x => x.BalanceDue == 0);
                            break;
                        case PaymentStatusFilter.Due:
                            query = query.Where(x => x.BalanceDue > 0 && x.DueDate < today);
                            break;
                        case PaymentStatusFilter.NotDue:
                            query = query.Where(x => x.BalanceDue > 0 && (!x.DueDate.HasValue || x.DueDate > today));
                            break;
                        default:
                            break;
                    }
                }

                if (request.SortBy.IsNullOrEmpty()) query = query.OrderByDescending(x => x.CreatedOn);
                else query = query.OrderBy(request.SortByAndOrder);
                Response data;
                if (request.Page == 0)
                    data = _mapper.Map<Response>(await query.ToListAsync());
                
                else
                    data = await query.ToPageResultsAsync<Model, Response>(request);

                var invoices = data.Items; // await query.ToListAsync();
                var itemIds = invoices.Select(x => x.Id).ToList();

                //var lineItems = await _dbContext.LineItems.Where(x => x.CompanyId == request.CompanyId && itemIds.Contains(x.ParentItemId)).ToListAsync();
                //var lineItemLookup = lineItems.ToLookup(x => x.ParentItemId);

                var docs = await _dbContext.Documents.Where(x => x.CompanyId == request.CompanyId && itemIds.Contains(x.ParentItemId))
                    .Select(x => new { x.ParentItemId, x.DocumentId }).ToListAsync();
                var docsLookup = docs.ToLookup(d => d.ParentItemId, f => f.DocumentId);

                foreach (var item in invoices)
                {
                    item.Documents = docsLookup[item.Id].ToList();
                    if (item.BalanceDue == 0)
                        item.Status = "Completed";

                    else
                    {
                        if (item.DueDate.HasValue && item.DueDate.Value.Date < today)
                        {
                            var dateDiff = (today - item.DueDate.Value.Date).Duration().Days;
                            item.Status = $"Overdue by {dateDiff} day(s)";
                        }
                        else
                        {
                            item.Status = "Not due";
                        }
                    }

                }
                return data;

            }
        }

    }
}
