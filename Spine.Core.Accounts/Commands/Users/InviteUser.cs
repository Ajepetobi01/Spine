using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Accounts.Commands.Users
{
    public static class InviteUser
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            //[Required(ErrorMessage = "Phone number is required")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email address is required")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Role is required")]
            public Guid RoleId { get; set; }
        }

        public class Response : BasicActionResult
        {
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

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IAuditLogHelper _auditHelper;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IMapper _mapper;
            private readonly IConfiguration _configuration;
            private readonly IEmailSender _emailSender;

            public Handler(SpineContext context, IMapper mapper, IConfiguration configuration, IAuditLogHelper auditHelper, IEmailSender emailSender, UserManager<ApplicationUser> userManager)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _userManager = userManager;
                _configuration = configuration;
                _emailSender = emailSender;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                if (await _userManager.FindByEmailAsync(request.Email) != null)
                {
                    return new Response("Email is already in use");
                }

                var user = _mapper.Map<ApplicationUser>(request);
                var role = await _dbContext.Roles.Where(x => x.Id == request.RoleId && !x.IsDeleted)
                                                                    .Select(x => x.Name).SingleOrDefaultAsync();

                if (role.IsNullOrEmpty())
                    return new Response("Role not found");
                
                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, role);

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.User,
                        Action = (int)AuditLogUserAction.Create,
                        Description = $"Added new user  {user.Id} with Email {user.Email} and role {role.GetFirstPart()}",
                        UserId = request.UserId
                    });

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _dbContext.AccountConfirmationTokens.Add(new AccountConfirmationToken
                {
                    Email = user.Email,
                    CreatedOn = Constants.GetCurrentDateTime(),
                    Token = code,
                    Id = user.Id
                });

                var webUrl = _configuration["SpineWeb"];
                var emailModel = new AcceptInvite
                {
                    ActionLink = Constants.GetAcceptInvitetLink(webUrl, code),
                    Name = user.FirstName,
                    BusinessName = "",
                    Date = Constants.GetCurrentDateTime().ToLongDateString()
                };

                var emailSent = await _emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - User Invite", EmailTemplateEnum.AcceptInvite, emailModel);
                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.Created)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
