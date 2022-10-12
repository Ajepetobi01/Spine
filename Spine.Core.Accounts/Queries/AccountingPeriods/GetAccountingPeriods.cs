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

namespace Spine.Core.Accounts.Queries.AccountingPeriods
{
    public static class GetAccountingPeriods
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            
            public string Search { get; set; }
            
            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
                nameof(Model.PeriodCode),
                nameof(Model.StartDate),
                nameof(Model.EndDate),
                nameof(Model.IsClosed),
                nameof(Model.BookClosedDate)
            })]
            public string SortBy { get; set; }

            [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
            public string Order { get; set; } = "desc";

            [JsonIgnore]
            public string SortByAndOrder => this.FindSortingAndOrder<Model>();
        }

        public class Model
        {
            public int Id { get; set; }
            [Sortable("PeriodCode")] public string PeriodCode { get; set; }
            [Sortable("StartDate", IsDefault = true)]public DateTime StartDate { get; set; }
            [Sortable("EndDate")]public DateTime EndDate { get; set; }
            [Sortable("BookClosedDate")]public DateTime? BookClosedDate { get; set; }
            [Sortable("IsClosed")] public bool IsClosed { get; set; }

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
                var query = from period in _dbContext.AccountingPeriods.Where(x =>
                        x.CompanyId == request.CompanyId)
                    select new Model
                    {
                        Id = period.Id,
                        PeriodCode = period.PeriodCode,
                        StartDate = period.StartDate,
                        EndDate = period.EndDate,
                        IsClosed = period.IsClosed,
                        BookClosedDate = period.BookClosedDate
                    };

                if (!request.Search.IsNullOrEmpty())
                {
                    query = query.Where(x => x.PeriodCode.Contains(request.Search));
                }
                
                query = request.SortBy.IsNullOrEmpty() 
                    ? query.OrderByDescending(x => x.StartDate) 
                    : query.OrderBy(request.SortByAndOrder);

                if (request.Page == 0)
                {
                    return _mapper.Map<Response>(await query.ToListAsync());
                }
                
                return await query.ToPageResultsAsync<Model, Response>(request);
                
            }
        }

    }
}
