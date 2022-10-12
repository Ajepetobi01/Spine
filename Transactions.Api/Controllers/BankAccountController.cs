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
using Spine.Services;
using System.Collections.Generic;
using System.IO;
using Spine.Common.ActionResults;
using System.Data;
using System.Text.Json;
using Spine.Common.Models;

namespace Transactions.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/bank-account")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class BankAcountController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BankAcountController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public BankAcountController(IMediator mediator, ILogger<BankAcountController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get bank accounts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetBankAccounts.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewBankAccount)]
        public async Task<IActionResult> GetBankAccounts([FromQuery] GetBankAccounts.Query request)
        {
            if (request == null) request = new GetBankAccounts.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// create bank account 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(CreateBankAccount.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.CreateBankAccount)]
        public async Task<IActionResult> CreateBankAccount([FromBody] CreateBankAccount.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// connect bank account 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("connect")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConnectBankAccount.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.CreateBankAccount)]
        public async Task<IActionResult> ConnectBankAccount([FromBody] ConnectBankAccount.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update bank account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateBankAccount.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateBankAccount)]
        public async Task<IActionResult> UpdateBankAccount([FromRoute] Guid id, [FromBody] UpdateBankAccount.Command request)
        {
            request.Id = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// mark bank account as active/inactive
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}/update-status")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ActivateDeactivateBankAccount.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateBankAccount)]
        public async Task<IActionResult> ActivateDeactivateBankAccount([FromRoute] Guid id)
        {
            var request = new ActivateDeactivateBankAccount.Command
            {
                Id = id,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }


        /// <summary>
        /// delete bank account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteBankAccount.Response))]
        [ProducesResponseType(typeof(DeleteBankAccount.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.DeleteBankAccount)]
        public async Task<IActionResult> DeleteBankAccount([FromRoute] Guid id)
        {
            var request = new DeleteBankAccount.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get bank transaction upload template
        /// </summary>
        /// <param name="templateGenerator"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBankTransactionUploadTemplate([FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(BulkImportType.BankTransaction, CompanyId.GetValueOrDefault());

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// preview for importing bank transactions 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="excelReader"></param>
        /// <returns></returns>
        [HttpPost("preview-import-transaction")]
        [ValidateModel]
        [ProducesResponseType(typeof(List<PreviewTransactionImport>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ImportBankTransactions)]
        public async Task<IActionResult> PreviewImportBankTransactions(IFormFile file, [FromServices] IExcelReader excelReader)
        {
            var result = new BasicActionResult();
            if (file == null || Path.GetExtension(file.FileName) is not (".xls" or ".xlsx" or ".xlsm"))
            {
                result.ErrorMessage = "Upload a valid excel file";
                return CommandResponse(result);
            }
            DataTable excelData;
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;
                excelData = await excelReader.ReadExcelFile(stream);
                if (excelData == null || excelData.Rows.Count < 1)
                {
                    result.ErrorMessage = "You cannot upload an empty file";
                    return CommandResponse(result);
                }
                if (excelData.Columns.Count == 0 || excelData.Columns[0].ColumnName != "TransactionDate")
                {
                    result.ErrorMessage = "Use the template provided";
                    return CommandResponse(result);
                }
            }

            var jsonString = excelReader.SerializeDataTableToJSON(excelData);
            var data = JsonSerializer.Deserialize<List<PreviewTransactionImport>>(jsonString);
           
            return Ok(data);
        }

        /// <summary>
        /// get account transactions from mono
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{accountId}/transactions")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetAccountTransactionsFromMono.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewBankAccount)]
        public async Task<IActionResult> GetAccountTransactionsFromMono(Guid accountId, [FromQuery] GetAccountTransactionsFromMono.Query request)
        {
            if (request == null) request = new GetAccountTransactionsFromMono.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.AccountId = accountId;
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        ///// <summary>
        ///// get  imported bank transactions
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpGet("transactions")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(GetBankTransactions.Response), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        //[HasPermission(Permissions.ViewTransactions)]
        //public async Task<IActionResult> GetBankTransactions([FromQuery] GetBankTransactions.Query request)
        //{
        //    request.CompanyId = CompanyId.GetValueOrDefault();

        //    var result = await _mediator.Send(request);
        //    return QueryResponse(result);
        //}



        ///// <summary>
        ///// match bank transactions to ledger accounts
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPut("match-transaction")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(MatchBankTransaction.Response), StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        //[HasPermission(Permissions.ImportBankTransactions)]
        //public async Task<IActionResult> MatchBankTransactions([FromBody] MatchBankTransaction.Command request)
        //{
        //    request.CompanyId = CompanyId.GetValueOrDefault();
        //    request.UserId = UserId.GetValueOrDefault();
        //    var result = await _mediator.Send(request);
        //    return CommandResponse(result);
        //}

    }
}
