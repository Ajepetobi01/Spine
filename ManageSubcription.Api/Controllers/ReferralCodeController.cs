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
    [Route("api/v1/referralcode")]
    [ApiController]
    [Authorize]
    public class ReferralCodeController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<ReferralCodeController> _logger;

        public ReferralCodeController(IManageSubcriptionRepository service, ILogger<ReferralCodeController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("all")]
        [HasPermission(Permissions.ViewReferralCode)]
        public IActionResult GetReferralCodes()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.GetReferralCode()
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
        [HasPermission(Permissions.ViewReferralCode)]
        public IActionResult GetReferralCode(Guid Id)
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.GetReferralCode().Where(x => x.Id == Id).FirstOrDefault()
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
        [HasPermission(Permissions.AddReferralCode)]
        public IActionResult CreateReferralCode([FromBody] CreateReferralCodeViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });

                if (_service.AnyRecordInReferralCode()) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "ReferralCode already created please try and update"
                });

                if (_service.CreateReferralCode(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        [HttpPut("update{id}")]
        [HasPermission(Permissions.UpdateReferralCode)]
        public IActionResult UpdateReferralCode([FromRoute] Guid id, [FromBody] CreateReferralCodeViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });


                if (_service.UpdateReferralCode(id, model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        [HasPermission(Permissions.DeleteReferralCode)]
        public IActionResult DeleteReferralCode(Guid Id)
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


                if (_service.DeleteReferralCode(Id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
