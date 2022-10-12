using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Services;
using Spine.Services.ViewModels;
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
    public class NotificationsController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NotificationsController> _logger;
        private readonly INotificationService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger, INotificationService service) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// get all notifications for logged in user
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(List<NotificationModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNotifications()
        {
            var data = await _service.GetNotifications(CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault());

            return Ok(data);
        }

        /// <summary>
        /// mark single notification as read
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        [HttpPut("{notificationId}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid notificationId)
        {
            await _service.MarkAsRead(CompanyId.GetValueOrDefault(), notificationId, UserId.GetValueOrDefault());

            return Ok();
        }

        /// <summary>
        /// clear notifications
        /// </summary>
        /// <param name="notificationIds"></param>
        /// <returns></returns>
        [HttpPut("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Clear([FromBody] List<Guid> notificationIds)
        {
            await _service.Clear(CompanyId.GetValueOrDefault(), notificationIds, UserId.GetValueOrDefault());

            return Ok();
        }
    }
}
