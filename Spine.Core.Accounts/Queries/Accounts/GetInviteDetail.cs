using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Accounts.Queries.Accounts
{
    public static class GetInviteDetail
    {
        public class Query : IRequest<Response>
        {
            [Required(ErrorMessage = "Confirmation code is required")]
            public string ConfirmationCode { get; set; }
        }

        public class Response
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public Guid RoleId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var confirmData = await _dbContext.AccountConfirmationTokens.Where(x => x.Token == request.ConfirmationCode)
                    .Join(_dbContext.Users, token => token.Email, user => user.Email, (token, user) =>
                                    new { token.Email, user.RoleId, user.FirstName, user.LastName, user.FullName })
                    .FirstOrDefaultAsync();

                if (confirmData == null)
                {
                    return null;
                }

                return new Response
                {
                    FirstName = confirmData.FirstName,
                    LastName = confirmData.LastName,
                    FullName = confirmData.FullName,
                    Email = confirmData.Email,
                    RoleId = confirmData.RoleId
                };

            }
        }
    }
}
