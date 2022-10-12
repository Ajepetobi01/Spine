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
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Customers.Queries
{
    public static class GetCustomers
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            // public bool OnlyMine { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public string SearchBy { get; set; }

            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinPurchases { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxPurchases { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinAmountOwed { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxAmountOwed { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MinAmountReceived { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxAmountReceived { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
                nameof(Model.OnboardingDate),
                nameof(Model.Name),
                nameof(Model.Email),
                nameof(Model.LastTransactionDate),
                nameof(Model.TotalPurchases)
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
            [Sortable("Email")]
            public string Email { get; set; }
            [Sortable("Name")]
            public string Name { get; set; }
            public string DateCreated { get; set; }
            public string PhoneNumber { get; set; }
            public decimal TotalReceived { get; set; }
            public decimal AmountOwed { get; set; }
            [Sortable("LastTransactionDate")]
            public DateTime? LastTransactionDate { get; set; }
            [Sortable("TotalPurchases")]
            public decimal TotalPurchases { get; set; }

            [JsonIgnore]
            public Guid CreatedBy { get; set; }

            [Sortable("OnboardingDate", IsDefault = true)]
            public DateTime OnboardingDate { get; set; }
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

                var query = from customer in _dbContext.Customers.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                                                                             && request.StartDate <= x.CreatedOn && x.CreatedOn <= request.EndDate.GetValueOrDefault().ToEndOfDay())
                            select new Model
                            {
                                Id = customer.Id,
                                Email = customer.Email,
                                Name = customer.Name,
                                DateCreated = customer.CreatedOn.ToLongDateString(),
                                PhoneNumber = customer.PhoneNumber,
                                AmountOwed = customer.AmountOwed,
                                TotalReceived = customer.AmountReceived,
                                LastTransactionDate = customer.LastTransactionDate,
                                TotalPurchases = customer.TotalPurchases,
                                CreatedBy = customer.CreatedBy,
                                OnboardingDate = customer.CreatedOn
                            };

                //   if (request.OnlyMine) query = query.Where(x => x.CreatedBy == request.UserId);

                if (!request.Name.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.Name));
                if (!request.Email.IsNullOrEmpty()) query = query.Where(x => x.Email.Contains(request.Email));
                if (!request.Phone.IsNullOrEmpty()) query = query.Where(x => x.PhoneNumber.Contains(request.Phone));
                if (request.MinAmountOwed != null) query = query.Where(x => x.AmountOwed >= request.MinAmountOwed);
                if (request.MaxAmountOwed != null) query = query.Where(x => x.AmountOwed <= request.MaxAmountOwed);
                if (request.MinAmountReceived != null) query = query.Where(x => x.TotalReceived >= request.MinAmountReceived);
                if (request.MaxAmountOwed != null) query = query.Where(x => x.TotalReceived <= request.MaxAmountReceived);
                if (request.MinPurchases != null) query = query.Where(x => x.TotalPurchases >= request.MinPurchases);
                if (request.MaxPurchases != null) query = query.Where(x => x.TotalPurchases <= request.MaxPurchases);

                if (!request.SearchBy.IsNullOrEmpty())
                    query = query.Where(x => x.Name.Contains(request.SearchBy) || x.Email.Contains(request.SearchBy) || x.PhoneNumber.Contains(request.SearchBy));

                query = request.SortBy.IsNullOrEmpty() 
                    ? query.OrderByDescending(x => x.OnboardingDate) 
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
