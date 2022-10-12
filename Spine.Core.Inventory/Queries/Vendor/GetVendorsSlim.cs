using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Vendor
{
    public static class GetVendorsSlim
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            //  [Required]
            //  [MinLength(3)]
            public string Search { get; set; }
        }

        public class Response : List<Model>
        {
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string BusinessName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
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
                var dontFilter = request.Search.IsNullOrEmpty();

                var items = await _dbContext.Vendors
                    .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                                                 && x.Status == Status.Active
                                                                 && (dontFilter || x.Name.Contains(request.Search)
                                                                     || x.Email.Contains(request.Search)
                                                                     || x.DisplayName.Contains(request.Search) ||
                                                                     x.BusinessName.Contains(request.Search)))
                    .Select(x => new Model
                    {
                        Id = x.Id, Name = x.Name, BusinessName = x.BusinessName, Email = x.Email,
                        Phone = x.PhoneNumber
                    })
                    .ToListAsync();

                return _mapper.Map<Response>(items);
            }
        }
    }
}
