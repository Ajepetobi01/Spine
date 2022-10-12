using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Accounts.Commands.Accounts
{
    public static class ConfirmAccount
    {
        public class Command : IRequest<Response>
        {
            [Required(ErrorMessage = "Confirmation code is required")]
            public string ConfirmationCode { get; set; }

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
            private readonly IEmailSender _emailSender;

            public Handler(UserManager<ApplicationUser> userManager, SpineContext dbContext, IEmailSender emailSender)
            {
                _userManager = userManager;
                _dbContext = dbContext;
                _emailSender = emailSender;
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

                var result = await _userManager.ConfirmEmailAsync(user, confirmData.Token);
                if (result.Succeeded)
                {
                    var company = await _dbContext.Companies.SingleAsync(x => x.Id == user.CompanyId);
                    company.IsVerified = true;
                    _dbContext.AccountConfirmationTokens.Remove(confirmData);
                    //await _dbContext.SaveChangesAsync();

                    if (await _dbContext.SaveChangesAsync() > 0)
                    {

                        var emailModel = new WelcomeMessage
                        {
                            UserName = company.Name,
                            Name = company.Name
                        };
                        var emailSent = _emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - Welcome to Spine", EmailTemplateEnum.WelcomeMessage, emailModel);

                        return new Response(HttpStatusCode.NoContent);
                    }
                    return new Response(HttpStatusCode.NoContent);
                }

                return new Response("An error occured. Please try again");
            }
        }
    }

}
