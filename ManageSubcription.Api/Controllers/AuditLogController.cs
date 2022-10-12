using AutoMapper;
using ManageSubcription.Api.Helpers;
using ManageSubcription.Api.Model;
using ManageSubcription.Api.Services;
using ManageSubcription.Api.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/auditlog")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class AuditLogController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<AuditLogController> _logger;
        private readonly IMapper mapper;
        private readonly IUriService uriService;

        public AuditLogController(IManageSubcriptionRepository service, ILogger<AuditLogController> logger, IMapper mapper
            , IUriService uriService) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.mapper = mapper;
            this.uriService = uriService;
        }

        /// <summary>
        /// get all activities log
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paginationQuery"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AuditLogViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetLogs([FromQuery] QueryAuditLog query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<FilterAuditLog>(query);
            var posts = _service.GetAuditLog(filter, pagination).ToList();
            var totalRecords = _service.GetAuditLog(filter).Count();
            var postsReponse = mapper.Map<List<AuditLogViewModel>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<AuditLogViewModel>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
    }
}
