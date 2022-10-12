using ManageSubcription.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spine.Common.Models;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/billing")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class BillingController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<BillingController> _logger;
        private readonly IConfiguration config;

        public BillingController(IManageSubcriptionRepository service, ILogger<BillingController> logger, IConfiguration config) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.config = config;
        }

        /// <summary>
        /// get all billings
        /// </summary>
        /// <returns></returns>
        [HttpGet("all-record")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult Getbillings()
        {
            try
            {
                object billings = _service.GetBillings();
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = billings
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

        /// <summary>
        /// get billing by billing id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("fetch/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(SubscriberBillingViewModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult Getbilling(int id)
        {
            try
            {
                if (id > 0)
                {
                    object billings = _service.GetBillingsById(id);
                    return Ok(new APIResponseModel
                    {
                        statusCode = (int)HttpStatusCode.OK,
                        data = billings
                    });
                }
                else
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = $"Id is required"
                    });
                }

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

        /// <summary>
        /// create billing
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(SubscriberBillingDTO))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult AddBilling([FromBody] SubscriberBillingDTO model)
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

                
                if (string.IsNullOrWhiteSpace(model.Address1))
                    errorMessage = "Address is a required field";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });


                if (_service.CreateBilling(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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

        /// <summary>
        /// update billing
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(SubscriberBillingDTO))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult UpdateBilling([FromBody] SubscriberBillingDTO model)
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

                if (model.ID_Billing < 1)
                    errorMessage = "Billing Id is a required";

                if (string.IsNullOrWhiteSpace(model.Address1))
                    errorMessage = "Address is a required field";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (_service.UpdateBilling(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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

        /// <summary>
        /// delete Billing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(APIResponseModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public IActionResult DeleteBilling(int id)
        {
            var exMessage = string.Empty;
            try
            {
                var errorMessage = string.Empty;
                if (id < 1)
                    errorMessage = "Note Id is a required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (_service.DeleteBilling(id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
