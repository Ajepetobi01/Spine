using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Queries.Accounts
{
    public static class GetEmail
    {
        public class Command : IRequest<Response>
        {
            [Required(ErrorMessage = "Email is required")]
            public string Email { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response(HttpStatusCode code)
            {
                Status = code;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
            public Response(string message, HttpStatusCode statusCode)
            {
                Message = message;
                Status = statusCode;
            }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SpineContext _dbContext;

            public Handler(UserManager<ApplicationUser> userManager, SpineContext dbContext)
            {
                _userManager = userManager;
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var confirmEail = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
                if (confirmEail != null)
                {
                    return new Response(confirmEail.FirstName, HttpStatusCode.OK);
                }
                else
                {
                    return new Response(HttpStatusCode.NotFound);
                }
            }
        }
    }
}
