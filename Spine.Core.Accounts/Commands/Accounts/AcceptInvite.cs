using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Commands.Accounts
{
    public static class AcceptUserInvite
    {
        public class Command : IRequest<Response>
        {
            [Required(ErrorMessage = "Confirmation code is required")]
            public string ConfirmationCode { get; set; }

            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            public string PhoneNumber { get; set; }

            // [MinLength(6, ErrorMessage = "Password cannot be less than 6 characters")]
            [Required(ErrorMessage = "Password is required")]
            public string Password { get; set; }

            public bool EnableTwoFactorAuth { get; set; }

            //[Compare(nameof(Password), ErrorMessage = "Password and confirm password must match")]
            //public string ConfirmPassword { get; set; }

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
                var confirmData = await _dbContext.AccountConfirmationTokens.FirstOrDefaultAsync(x => x.Token == request.ConfirmationCode);
                if (confirmData == null)
                {
                    return new Response($"Confirmation Code is invalid or has expired.");
                }

                var user = await _userManager.FindByEmailAsync(confirmData.Email);
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

                user.FirstName = request.FirstName.ToTitleCase();
                user.LastName = request.LastName.ToTitleCase();
                user.FullName = $"{user.FirstName} {user.LastName}";
                user.TwoFactorEnabled = request.EnableTwoFactorAuth;
                user.PhoneNumber = request.PhoneNumber;

                var result = await _userManager.ConfirmEmailAsync(user, confirmData.Token);
                if (result.Succeeded)
                {
                    _dbContext.AccountConfirmationTokens.Remove(confirmData);
                    await _dbContext.SaveChangesAsync();

                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, request.Password);

                    return new Response(HttpStatusCode.NoContent);
                }

                return new Response("An error occured. Please try again");
            }
        }
    }

}
