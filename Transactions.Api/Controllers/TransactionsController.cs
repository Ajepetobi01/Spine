using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Spine.Core.Transactions.Queries;
using Spine.Core.Transactions.Commands;
using Microsoft.AspNetCore.Http;
using Transactions.Api.Filters;
using Transactions.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Common.Models;
using System.Collections.Generic;

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
    public class TransactionsController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionsController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Category

        /// <summary>
        /// get transaction categories
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("categories")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransactionCategories.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Categories([FromQuery] GetTransactionCategories.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get transaction category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("categories/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransactionCategory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            var request = new GetTransactionCategory.Query
            {
                Id = id,
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// create transaction category
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("category")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(CreateTransactionCategory.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.CreateTransactionCategory)]
        public async Task<IActionResult> CreateTransactionCategory([FromBody] CreateTransactionCategory.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update transaction category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("category/{id}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateTransactionCategory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateTransactionCategory)]
        public async Task<IActionResult> UpdateTransactionCategory(Guid id, UpdateTransactionCategory.Command request)
        {
            request.Id = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete transaction category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("category/{id}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeleteTransactionCategory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.DeleteTransactionCategory)]
        public async Task<IActionResult> DeleteTransactionCategory(Guid id)
        {
            var request = new DeleteTransactionCategory.Command
            {
                Id = id,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        #endregion

        /// <summary>
        /// get transaction dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("dashboard")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransactionDashboard.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> TransactionDashboard([FromQuery] GetTransactionDashboard.Query request)
        {
            if (request == null) request = new GetTransactionDashboard.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get transaction dashboard (for mobile)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("mobile-dashboard")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransactionMobileDashboard.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> GetTransactionMobileDashboard([FromQuery] GetTransactionMobileDashboard.Query request)
        {
            if (request == null) request = new GetTransactionMobileDashboard.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get transactions
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransactions.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> AllTransactions([FromQuery] GetTransactions.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get transactions by accountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("account/{accountId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransactions.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> GetAccountTransactions([FromRoute] Guid accountId, [FromQuery] GetTransactions.Query request)
        {
            request.AccountId = accountId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get transaction summary  by bank account Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpGet("{accountId}/summary")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransactionSummary.Model), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> TransactionSummary([FromRoute] Guid accountId)
        {
            var request = new GetTransactionSummary.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                AccountId = accountId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get transaction by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetTransaction.Model), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> Transaction([FromRoute] Guid id)
        {
            var request = new GetTransaction.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// upload bank transactions (from excel preview)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpPost("{accountId}/upload")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ImportBankTransaction.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ImportBankTransactions)]
        public async Task<IActionResult> ImportBankTransactions([FromRoute] Guid accountId, [FromBody] List<ImportBankTransaction.ImportTransactionModel> model)
        {
            var request = new ImportBankTransaction.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Transactions = model,
                UserId = UserId.GetValueOrDefault(),
                BankAccountId = accountId
            };

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// upload bank transactions (gotten from mono)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpPost("{accountId}/mono-upload")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ImportBankTransactionFromMono.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ImportBankTransactions)]
        public async Task<IActionResult> ImportBankTransactionFromMono([FromRoute] Guid accountId, [FromBody] List<ImportBankTransactionFromMono.ImportTransactionModel> model)
        {
            var request = new ImportBankTransactionFromMono.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Transactions = model,
                UserId = UserId.GetValueOrDefault(),
                BankAccountId = accountId
            };

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }


        /// <summary>
        /// add manual  transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("manual")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddManualTransaction.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.CreateTransactions)]
        public async Task<IActionResult> AddManualTransaction([FromBody] AddManualTransaction.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update  transaction
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateTransaction.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateTransactions)]
        public async Task<IActionResult> UpdateTransaction([FromRoute] Guid id, [FromBody] UpdateTransaction.Command request)
        {
            request.Id = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// set transaction reminder
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{transactionId}/add-reminder")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddTransactionReminder.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddTransactionReminder([FromRoute] Guid transactionId, [FromBody] AddTransactionReminder.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = transactionId;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// extend transaction reminder
        /// </summary>
        /// <param name="reminderId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{reminderId}/extend-reminder")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ExtendTransactionReminder.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExtendTransactionReminder([FromRoute] Guid reminderId, [FromBody] ExtendTransactionReminder.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = reminderId;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get ledger accounts (charts of account)
        /// </summary>
        /// <returns></returns>
        [HttpGet("charts-of-accounts")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetChartOfAccounts.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> GetChartOfAccounts([FromQuery] GetChartOfAccounts.Query request)
        {
            request ??= new GetChartOfAccounts.Query();
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get ledger accounts (slim for dropdown)
        /// </summary>
        /// <returns></returns>
        [HttpGet("ledger-accounts")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetLedgerAccounts.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewTransactions)]
        public async Task<IActionResult> GetLedgerAccounts()
        {
            var request = new GetLedgerAccounts.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

    }
}
