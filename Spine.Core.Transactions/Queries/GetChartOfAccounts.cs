using System;
using System.Collections.Generic;
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
    public static class GetChartOfAccounts
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }
            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
                nameof(Model.AccountName),
                nameof(Model.AccountNo),
                nameof(Model.AccountType),
                nameof(Model.AccountClass),
                nameof(Model.AccountSubClass)
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

            [Sortable("AccountName", IsDefault = true)]
            public string AccountName { get; set; }

            [Sortable("AccountNo")] public string AccountNo { get; set; }
            [Sortable("AccountType")] public string AccountType { get; set; }
            [Sortable("AccountClass")] public string AccountClass { get; set; }
            [Sortable("AccountSubClass")] public string AccountSubClass { get; set; }
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
                var query = from acct in _dbContext.LedgerAccounts.Where(x =>
                        x.CompanyId == request.CompanyId && !x.IsDeleted)
                    join type in _dbContext.AccountTypes on acct.AccountTypeId equals type.Id
                    join subClass in _dbContext.AccountSubClasses on type.AccountSubClassId equals subClass.Id
                    join clas in _dbContext.AccountClasses on subClass.AccountClassId equals clas.Id
                    select new Model
                    {
                        Id = acct.Id,
                        AccountName = acct.AccountName,
                        AccountNo = acct.GLAccountNo,
                        AccountClass = clas.Class,
                        AccountSubClass = subClass.SubClass,
                        AccountType = type.Name
                    };

                if (!request.Search.IsNullOrWhiteSpace())
                    query = query.Where(x => x.AccountName.Contains(request.Search) ||
                                             x.AccountClass.Contains(request.Search)
                                             || x.AccountSubClass.Contains(request.Search)
                                             || x.AccountNo.Contains(request.Search) ||
                                             x.AccountType.Contains(request.Search));
                
                query = query.OrderBy(request.SortByAndOrder);

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
