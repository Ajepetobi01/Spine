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
    [Route("api/invoice-payment")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class InvoicePaymentController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvoicePaymentController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public InvoicePaymentController(IMediator mediator, ILogger<InvoicePaymentController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get invoice payments
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoicePayments.Response))]
        [ProducesResponseType(typeof(GetInvoicePayments.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewInvoicePayment)]
        public async Task<IActionResult> GetInvoicePayments([FromQuery] GetInvoicePayments.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get invoice payment by id
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [HttpGet("{paymentId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoicePayments.Response))]
        [ProducesResponseType(typeof(GetInvoicePayments.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewInvoicePayment)]
        public async Task<IActionResult> GetInvoicePayments([FromRoute] Guid paymentId)
        {
            var request = new GetInvoicePayment.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = paymentId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }


        /// <summary>
        /// add invoice payment manually
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{invoiceId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddInvoicePayment.Response))]
        [ProducesResponseType(typeof(AddInvoicePayment.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AddInvoicePayment)]
        public async Task<IActionResult> AddInvoicePayment([FromRoute] Guid invoiceId, [FromBody] AddInvoicePayment.Command request)
        {
            request.InvoiceId = invoiceId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

    }
}
