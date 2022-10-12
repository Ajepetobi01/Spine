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
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Transactions.Queries.Reports
{
    public static class AllLedgerEntries
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            
            public List<Guid> AccountIds { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            
            public string Search { get; set; }
            
            [Column(TypeName = "decimal(18,2")]
            public decimal? MinAmount { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxAmount { get; set; }
            
            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
                nameof(Model.ValueDate),
                nameof(Model.CreatedOn),
                nameof(Model.Credit),
                nameof(Model.Debit),
                nameof(Model.ReferenceNo),
                
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "asc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            [JsonIgnore] public Guid LedgerAccountId { get; set; }
            
            public Guid Id { get; set; }
            public Guid TransactionGroupId { get; set; }
            
            public string Account { get; set; }
        
            public string AccountingPeriod { get; set; }
        
            [Sortable("ValueDate", IsDefault = true)]
            public DateTime ValueDate { get; set; }
            public string Narration { get; set; }
           
            [Sortable("ReferenceNo")] public string ReferenceNo { get; set; }

            public string Customer { get; set; }
            public string Vendor { get; set; }

            [Sortable("Credit")] public decimal Credit { get; set; }
            [Sortable("Debit")] public decimal Debit { get; set; }
            public decimal Amount { get; set; }
       
            public string TransactionType { get; set; }

            public string CurrencyCode { get; set; }
            public string CurrencySymbol { get; set; }
        
            [Sortable("CreatedOn")] public DateTime CreatedOn { get; set; }
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
                var query = from trans in _dbContext.GeneralLedgers.Where(x =>
                        x.CompanyId == request.CompanyId)
                    join period in _dbContext.AccountingPeriods on trans.AccountingPeriodId equals period.Id
                    join ledgerAccount in _dbContext.LedgerAccounts on trans.LedgerAccountId equals ledgerAccount.Id
                    join curr in _dbContext.Currencies on trans.BaseCurrencyId equals curr.Id
                    join cust in _dbContext.Customers.Where(x => x.CompanyId == request.CompanyId)
                        on trans.CustomerId equals cust.Id into transCust
                    from cust in transCust.DefaultIfEmpty()
                    join vend in _dbContext.Vendors.Where(x => x.CompanyId == request.CompanyId)
                        on trans.VendorId equals vend.Id into transVend
                    from vend in transVend.DefaultIfEmpty()

                    select new Model
                    {
                        Id = trans.Id,
                        Account = ledgerAccount.AccountName,
                        LedgerAccountId = trans.LedgerAccountId,
                        AccountingPeriod =
                            period.StartDate.ToShortDateString() + " - " + period.EndDate.ToShortDateString(),
                        CurrencyCode = curr.Code,
                        CurrencySymbol = curr.Symbol,
                        TransactionType = trans.Type.GetDescription(),
                        Customer = cust.Name,
                        Vendor = vend.Name,
                        TransactionGroupId = trans.TransactionGroupId,
                        ReferenceNo = trans.ReferenceNo,
                        Narration = trans.Narration,
                        ValueDate = trans.ValueDate,
                        Credit = trans.CreditAmount,
                        Debit = trans.DebitAmount,
                        Amount = trans.DebitAmount - trans.CreditAmount,
                        CreatedOn = trans.TransactionDate,
                        CreatedBy = trans.CreatedBy,
                    };

                if (request.StartDate.HasValue) query = query.Where(x => x.ValueDate >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.ValueDate.Date <= request.EndDate);

                //filter out unselected accounts
                if (!request.AccountIds.IsNullOrEmpty())
                {
                    query = query.Where(x => request.AccountIds.Contains(x.LedgerAccountId));
                }

                if (!request.Search.IsNullOrEmpty())
                {
                    query = query.Where(x => x.Account.Contains(request.Search)
                                            // || x.Customer.Contains(request.Search) || x.Vendor.Contains(request.Search)
                                             || x.ReferenceNo.Contains(request.Search) ||
                                             x.Narration.Contains(request.Search));
                }

                if (request.MinAmount is > 0)
                    query = query.Where(x =>
                        (x.Credit > 0 && x.Credit >= request.MinAmount) || (x.Debit > 0 && x.Debit >= request.MinAmount));
                if (request.MaxAmount is > 0)
                    query = query.Where(x =>
                        (x.Credit > 0 && x.Credit >= request.MaxAmount) || (x.Debit > 0 && x.Debit >= request.MaxAmount));

                query = request.SortBy.IsNullOrEmpty()
                    ? query.OrderByDescending(x => x.ValueDate)
                    : query.OrderBy(request.SortByAndOrder);

                Response items;
                if (request.Page == 0)
                    items = _mapper.Map<Response>(await query.ToListAsync());
                else
                    items = await query.ToPageResultsAsync<Model, Response>(request);

                return items;
            }
        }
    }
}
