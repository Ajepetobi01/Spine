using ManageSubcription.Api.Authorizations;
using ManageSubcription.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using Spine.Data;
using Spine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/permission")]
    [ApiController]
    [Authorize]
    public class PermissionController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<PermissionController> _logger;
        private readonly IAuditLogHelper auditHelper;
        private readonly SpineContext context;

        public PermissionController(IManageSubcriptionRepository service, ILogger<PermissionController> logger, 
            IAuditLogHelper auditHelper, SpineContext context)
            : base(logger)
        {
            _service = service;
            _logger = logger;
            this.auditHelper = auditHelper;
            this.context = context;
        }

        /// <summary>
        /// get all permissions
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetPermissions()
        {
            try
            {
                var permissions = _service.GetPermissions();
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = permissions
                });

            }
            catch (Exception ex)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = ex.Message
                });
            }
        }

    }
}
