using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Transactions.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Core.Transactions.Queries.Reports;
using Transactions.Api.Filters;

namespace Transactions.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class ReportController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReportController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public ReportController(IMediator mediator, ILogger<ReportController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get general ledger entries
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("ledger-entries")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AllLedgerEntries.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactionReport)]
        [ValidateModel]
        public async Task<IActionResult> GetGeneralLedgerEntries([FromQuery] AllLedgerEntries.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get trial balance
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("trial-balance")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(TrialBalance.Response), StatusCodes.Status200OK)]
        [HasPermission(Permissions.ViewTransactionReport)]
        [ValidateModel]
        public async Task<IActionResult> TrialBalance([FromQuery] TrialBalance.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get trial balance detailed
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("trial-balance/detailed")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(TrialBalanceDetailed.Response), StatusCodes.Status200OK)]
        [HasPermission(Permissions.ViewTransactionReport)]
        [ValidateModel]
        public async Task<IActionResult> TrialBalanceDetailed([FromQuery] TrialBalanceDetailed.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get profit and loss
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("profit-and-loss")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ProfitAndLoss.Response), StatusCodes.Status200OK)]
        [HasPermission(Permissions.ViewTransactionReport)]
        [ValidateModel]
        public async Task<IActionResult> ProfitAndLoss([FromQuery] ProfitAndLoss.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        // /// <summary>
        // /// get details of the subclass for profit and loss report
        // /// </summary>
        // /// <param name="request"></param>
        // /// <returns></returns>
        // [HttpGet("profit-and-loss/detailed")]
        // [Consumes(MediaTypeNames.Application.Json)]
        // [ProducesResponseType(typeof(ProfitAndLossDetailed.Response), StatusCodes.Status200OK)]
        // [HasPermission(Permissions.ViewTransactionReport)]
        // [ValidateModel]
        // public async Task<IActionResult> ProfitAndLossDetailed([FromQuery] ProfitAndLossDetailed.Query request)
        // {
        //     request.CompanyId = CompanyId.GetValueOrDefault();
        //
        //     var result = await _mediator.Send(request);
        //     return QueryResponse(result);
        // }

        /// <summary>
        /// get statement of financial position
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("financial-position")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(FinancialPosition.Response), StatusCodes.Status200OK)]
        [HasPermission(Permissions.ViewTransactionReport)]
        [ValidateModel]
        public async Task<IActionResult> FinancialPosition([FromQuery] FinancialPosition.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        
        /// <summary>
        /// get transaction summary
        /// </summary>
        /// <param name="request"></param>
        /// <param name="bankAccountId"></param>
        /// <returns></returns>
        [HttpGet("{bankAccountId}/summary")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetSummary.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactionReport)]
        [ValidateModel]
        public async Task<IActionResult> GetSummary([FromRoute] Guid bankAccountId, [FromQuery] GetSummary.Query request)
        {
            request.BankAccountId = bankAccountId;
            request.CompanyId = CompanyId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
    }
}
