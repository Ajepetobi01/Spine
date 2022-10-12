using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;
using Spine.Common.Extensions;
using AutoMapper;

namespace Spine.Core.Customers.Queries
{
    public static class GetCustomersByName
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public string Search { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public string PhoneNo { get; set; }
        }

        public class Response : List<Model>
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
                var data = await (from customer in _dbContext.Customers.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                                                                             && (request.Search.IsNullOrEmpty()
                                                                                                             || x.Name.Contains(request.Search)
                                                                                                              || x.BusinessName.Contains(request.Search)
                                                                                                             || x.Email.Contains(request.Search)))
                                  select new Model
                                  {
                                      Id = customer.Id,
                                      Email = customer.Email,
                                      Name = customer.Name,
                                      PhoneNo = customer.PhoneNumber,
                                  }).ToListAsync();

                return _mapper.Map<Response>(data);

            }
        }

    }
}
