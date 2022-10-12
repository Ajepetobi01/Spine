using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Accounts.Queries.Accounts
{
    public static class ForgotPassword
    {
        public class Query : IRequest<Response>
        {
            public string Email { get; set; }
        }

        public class Response : BasicActionResult
        {
            //   public string Message { get; set; }
            public Response()
            {
                Status = HttpStatusCode.OK;
            }

            public Response(HttpStatusCode statusCode)
            {
                Status = statusCode;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SpineContext _dbContext;
            private readonly IConfiguration _configuration;
            private readonly IEmailSender _emailSender;

            public Handler(UserManager<ApplicationUser> userManager, IConfiguration configuration, SpineContext dbContext, IEmailSender emailSender)
            {
                _userManager = userManager;
                _dbContext = dbContext;
                _configuration = configuration;
                _emailSender = emailSender;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new Response("No account with this email address");
                }

                string code = await _userManager.GeneratePasswordResetTokenAsync(user);

                _dbContext.PasswordResetTokens.Add(new PasswordResetToken
                {
                    Email = user.Email,
                    Token = code,
                    Id = SequentialGuid.Create()
                });

                var webUrl = _configuration["SpineWeb"];
                var emailModel = new ResetPassword
                {
                    ActionLink = Constants.GetResetPasswordLink(webUrl, code),
                    Name = user.FullName,
                };

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    var emailSent = await _emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - Reset Password", EmailTemplateEnum.ResetPassword, emailModel);

                    return new Response();
                }

                return new Response(HttpStatusCode.BadRequest);

            }
        }
    }

}
