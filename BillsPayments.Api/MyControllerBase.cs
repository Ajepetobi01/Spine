using System;
using System.Net;
using BillsPayments.Api.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Common.ActionResults;
using Spine.Common.Models;

namespace BillsPayments.Api
{
    public interface IMyControllerBase
    {
        Guid? CompanyId { get; set; }
        Guid? UserId { get; set; }
        string Username { get; set; }
        IMediator Mediator { get; }
        ILogger Logger { get; }

    }

    [SetUserPropertiesFilter]
    public class MyControllerBase : ControllerBase, IMyControllerBase
    {
        public Guid? CompanyId { get; set; }
        public Guid? UserId { get; set; }
        public string Username { get; set; }

        public IMediator Mediator { get; }
        public ILogger Logger { get; }

        public MyControllerBase(IMediator mediator, ILogger logger)
        {
            Mediator = mediator;
            Logger = logger;
        }

        protected IActionResult CommandResponse(BasicActionResult result)
        {
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                var objectResult = new ObjectResult(new BaseResponse { ErrorMessage = result.ErrorMessage })
                {
                    StatusCode = (int)result.Status
                };

                return objectResult;
            }

            if (result.Status == HttpStatusCode.OK)
            {
                return Ok(result);
            }

            return StatusCode((int)result.Status, result);
        }

        protected IActionResult QueryResponse(object data)
        {
            if (data == null)
            {
                return NotFound("No record found");
            }

            var objectResult = new ObjectResult(data)
            {
                StatusCode = 200
            };

            return objectResult;
        }

    }

}
