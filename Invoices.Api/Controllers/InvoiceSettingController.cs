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
    [Route("api/invoice-setting")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class InvoiceSettingController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvoiceSettingController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public InvoiceSettingController(IMediator mediator, ILogger<InvoiceSettingController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// check if invoice settings is set
        /// </summary>
        /// <returns></returns>
        [HttpGet("check")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckInvoiceSettings()
        {
            var request = new CheckInvoiceSettings.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }


        /// <summary>
        /// add invoice settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddInvoiceSettings.Response))]
        [ProducesResponseType(typeof(AddInvoiceSettings.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> AddInvoiceSettings([FromBody] AddInvoiceSettings.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }


        /// <summary>
        /// get invoice setting
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoiceSettings.Response))]
        [ProducesResponseType(typeof(GetInvoiceSettings.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInvoiceSettings()
        {
            var request = new GetInvoiceSettings.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// update invoice preference
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("preference")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateInvoicePreference.Response))]
        [ProducesResponseType(typeof(UpdateInvoicePreference.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> UpdateInvoiceSettings([FromBody] UpdateInvoicePreference.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update invoice preference customization
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("customization")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateInvoiceCustomizationPreference.Response))]
        [ProducesResponseType(typeof(UpdateInvoiceCustomizationPreference.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> UpdateInvoicePReferenceCustomization([FromBody] UpdateInvoiceCustomizationPreference.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update invoice payment preference
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("payment")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateInvoicePaymentPreference.Response))]
        [ProducesResponseType(typeof(UpdateInvoicePaymentPreference.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> UpdateInvoicePaymentPreference([FromBody] UpdateInvoicePaymentPreference.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// add invoice payment integration settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("payment-integration")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddPaymentIntegration.Response))]
        [ProducesResponseType(typeof(AddPaymentIntegration.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> AddPaymentIntegration([FromBody] AddPaymentIntegration.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get invoice customizations
        /// </summary>
        /// <returns></returns>
        [HttpGet("customization")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoiceCustomizations.Response))]
        [ProducesResponseType(typeof(GetInvoiceCustomizations.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInvoiceCustomizations()
        {
            var request = new GetInvoiceCustomizations.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// add new invoice customization
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("customization")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddInvoiceCustomization.Response))]
        [ProducesResponseType(typeof(AddInvoiceCustomization.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> AddInvoiceCustomization([FromBody] AddInvoiceCustomization.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update invoice customization
        /// </summary>
        /// <param name="request"></param>
        /// <param name="customizationId"></param>
        /// <returns></returns>
        [HttpPut("customization/{customizationId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateInvoiceCustomization.Response))]
        [ProducesResponseType(typeof(UpdateInvoiceCustomization.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInvoiceSettings)]
        public async Task<IActionResult> UpdateInvoiceCustomization([FromBody] UpdateInvoiceCustomization.Command request, Guid customizationId)
        {
            request.Id = customizationId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
    }
}
