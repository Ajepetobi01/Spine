using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Spine.Core.BillsPayments.Queries;
using Spine.Core.BillsPayments.Commands;
using Microsoft.AspNetCore.Http;
using BillsPayments.Api.Filters;
using BillsPayments.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Common.Models;

namespace BillsPayments.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class BillsPaymentsController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BillsPaymentsController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public BillsPaymentsController(IMediator mediator, ILogger<BillsPaymentsController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("all-categories")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCategories.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> Categories()
        {
            var result = await _mediator.Send(new GetCategories.Query());
            return CommandResponse(result);
        }

        /// <summary>
        /// get categories used by spine
        /// </summary>
        /// <returns></returns>
        [HttpGet("categories")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetSpineCategories.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> GetSpineCategories()
        {
            var result = await _mediator.Send(new GetSpineCategories.Query());
            return QueryResponse(result);
        }

        /// <summary>
        /// get billers by category id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("categories/{categoryId}/billers")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetBillersByCategoryId.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> Billers([FromRoute] string categoryId)
        {
            var result = await _mediator.Send(new GetBillersByCategoryId.Query { Id = categoryId });
            return CommandResponse(result);
        }

        /// <summary>
        /// get payment items by biller
        /// </summary>
        /// <param name="billerId"></param>
        /// <returns></returns>
        [HttpGet("billers/{billerId}/items")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetPaymentItemsByBillerId.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> PaymentItems([FromRoute] int billerId)
        {
            var result = await _mediator.Send(new GetPaymentItemsByBillerId.Query { BillerId = billerId });
            return CommandResponse(result);
        }

        /// <summary>
        /// get all bills payments
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetBillPayments.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> GetBillPayments([FromQuery] GetBillPayments.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// validate customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("validate-customer")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ValidateCustomer.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> ValidateCustomer([FromBody] ValidateCustomer.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }


        /// <summary>
        ///send payment advice
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("send-payment-advice")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(SendPaymentAdvice.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> MakeBillPayment([FromBody] SendPaymentAdvice.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///verify transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("verify-transaction")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(VerifyTransaction.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.BillsPayment)]
        public async Task<IActionResult> VerifyTransaction([FromBody] VerifyTransaction.Request request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }


    }
}
