using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Users
{
    public static class GetUserProfile
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid Id { get; set; }

            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public Guid RoleId { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public string PhoneNumber { get; set; }
            public bool TwoFactor { get; set; }

        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query message, CancellationToken token)
            {
                var data = await (from user in _dbContext.Users.Where(x => x.CompanyId == message.CompanyId && x.Id == message.Id && !x.IsDeleted)
                                  join role in _dbContext.Roles on user.RoleId equals role.Id
                                  select new Response
                                  {
                                      Id = user.Id,
                                      Email = user.Email,
                                      FirstName = user.FirstName,
                                      LastName = user.LastName,
                                      Name = user.FullName,
                                      RoleId = role.Id,
                                      Role = role.Name,
                                      PhoneNumber = user.PhoneNumber,
                                      TwoFactor = user.TwoFactorEnabled
                                  }).SingleOrDefaultAsync();

                if (data != null)
                    data.Role = data.Role.GetFirstPart();

                return data;

            }
        }

    }
}
