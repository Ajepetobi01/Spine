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
using Spine.Common.Models;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Inventories.Queries.Journal
{
    public static class GetJournals
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinDebitAmount { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxDebitAmount { get; set; }
            
            [Column(TypeName = "decimal(18,2")]
            public decimal? MinCreditAmount { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxCreditAmount { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.ProductName),
                nameof(Model.JournalDate),
                nameof(Model.JournalNo),
                nameof(Model.Debit),
                nameof(Model.Credit),
                nameof(Model.CreatedOn),
                nameof(Model.AccountName),
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
            [Sortable("ProductName")] public string ProductName { get; set; }

            public string Description { get; set; }

            [Sortable("JournalDate")] public DateTime JournalDate { get; set; }
            [Sortable("JournalNo")] public string JournalNo { get; set; }

            [Sortable("Debit")] public decimal Debit { get; set; }

            [Sortable("Credit")] public decimal Credit { get; set; }
            
            public string ItemDescription { get; set; }

            [Sortable("AccountName")] public string AccountName { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }
            
            public CurrencyModel Currency { get; set; }
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
                var query = from journal in _dbContext.JournalPostings.Where(x => x.CompanyId == request.CompanyId)
                    join account in _dbContext.LedgerAccounts on journal.LedgerAccountId equals account.Id
                    join cur in _dbContext.Currencies on journal.CurrencyId equals cur.Id
                            select new Model
                            {
                                Id = journal.Id,
                                ProductName = journal.ProductName,
                                Description = journal.Description,
                                JournalDate = journal.PostingDate,
                                JournalNo = journal.JournalNo,
                                Debit = journal.Debit / journal.RateToBaseCurrency,
                                CreatedOn = journal.CreatedOn,
                                AccountName = account.AccountName,
                                ItemDescription = journal.ItemDescription,
                                Credit = journal.Credit / journal.RateToBaseCurrency,
                                Currency = new CurrencyModel
                                {
                                    Code = cur.Code,
                                    Id = cur.Id,
                                    Symbol = cur.Symbol,
                                    Name = cur.Name
                                },
                            };

                if (request.StartDate.HasValue) query = query.Where(x => x.JournalDate >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.JournalDate.Date <= request.EndDate);
                
                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.ProductName.Contains(request.Search)
                                                                              || x.JournalNo.Contains(request.Search)
                                                                              || x.Description.Contains(request.Search)
                                                                              || x.ItemDescription.Contains(request.Search));

                if (request.MinDebitAmount != null) query = query.Where(x => x.Debit >= request.MinDebitAmount);
                if (request.MaxDebitAmount != null) query = query.Where(x => x.Debit <= request.MaxDebitAmount);

                if (request.MinCreditAmount != null) query = query.Where(x => x.Credit >= request.MinCreditAmount);
                if (request.MaxCreditAmount != null) query = query.Where(x => x.Credit <= request.MaxCreditAmount);
                
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
