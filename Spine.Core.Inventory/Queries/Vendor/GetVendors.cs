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
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Inventories.Queries.Vendor
{
    public static class GetVendors
    {
        public class Query : IRequest<Response>, IPagedRequest, ISortedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            [Column(TypeName = "decimal(18,2")]
            public decimal? MinPayables { get; set; }
            [Column(TypeName = "decimal(18,2")]
            public decimal? MaxPayables { get; set; }

            public Status? Status { get; set; }
            public TypeOfVendor? VendorType { get; set; }

            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;

            [StringRange(new[] {
               nameof(Model.Name),
               nameof(Model.Email),
               nameof(Model.BusinessName),
                nameof(Model.VendorType),
                nameof(Model.Payables),
                nameof(Model.Receivables),
                 nameof(Model.LastTransactionDate),
                nameof(Model.CreatedOn),
                nameof(Model.Status),
                nameof(Model.TotalPurchases)
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
            [Sortable("Name")] public string Name { get; set; }
            [Sortable("Email")] public string Email { get; set; }
            
            public string PhoneNo { get; set; }
            [Sortable("BusinessName")] public string BusinessName { get; set; }
            
            [Sortable("VendorType")] [JsonIgnore] public TypeOfVendor VendorTypeEnum { get; set; }
            public string VendorType { get; set; }

            [Sortable("LastTransactionDate")] public DateTime? LastTransactionDate { get; set; }

            [Sortable("Receivables")] public decimal Receivables { get; set; }
            [Sortable("Payables")] public decimal Payables { get; set; }

            [Sortable("TotalPurchases")] public decimal TotalPurchases { get; set; }

            [Sortable("Status")] [JsonIgnore] public Status StatusEnum { get; set; }
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
                var query = from inv in _dbContext.Vendors.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    select new Model
                    {
                        Id = inv.Id,
                        Name = inv.Name,
                        LastTransactionDate = inv.LastTransactionDate,
                        CreatedOn = inv.CreatedOn,
                        Payables = inv.AmountOwed,
                        Receivables = inv.AmountReceived,
                        TotalPurchases = inv.AmountOwed + inv.AmountReceived,
                        Status = inv.Status.GetDescription(),
                        StatusEnum = inv.Status,
                        VendorTypeEnum = inv.VendorType,
                        VendorType = inv.VendorType.GetDescription(),
                        Email = inv.Email,
                        BusinessName = inv.BusinessName,
                        PhoneNo = inv.PhoneNumber
                    };

                if (request.StartDate.HasValue) query = query.Where(x => x.CreatedOn >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.CreatedOn.Date <= request.EndDate);
                if (!request.Search.IsNullOrEmpty())
                    query = query.Where(x => x.Name.Contains(request.Search)
                                             || x.BusinessName.Contains(request.Search)
                                             || x.Email.Contains(request.Search));

                if (request.MinPayables != null) query = query.Where(x => x.Payables >= request.MinPayables);
                if (request.MaxPayables != null) query = query.Where(x => x.Payables <= request.MaxPayables);

                if (request.Status.HasValue) query = query.Where(x => x.StatusEnum == request.Status);
                if (request.VendorType.HasValue) query = query.Where(x => x.VendorTypeEnum == request.VendorType);

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
