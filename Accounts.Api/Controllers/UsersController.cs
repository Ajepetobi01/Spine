using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Spine.Core.Accounts.Queries.Users;
using Spine.Core.Accounts.Commands.Users;
using Microsoft.AspNetCore.Http;
using Accounts.Api.Filters;
using Accounts.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Common.Models;

namespace Accounts.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class UsersController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public UsersController(IMediator mediator, ILogger<UsersController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get all users (team) 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetUsers.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewUsers)]
        public async Task<IActionResult> AllUsers([FromQuery] GetUsers.Query request)
        {
            if (request == null) request = new GetUsers.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get user profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("my-profile")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetUserProfile.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserProfile()
        {
            var request = new GetUserProfile.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// invite user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("invite")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(InviteUser.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.InviteUsers)]
        public async Task<IActionResult> InviteUser([FromBody] InviteUser.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }


        /// <summary>
        /// get user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetUserProfile.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewUsers)]
        public async Task<IActionResult> GetUserById([FromRoute] Guid userId)
        {
            var request = new GetUserProfile.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = userId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// update user information
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{userId}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateUser.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateUser)]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] UpdateUser.Command request)
        {
            request.Id = userId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeleteUser.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.DeleteUsers)]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
        {
            var request = new DeleteUser.Command
            {
                Id = userId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        /// <summary>
        /// restore deleted user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut("restore/{userId}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeleteUser.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateUser)]
        public async Task<IActionResult> RestoreDeletedUser([FromRoute] Guid userId)
        {
            var request = new RestoreDeletedUser.Command
            {
                Id = userId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

    }
}
