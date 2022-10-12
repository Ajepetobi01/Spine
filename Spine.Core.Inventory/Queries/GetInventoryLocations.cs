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
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Inventories.Queries
{
    public static class GetInventoryLocations
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }

            public string Name { get; set; }
            public string State { get; set; }
            public Status? Status { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Name),
               nameof(Model.State),
                nameof(Model.PhoneNumber),
                nameof(Model.CreatedOn),
                nameof(Model.Status)
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
            [Sortable("Name")]
            public string Name { get; set; }
            [Sortable("State")]
            public string State { get; set; }

            public string Address { get; set; }
            [Sortable("PhoneNumber")]
            public string PhoneNumber { get; set; }

            [Sortable("Status")]
            public Status StatusEnum { get; set; }
            public string Status { get; set; }

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
                var query = from loc in _dbContext.InventoryLocations.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                            select new Model
                            {
                                Id = loc.Id,
                                Name = loc.Name,
                                CreatedOn = loc.CreatedOn,
                                Address = loc.Address,
                                Status = loc.Status.GetDescription(),
                                StatusEnum = loc.Status,
                                State = loc.State,
                                PhoneNumber = loc.PhoneNumber
                            };


                if (!request.Search.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.Search)
                                                                            || x.Address.Contains(request.Search));

                if (!request.Name.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.Name));
                if (!request.State.IsNullOrEmpty()) query = query.Where(x => x.State.Contains(request.State));
                if (request.Status.HasValue) query = query.Where(x => x.StatusEnum == request.Status.Value);
                if (request.StartDate.HasValue) query = query.Where(x => x.CreatedOn >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.CreatedOn.Date <= request.EndDate);

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
