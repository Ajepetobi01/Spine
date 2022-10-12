using ManageSubcription.Api.Authorizations;
using ManageSubcription.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/template")]
    [ApiController]
    [Authorize]
    public class TemplateController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<TemplateController> _logger;
        public TemplateController(IManageSubcriptionRepository service, ILogger<TemplateController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpGet("all")]
        [HasPermission(Permissions.ViewTemplate)]
        public IActionResult GetGetTemplates()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.GetTemplates()
                });
            }
            catch (Exception e)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = e.Message
                });
            }
        }
        [HttpGet("{Id}")]
        [HasPermission(Permissions.ViewTemplate)]
        public IActionResult GetGetTemplate(Guid Id)
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.GetTemplates().Where(x => x.Id == Id).FirstOrDefault()
                });
            }
            catch (Exception e)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = e.Message
                });
            }
        }
        [HttpPost("create")]
        [HasPermission(Permissions.AddTemplate)]
        public IActionResult CreateTemplate([FromBody] CreateTemplateViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });

                var errorMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(model.Name))
                    errorMessage = "Name is a required field";
                if (string.IsNullOrWhiteSpace(model.Subject))
                    errorMessage = "Subject message is required";
                if (string.IsNullOrWhiteSpace(model.Body))
                    errorMessage = "Body message is required";
                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (_service.CreateTemplate(model))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.OK,
                        message = "success"
                    });
                }

                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = "Internal Server Error"
                });
            }
            catch (Exception ex)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = ex.Message
                });
            }
        }
        [HttpPut("update{templateId}")]
        [HasPermission(Permissions.UpdateTemplate)]
        public IActionResult UpdateTemplate([FromRoute] Guid templateId, [FromBody] CreateTemplateViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });

                var errorMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(model.Name))
                    errorMessage = "Name is a required field";
                if (string.IsNullOrWhiteSpace(model.Subject))
                    errorMessage = "Subject message is required";
                if (string.IsNullOrWhiteSpace(model.Body))
                    errorMessage = "Body message is required";
                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (_service.UpdateTemplate(templateId, model))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.OK,
                        message = "success"
                    });
                }

                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = "Internal Server Error"
                });
            }
            catch (Exception ex)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = ex.Message
                });
            }
        }
        [HttpDelete("Id")]
        [HasPermission(Permissions.DeleteTemplate)]
        public IActionResult DeleteTemplate(Guid Id)
        {
            var exMessage = string.Empty;
            try
            {
                var errorMessage = string.Empty;

                if (Id == Guid.Empty)
                    errorMessage = "Valid Id is a required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });


                if (_service.DeleteTemplate(Id))
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = false,
                        statusCode = (int)HttpStatusCode.OK,
                        message = "success"
                    });
                }
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = "Internal Server Error"
                });

            }
            catch (Exception ex)
            {
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = ex.Message
                });
            }
        }
    }
}
