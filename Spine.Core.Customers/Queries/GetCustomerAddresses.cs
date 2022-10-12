using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Customers.Queries
{
    public static class GetCustomerAddresses
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid CustomerId { get; set; }

            public bool PrimaryOnly { get; set; }
        }

        public class Response : List<Model>
        {
        }

        public class Model
        {
            public Guid Id { get; set; }
            public bool IsPrimary { get; set; }
            public bool IsBilling { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
            public string State { get; set; }
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
                var addresses = await (from cust in _dbContext.CustomerAddresses
                                       where cust.CompanyId == request.CompanyId && cust.CustomerId == request.CustomerId && !cust.IsDeleted
                                       select new Model
                                       {
                                           Id = cust.Id,
                                           IsBilling = cust.IsBilling,
                                           IsPrimary = cust.IsPrimary,
                                           State = cust.State,
                                           AddressLine1 = cust.AddressLine1,
                                           AddressLine2 = cust.AddressLine2,
                                           PostalCode = cust.PostalCode,
                                           Country = cust.Country
                                       }).ToListAsync();

                if (request.PrimaryOnly) addresses = addresses.Where(x => x.IsPrimary).ToList();

                return _mapper.Map<Response>(addresses);
            }
        }

    }
}
