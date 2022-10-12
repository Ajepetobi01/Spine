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

namespace Spine.Core.Transactions.Queries
{
    public static class GetTransactions
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid? AccountId { get; set; }

            [JsonIgnore]
            public Guid UserId { get; set; }
            public bool OnlyMine { get; set; }
            
            public bool? IsInflow { get; set; }

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

            public Guid? CategoryId { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Category),
                nameof(Model.TransactionDate),
                nameof(Model.Amount),
                nameof(Model.Debit),
                nameof(Model.Credit),
                   nameof(Model.Payee),
                nameof(Model.ChequeNo),
                nameof(Model.CreatedOn)
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
            public Guid TransactionGroupId { get; set; }
            [Sortable("Category")]
            public string Category { get; set; }
            [Sortable("Payee")]
            public string Payee { get; set; }
            [Sortable("ChequeNo")]
            public string ChequeNo { get; set; }
            [Sortable("TransactionDate", IsDefault = true)]
            public DateTime TransactionDate { get; set; }
            public string Description { get; set; }
            public string ReferenceNo { get; set; }
            public string UserReferenceNo { get; set; }
            [Sortable("Amount")]
            public decimal Amount { get; set; }
            [Sortable("Debit")]
            public decimal Debit { get; set; }
            [Sortable("Credit")]
            public decimal Credit { get; set; }

            [Sortable("CreatedOn", IsDefault = true)]
            public DateTime CreatedOn { get; set; }
            
            //       public string Source { get; set; }
            //       public string Type { get; set; }

            public List<Guid> Documents { get; set; }

            [JsonIgnore]
            public Guid CreatedBy { get; set; }
            [JsonIgnore]
            public Guid? BankAccountId { get; set; }
            [JsonIgnore]
            public Guid? CategoryId { get; set; }
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
                var query = from trans in _dbContext.Transactions.Where(x =>
                        x.CompanyId == request.CompanyId && !x.IsDeleted)
                    join cat in _dbContext.TransactionCategories on trans.CategoryId equals cat.Id into transCat
                    from cat in transCat.DefaultIfEmpty()
                    where cat == null || !cat.IsDeleted
                    select new Model
                    {
                        Id = trans.Id,
                        CategoryId = trans.CategoryId,
                        Category = cat.Name ?? "Uncategorized",
                        BankAccountId = trans.BankAccountId,
                        Payee = trans.Payee,
                        ChequeNo = trans.ChequeNo,
                        TransactionGroupId = trans.TransactionGroupId,
                        TransactionDate = trans.TransactionDate,
                        UserReferenceNo = trans.UserReferenceNo,
                        ReferenceNo = trans.ReferenceNo,
                        Description = trans.Description,
                        Amount = trans.Amount,
                        Debit = trans.Debit,
                        Credit = trans.Credit,
                        CreatedBy = trans.CreatedBy,
                        CreatedOn = trans.CreatedOn
                        //     Source = trans.Source.GetDescription(),
                        //      Type = trans.Type.GetDescription()
                    };

                if (request.IsInflow.HasValue)
                {
                    query = request.IsInflow.Value
                        ? query.Where(x => x.Credit > 0)
                        : query.Where(x => x.Debit > 0);
                }

                if (request.OnlyMine) query = query.Where(x => x.CreatedBy == request.UserId);
                if (request.AccountId.HasValue) query = query.Where(x => x.BankAccountId == request.AccountId);


                if (request.StartDate.HasValue) query = query.Where(x => x.TransactionDate >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.TransactionDate.Date <= request.EndDate);

                if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId);
                if (request.MinDebitAmount != null) query = query.Where(x => x.Debit >= request.MinDebitAmount);
                if (request.MaxDebitAmount != null) query = query.Where(x => x.Debit <= request.MaxDebitAmount);

                if (request.MinCreditAmount != null) query = query.Where(x => x.Credit >= request.MinCreditAmount);
                if (request.MaxCreditAmount != null) query = query.Where(x => x.Credit <= request.MaxCreditAmount);

                if (!request.Search.IsNullOrWhiteSpace())
                    query = query.Where(x => x.ReferenceNo.Contains(request.Search) ||
                                             x.UserReferenceNo.Contains(request.Search)
                                             || x.Payee.Contains(request.Search)
                                             || x.ChequeNo.Contains(request.Search) ||
                                             x.Description.Contains(request.Search) ||
                                             x.Category.Contains(request.Search));

                query = request.SortBy.IsNullOrEmpty() ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(request.SortByAndOrder);

                Response items;
                if (request.Page == 0)
                    items = _mapper.Map<Response>(await query.ToListAsync());
                
                else
                    items = await query.ToPageResultsAsync<Model, Response>(request);

               // var itemIds = items.Items.Select(x => x.Id).ToList();

                //var docs = await _dbContext.Documents.Where(x => x.CompanyId == request.CompanyId && itemIds.Contains(x.ParentItemId))
                //  .Select(x => new { x.ParentItemId, x.DocumentId }).ToListAsync();
                //var docsLookup = docs.ToLookup(d => d.ParentItemId, f => f.DocumentId);

                //foreach (var item in items.Items)
                //{
                //    item.Documents = docsLookup[item.Id].ToList();
                //}

                return items;

            }
        }

    }
}
