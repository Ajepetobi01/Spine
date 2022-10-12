using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Core.Accounts.Queries.Common;
using Spine.Core.Accounts.Commands;
using Spine.Common.Models;

namespace Accounts.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class CommonController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CommonController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public CommonController(IMediator mediator, ILogger<CommonController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get business types 
        /// </summary>
        /// <returns></returns>
        [HttpGet("business-types")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetBusinessTypes.Response))]
        [ProducesResponseType(typeof(GetBusinessTypes.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BusinessTypes()
        {
            var result = await _mediator.Send(new GetBusinessTypes.Query());
            return QueryResponse(result);
        }

        /// <summary>
        /// get operating  sectors 
        /// </summary>
        /// <returns></returns>
        [HttpGet("operating-sectors")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetOperatingSectors.Response))]
        [ProducesResponseType(typeof(GetOperatingSectors.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> OperatingSectors()
        {
            var result = await _mediator.Send(new GetOperatingSectors.Query());
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get months
        /// </summary>
        /// <returns></returns>
        [HttpGet("months")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult Months()
        {
            return Ok(EnumExtensions.GetValues<Month>());
        }
        
        /// <summary>
        /// get accounting  methods 
        /// </summary>
        /// <returns></returns>
        [HttpGet("accounting-methods")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult AccountingMethods()
        {
            return Ok(EnumExtensions.GetValues<AccountingMethod>());
        }

        /// <summary>
        /// get all permissions
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("permissions")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetAllPermissions.Response))]
        [ProducesResponseType(typeof(GetAllPermissions.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AllPermissions([FromQuery] GetAllPermissions.Query request)
        {
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// update newly added things (permissions etc) (not used by frontend)
        /// </summary>
        /// <returns></returns>
        [HttpPut("new-things")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateNewlyAddedThings.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateNewlyAddedThings()
        {
            var request = new UpdateNewlyAddedThings.Command();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

    }
}
