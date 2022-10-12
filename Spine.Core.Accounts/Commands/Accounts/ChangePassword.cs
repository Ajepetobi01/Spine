using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Commands.Accounts
{
    public static class ChangePassword
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [Required(ErrorMessage = "Old password is required")]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = "New Password is required")]
            // [MinLength(6)]
            [DifferentFrom(nameof(OldPassword), ErrorMessage = "Current password and new password cannot be the same")]
            public string NewPassword { get; set; }

            [Compare(nameof(NewPassword), ErrorMessage = "New password and confirm password must match")]
            public string ConfirmNewPassword { get; set; }

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
                var user = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (user == null)
                {
                    return new Response($"User not found");
                }

                var validators = _userManager.PasswordValidators;
                foreach (var validator in validators)
                {
                    var check = await validator.ValidateAsync(_userManager, user, request.NewPassword);
                    if (!check.Succeeded) return new Response(check.Errors.FirstOrDefault()?.Description);
                }

                if (await _userManager.CheckPasswordAsync(user, request.OldPassword))
                {
                    await _userManager.RemovePasswordAsync(user);
                    var result = await _userManager.AddPasswordAsync(user, request.NewPassword);
                    return result.Succeeded ? new Response(HttpStatusCode.NoContent) : new Response("Could not change password. Try again");
                }

                return new Response("Current password is invalid");

            }
        }
    }

}
