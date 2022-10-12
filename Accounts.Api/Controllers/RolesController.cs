using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Spine.Core.Accounts.Queries.Roles;
using Microsoft.AspNetCore.Http;
using Accounts.Api.Filters;
using Accounts.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Core.Accounts.Commands.Users;
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
    public class RolesController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RolesController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public RolesController(IMediator mediator, ILogger<RolesController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get all roles 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetRoles.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        //[HasPermission(Permissions.ViewRole)]
        public async Task<IActionResult> Roles([FromQuery] GetRoles.Query request)
        {
            if (request == null) request = new GetRoles.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetRole.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        //[HasPermission(Permissions.ViewRole)]
        public async Task<IActionResult> Role([FromRoute] Guid roleId)
        {
            var request = new GetRole.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = roleId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get all users (team) in role
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{roleId}/users")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetUsersInRole.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewUsers)]
        public async Task<IActionResult> UsersInRole([FromRoute] Guid roleId, [FromQuery] GetUsersInRole.Query request)
        {
            if (request == null) request = new GetUsersInRole.Query();

            request.RoleId = roleId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// add user to role, will remove the user's previous role
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{roleId}/users")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateUserRole.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateUserRole)]
        public async Task<IActionResult> AddUserToRole([FromRoute] Guid roleId, [FromBody] UpdateUserRole.Command request)
        {
            request.RoleId = roleId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UpdatedBy = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }


        /// <summary>
        /// get role permissions
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}/permissions")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetRolePermissions.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        //[HasPermission(Permissions.ViewRole)]
        public async Task<IActionResult> RolePermissions([FromRoute] Guid roleId)
        {
            var request = new GetRolePermissions.Query
            {
                RoleId = roleId,
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        ///// <summary>
        ///// add role
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost("")]
        //[ValidateModel]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(AddRole.Response), StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        //[HasPermission(Permissions.AddRole)]
        //public async Task<IActionResult> AddRole([FromBody] AddRole.Command request)
        //{
        //    request.CompanyId = CompanyId.GetValueOrDefault();
        //    request.UserId = UserId.GetValueOrDefault();
        //    var result = await _mediator.Send(request);
        //    return CommandResponse(result);
        //}

        ///// <summary>
        ///// update role permission
        ///// </summary>
        ///// <param name="roleId"></param>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPut("{roleId}/permissions")]
        //[ValidateModel]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(UpdateRolePermission.Response), StatusCodes.Status204NoContent)]
        //[ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        //[HasPermission(Permissions.UpdateRolePermission)]
        //public async Task<IActionResult> UpdateRolePermissions([FromRoute] Guid roleId, [FromBody] UpdateRolePermission.Command request)
        //{
        //    request.RoleId = roleId;
        //    request.CompanyId = CompanyId.GetValueOrDefault();
        //    request.UserId = UserId.GetValueOrDefault();
        //    var result = await _mediator.Send(request);
        //    return CommandResponse(result);
        //}

    }
}
