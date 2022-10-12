using ManageSubcription.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/shipping")]
    [ApiController]
    [Authorize]
    public class ShippingController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<ShippingController> _logger;
        private readonly IConfiguration config;

        public ShippingController(IManageSubcriptionRepository service, ILogger<ShippingController> logger, 
            IConfiguration config) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.config = config;
        }

        [Route("all-record")]
        [HttpGet]
        public IActionResult GetShippings()
        {
            try
            {
                object shippings = _service.GetShipping();
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = shippings
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

        [Route("fetch/{id}")]
        [HttpGet]
        public IActionResult GetShipping(int id)
        {
            try
            {
                if (id > 0)
                {
                    object billings = _service.GetShippingById(id);
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

        [Route("create")]
        [HttpPost]
        public IActionResult AddShipping([FromBody] SubscriberShippingDTO model)
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

                if (_service.CreateShipping(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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

        [Route("update")]
        [HttpPut]
        public IActionResult UpdateShipping([FromBody] SubscriberShippingDTO model)
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

                if (_service.UpdateShipping(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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

        [HttpDelete("delete")]
        public IActionResult DeleteShipping(int id)
        {
            var exMessage = string.Empty;
            try
            {
                if (_service.DeleteShipping(id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
