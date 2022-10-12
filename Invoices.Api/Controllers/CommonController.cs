using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spine.Core.Invoices.Queries;
using Spine.Common.Extensions;
using Spine.Common.Enums;

namespace Invoices.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api")]
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
        /// get invoice types 
        /// </summary>
        /// <returns></returns>
        [HttpGet("invoice-types")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInvoiceTypes.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InvoiceTypes()
        {
            var result = await _mediator.Send(new GetInvoiceTypes.Query());
            return QueryResponse(result);
        }

        /// <summary>
        /// invoice payment status filter
        /// </summary>
        /// <returns></returns>
        [HttpGet("payment-status-filter")]
        public IActionResult PaymentStatusFilter()
        {
            return Ok(EnumExtensions.GetValues<PaymentStatusFilter>());
        }

        /// <summary>
        /// get currencies  
        /// </summary>
        /// <returns></returns>
        [HttpGet("currency")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetCurrencies.Response))]
        [ProducesResponseType(typeof(GetCurrencies.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Currencies()
        {
            var result = await _mediator.Send(new GetCurrencies.Query());
            return QueryResponse(result);
        }

        /// <summary>
        /// invoice frequency
        /// </summary>
        /// <returns></returns>
        [HttpGet("invoice-frequency")]
        public IActionResult InvoiceFrequency()
        {
            return Ok(EnumExtensions.GetValues<InvoiceFrequency>());
        }

        /// <summary>
        /// discount type
        /// </summary>
        /// <returns></returns>
        [HttpGet("discount-type")]
        public IActionResult DiscountType()
        {
            return Ok(EnumExtensions.GetValues<DiscountType>());
        }

        /// <summary>
        /// enums for invoice preferences
        /// </summary>
        /// <returns></returns>
        [HttpGet("invoice-preference")]
        public IActionResult InvoicePreferenceEnum()
        {
            var result = new
            {
                Discount = EnumExtensions.GetValues<DiscountSettings>(),
                Tax = EnumExtensions.GetValues<TaxSettings>(),
                ApplyTax = EnumExtensions.GetValues<ApplyTaxSettings>(),
                PaymentIntegrationProvider = EnumExtensions.GetValues<PaymentIntegrationProvider>(),
                PaymentIntegrationType = EnumExtensions.GetValues<PaymentIntegrationType>()
            };

            return Ok(result);
        }

        /// <summary>
        /// payment mode
        /// </summary>
        /// <returns></returns>
        [HttpGet("payment-mode")]
        public IActionResult PaymentMode()
        {
            return Ok(EnumExtensions.GetValues<PaymentMode>());
        }


        /// <summary>
        /// invoice color themes for customization
        /// </summary>
        /// <returns></returns>
        [HttpGet("customization-themes")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoiceColorThemes.Response))]
        [ProducesResponseType(typeof(GetInvoiceColorThemes.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CustomizationThemes()
        {
            var result = await _mediator.Send(new GetInvoiceColorThemes.Query());
            return QueryResponse(result);
        }
    }
}
