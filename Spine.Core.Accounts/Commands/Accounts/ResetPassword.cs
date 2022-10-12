using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Commands.Accounts
{
    public static class ResetPassword
    {
        public class Command : IRequest<Response>
        {
            [Required(ErrorMessage = "Reset code is required")]
            public string ResetCode { get; set; }

            [Required(ErrorMessage = "Password is required")]
            public string Password { get; set; }

            [Compare(nameof(Password), ErrorMessage = "Password and confirm password must match")]
            public string ConfirmPassword { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
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
                var resetData = await _dbContext.PasswordResetTokens.FirstOrDefaultAsync(x => x.Token == request.ResetCode);
                if (resetData == null)
                {
                    return new Response($"Reset Code is invalid or has expired. Reset your password from the login page");
                }

                var user = await _userManager.FindByEmailAsync(resetData.Email);
                if (user == null)
                {
                    return new Response($"An error occured");
                }

                var validators = _userManager.PasswordValidators;
                foreach (var validator in validators)
                {
                    var check = await validator.ValidateAsync(_userManager, user, request.Password);
                    if (!check.Succeeded) return new Response(check.Errors.FirstOrDefault()?.Description);
                }

                var result = await _userManager.ResetPasswordAsync(user, resetData.Token, request.Password);
                if (result.Succeeded)
                {
                    _dbContext.PasswordResetTokens.Remove(resetData);
                    await _dbContext.SaveChangesAsync();

                    return new Response();
                }

                return new Response("Unable to reset password. Try again");
            }
        }
    }

}
