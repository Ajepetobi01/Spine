using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Spine.Common.Models;
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Misc;
using AutoMapper;
using Spine.Common.Extensions;
using Spine.Common.Enums;
using Spine.Services.Paystack.Verification;

namespace Transactions.Api.Controllers
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


        ///// <summary>
        ///// get list of banks (from json)
        ///// </summary>
        ///// <param name="env"></param>
        ///// <returns></returns>
        //[HttpGet("banks")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[Produces(typeof(List<BanksModel>))]
        //[ProducesResponseType(typeof(List<BanksModel>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        //public IActionResult Banks([FromServices] IWebHostEnvironment env)
        //{
        //    using StreamReader r = new($"{env.ContentRootPath}/Banks.json");
        //    string json = r.ReadToEnd();
        //    var banks = JsonSerializer.Deserialize<List<BanksModel>>(json);
        //    return Ok(banks);
        //}

        /// <summary>
        /// get list of banks (from paystack api)
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="paystackClient"></param>
        /// <returns></returns>
        [HttpGet("banks")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(List<BanksModel>))]
        [ProducesResponseType(typeof(List<BanksModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Banks([FromServices] PaystackClient paystackClient, [FromServices] IMapper mapper)
        {
            var banks = new List<BanksModel>();
            var response = await new ListBanks.Handler().Handle(new ListBanks.Request(), paystackClient);
            if (response != null && response.Status)
            {
                banks = mapper.Map<List<BanksModel>>(response.Data);
            }

            return Ok(banks);
        }

        /// <summary>
        /// get account name (with paystack api)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="paystackClient"></param>
        /// <param name="accountNumber"></param>
        /// <param name="bankCode"></param>
        /// <returns></returns>
        [HttpGet("get-account-name")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountName([FromServices] PaystackClient paystackClient, [FromQuery] string accountNumber, [FromQuery] string bankCode)
        {
            var response = await new VerifyAccountNumber.Handler().Handle(new VerifyAccountNumber.Request { AccountNo = accountNumber, BankCode = bankCode }, paystackClient);
            if (response != null && response.Status)
            {
                return Ok(response.Data.AccountName);
            }

            return NotFound("Could not fetch account name. Try again");
        }

        ///// <summary>
        ///// transaction types
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("transaction-types")]
        //public IActionResult TransactionTypes()
        //{
        //    return Ok(EnumExtensions.GetValues<TransactionType>());
        //}
        
        /// <summary>
        /// profit and loss types for report
        /// </summary>
        /// <returns></returns>
        [HttpGet("pl-report")]
        public IActionResult PLReportTypes()
        {
            return Ok(EnumExtensions.GetValues<PLReportType>());
        }

    }
}
