using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using Spine.Core.Accounts.Queries.Accounts;
using System.Threading.Tasks;
using Spine.Core.Accounts.Commands.Accounts;
using Microsoft.AspNetCore.Http;
using Accounts.Api.Filters;
using Spine.Common.Extensions;
using Spine.Common.Models;
using Spine.Core.Accounts.Commands.Users;

namespace Accounts.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AccountController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccountController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public AccountController(IMediator mediator, ILogger<AccountController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// login 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(Login.Response))]
        [ProducesResponseType(typeof(Login.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] Login.Query request)
        {
            request.IpAddress = IpAddress();
            var result = await _mediator.Send(request);

            if (result.RefreshToken.IsNullOrEmpty()) return CommandResponse(result);
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            return CommandResponse(result);
        }

        /// <summary>
        /// login with OTP
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("otp-login")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(Login2FA.Response))]
        [ProducesResponseType(typeof(Login2FA.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login2FA([FromBody] Login2FA.Query request)
        {
            request.IpAddress = IpAddress();
            var result = await _mediator.Send(request);
            if (result.RefreshToken.IsNullOrEmpty()) return CommandResponse(result);
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            return CommandResponse(result);
        }

        /// <summary>
        /// get account detail after login - (permissions, etc) 
        /// </summary>
        /// <returns></returns>
        [HttpGet("details")]
        [Authorize]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetLoginDetail.Response))]
        [ProducesResponseType(typeof(GetLoginDetail.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AccountDetails()
        {
            var request = new GetLoginDetail.Query
            {
                UserId = UserId.GetValueOrDefault(),
                CompanyId = CompanyId.GetValueOrDefault()
            };
            
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// sign up 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("signup")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Signup.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Signup([FromBody] Signup.Command request)
        {
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///forgot password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ForgotPassword.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword.Query request)
        {
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///reset password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ForgotPassword.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword.Command request)
        {
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///confirm account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("confirm-account")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConfirmAccount.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccount.Command request)
        {
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///accept invite
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("accept-invite")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInviteDetail.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptInvite([FromQuery] GetInviteDetail.Query request)
        {
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        ///accept invite
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("accept-invite")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AcceptUserInvite.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptUserInvite.Command request)
        {
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///toggle 2FA status
        /// </summary>
        /// <returns></returns>
        [HttpPut("update-2fa")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [Authorize]
        [ProducesResponseType(typeof(Toggle2FA.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Toggle2FA()
        {
            var request = new Toggle2FA.Command
            {
                UserId = UserId.GetValueOrDefault(),
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        /// <summary>
        ///change password for logged in users
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("change-password")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [Authorize]
        [ProducesResponseType(typeof(ChangePassword.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///confirm account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("verify-email")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetEmail.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckEmail([FromQuery] GetEmail.Command request)
        {
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        #region Refresh Tokens

        /// <summary>
        /// refresh login token 
        /// </summary>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(RefreshLoginToken.Response))]
        [ProducesResponseType(typeof(RefreshLoginToken.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshLoginToken.Command request)
        {
            request ??= new RefreshLoginToken.Command();
            request.RefreshToken = Request.Cookies["refreshToken"];
            request.IpAddress = IpAddress();

            var result = await _mediator.Send(request);

            if (result.RefreshToken.IsNullOrEmpty()) return CommandResponse(result);
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            return CommandResponse(result);
        }

        /// <summary>
        /// revoke refresh token (logout)
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        [Authorize]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(RevokeRefreshToken.Response))]
        [ProducesResponseType(typeof(RevokeRefreshToken.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RevokeRefreshToken.Command request)
        {
            request ??= new RevokeRefreshToken.Command();
            request.RefreshToken = Request.Cookies["refreshToken"];
            request.IpAddress = IpAddress();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        #endregion
    }
}
