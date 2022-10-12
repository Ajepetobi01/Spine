using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Spine.Core.Accounts.Commands.Companies;
using Microsoft.AspNetCore.Http;
using Spine.Core.Accounts.Queries.Companies;
using Accounts.Api.Filters;
using Accounts.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Common.Models;
using Spine.Core.Accounts.Commands.AccountingPeriods;
using Spine.Core.Accounts.Queries.AccountingPeriods;

namespace Accounts.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class CompanyController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CompanyController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public CompanyController(IMediator mediator, ILogger<CompanyController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get business profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("profile")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCompanyProfile.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewCompanyProfile)]
        public async Task<IActionResult> GetCompanyProfile()
        {
            var request = new GetCompanyProfile.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        ///// <summary>
        ///// get business financials
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("financial")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(GetCompanyFinancial.Response), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        //[HasPermission(Permissions.ViewCompanyFinancial)]
        //public async Task<IActionResult> GetCompanyFinancials()
        //{
        //    var request = new GetCompanyFinancial.Query
        //    {
        //        CompanyId = CompanyId.GetValueOrDefault()
        //    };
        //    var result = await _mediator.Send(request);
        //    return QueryResponse(result);
        //}

        /// <summary>
        ///update business profile
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("update-profile")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateCompanyProfile.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateCompanyProfile)]
        public async Task<IActionResult> UpdateBusinessProfile([FromBody] UpdateCompanyProfile.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get company base currency
        /// </summary>
        /// <returns></returns>
        [HttpGet("base-currency")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCompanyBaseCurrency.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyBaseCurrency()
        {
            var request = new GetCompanyBaseCurrency.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        
        /// <summary>
        /// get all accounting periods 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("accounting-periods")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetAccountingPeriods.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AccountingPeriods([FromQuery] GetAccountingPeriods.Query request)
        {
            request ??= new GetAccountingPeriods.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// create accounting period
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("accounting-period")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddAccountingPeriod.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ManageAccountingPeriod)]
        public async Task<IActionResult> AddAccountingPeriod([FromBody] AddAccountingPeriod.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// close accounting period
        /// </summary>
        /// <param name="periodId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("close-accounting-period/{periodId}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(CloseAccountingPeriod.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ManageAccountingPeriod)]
        public async Task<IActionResult> ManageAccountingPeriod([FromQuery] int periodId, [FromBody] CloseAccountingPeriod.Command request)
        {
            request.PeriodId = periodId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        /// <summary>
        /// reopen closed accounting period
        /// </summary>
        /// <param name="periodId"></param>
        /// <returns></returns>
        [HttpPut("reopen-accounting-period/{periodId}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ReopenAccountingPeriod.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ManageAccountingPeriod)]
        public async Task<IActionResult> ManageAccountingPeriod([FromQuery] int periodId)
        {
            var request = new ReopenAccountingPeriod.Command
            {
                PeriodId = periodId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

    }
}
