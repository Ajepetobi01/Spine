using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Extensions;
using Spine.Common.Models;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Accounts.Queries.Users
{
    public static class GetUsers
    {
        public class Query : IRequest<Response>, IPagedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public bool IsDeleted { get; set; }
            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public bool EmailConfirmed { get; set; }
            public string Role { get; set; }
            public string PhoneNumber { get; set; }
            public bool TwoFactor { get; set; }

            public DateTime DateCreated { get; set; }

        }

        public class Response : PagedResult<Model>
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
                var query = (from user in _dbContext.Users.Where(x =>
                        x.CompanyId == request.CompanyId && x.IsDeleted == request.IsDeleted)
                    join role in _dbContext.Roles on user.RoleId equals role.Id
                    select new Model
                    {
                        Id = user.Id,
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Name = user.FullName,
                        Role = role.Name,
                        PhoneNumber = user.PhoneNumber,
                        TwoFactor = user.TwoFactorEnabled,
                        DateCreated = user.CreatedOn
                    });

                query = query.OrderByDescending(x => x.DateCreated);
                Response data;
                if (request.Page == 0)
                    data = _mapper.Map<Response>(await query.ToListAsync());
                
                else
                    data = await query.ToPageResultsAsync<Model, Response>(request);

                foreach (var item in data.Items)
                {
                    item.Role = item.Role.GetFirstPart();
                }

                return data;

            }
        }

    }
}
