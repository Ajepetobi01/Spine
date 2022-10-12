using AutoMapper;
using ManageSubcription.Api.Helpers;
using ManageSubcription.Api.Model;
using ManageSubcription.Api.Services;
using ManageSubcription.Api.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/PromotionalCode")]
    [ApiController]
    [Authorize]
    public class PromotionalCodeController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<PromotionalCodeController> _logger;
        private readonly IMapper mapper;
        private readonly IUriService uriService;

        public PromotionalCodeController(IManageSubcriptionRepository service, ILogger<PromotionalCodeController> logger, IMapper mapper, IUriService uriService) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.mapper = mapper;
            this.uriService = uriService;
        }
        /// <summary>
        /// get promotion
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public IActionResult GetPromotionalCode([FromQuery] PromotionalCodeQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<FilterPromotionalCode>(query);
            var posts = _service.GetPromoCode(filter, pagination).ToList();
            var totalRecords = _service.GetPromoCode(filter).Count();
            var postsReponse = mapper.Map<List<PromoViewModel>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<PromoViewModel>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        [HttpGet("{Id}")]
        public IActionResult GetPromotionalCodeById(Guid Id)
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.GetPromoCodeById(Id)
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
        public IActionResult CreatePromotionCode([FromBody] CreatePromoViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });


                if (_service.CreatePromoCode(model, CompanyId.GetValueOrDefault()))
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
        [HttpPut("update{Id}")]
        public IActionResult UpdatePromotionCode([FromRoute] Guid Id, [FromBody] CreatePromoViewModel model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });


                if (_service.UpdatePromoCode(Id, model, CompanyId.GetValueOrDefault()))
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
        [HttpDelete("Id")]
        public IActionResult DeletePromotionCode(Guid Id)
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


                if (_service.DeletePromoCode(Id, CompanyId.GetValueOrDefault()))
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
