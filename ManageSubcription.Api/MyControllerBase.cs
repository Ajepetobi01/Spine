using ManageSubcription.Api.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Spine.Common.ActionResults;
using Spine.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ManageSubcription.Api
{
    public interface IMyControllerBase
    {
        Guid? CompanyId { get; set; }
        Guid? UserId { get; set; }
        string Username { get; set; }
        ILogger Logger { get; }

    }

    [SetUserPropertiesFilter]
    public class MyControllerBase : ControllerBase, IMyControllerBase
    {
        public Guid? CompanyId { get; set; }
        public Guid? UserId { get; set; }
        public string Username { get; set; }
        public ILogger Logger { get; }

        public MyControllerBase(ILogger logger)
        {
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

        protected IActionResult ExportResponse(string exportType, MemoryStream stream, string fileName)
        {
            stream.Position = 0;
            stream.Seek(0, SeekOrigin.Begin);
            switch (exportType)
            {
                case "excel":
                    // fileName += ".xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                // case "pdf":
                //      fileName += ".pdf";

                default:
                    return null;
            }



        }

    }
}
