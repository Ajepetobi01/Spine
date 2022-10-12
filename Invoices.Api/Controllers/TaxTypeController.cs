using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spine.Core.Invoices.Queries;
using Invoices.Api.Filters;
using Invoices.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Core.Invoices.Commands;
using Spine.Common.Models;

namespace Invoices.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class TaxTypeController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TaxTypeController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public TaxTypeController(IMediator mediator, ILogger<TaxTypeController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get all taxtypes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetTaxTypes.Response))]
        [ProducesResponseType(typeof(GetTaxTypes.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ValidateModel]
        public async Task<IActionResult> GetTaxTypes([FromQuery] GetTaxTypes.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get tax type by id
        /// </summary>
        /// <param name="taxId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{taxId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetTaxType.Response))]
        [ProducesResponseType(typeof(GetTaxType.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ValidateModel]
        public async Task<IActionResult> GetTaxType([FromRoute] Guid taxId, [FromQuery] GetTaxType.Query request)
        {
            request.Id = taxId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }


        /// <summary>
        /// add tax type 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddTaxType.Response))]
        [ProducesResponseType(typeof(AddTaxType.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> AddTaxType([FromBody] AddTaxType.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update tax types
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateTaxType.Response))]
        [ProducesResponseType(typeof(UpdateTaxType.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> UpdateTaxType([FromBody] UpdateTaxType.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete tax type 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteTaxType.Response))]
        [ProducesResponseType(typeof(DeleteTaxType.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> DeleteInvoice([FromRoute] Guid id)
        {
            var request = new DeleteTaxType.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

    }
}
