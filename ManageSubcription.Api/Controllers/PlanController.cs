using AutoMapper;
using ManageSubcription.Api.Authorizations;
using ManageSubcription.Api.Helpers;
using ManageSubcription.Api.Model;
using ManageSubcription.Api.Services;
using ManageSubcription.Api.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Models;
using Spine.Core.ManageSubcription.Filter;
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
    [Route("api/v1/plan")]
    [ApiController]
    [Authorize]
    public class PlanController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository service;
        private readonly ILogger<PlanController> logger;
        private readonly IMapper mapper;
        private readonly IUriService uriService;

        public PlanController(IManageSubcriptionRepository service, ILogger<PlanController> logger, IMapper mapper, IUriService uriService)
            : base(logger)
        {
            this.service = service;
            this.logger = logger;
            this.mapper = mapper;
            this.uriService = uriService;
        }

        /// <summary>
        /// get plans
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [HasPermission(Permissions.ViewSetting)]
        public IActionResult GetPlans([FromQuery] PlanQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<FilterPlan>(query);
            var posts = service.GetPlans(filter, pagination).ToList();
            var totalRecords = service.GetPlans(filter).Count();
            var postsReponse = mapper.Map<List<PlanViewModel>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<PlanViewModel>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        /// <summary>
        /// get plan by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("id")]
        [HasPermission(Permissions.ViewSetting)]
        public IActionResult Plan(int id)
        {
            var exMessage = string.Empty;
            try
            {
                object products = null;

                if (id > 0)
                {
                    products = service.GetPlanById(id);
                }

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = products
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
        /// create plan
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [HasPermission(Permissions.AddPlan)]
        public IActionResult AddSubscriptionPlan([FromBody] AddPlanViewModel model)
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

                if (model.PlanDuration < 1)
                    errorMessage = "Please specify duration in month eg 1 year will be 12";

                if (string.IsNullOrWhiteSpace(model.PlanName))
                    errorMessage = "Plan Name is a required field";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (!model.IsFreePlan)
                {
                    if (model.Amount == 0)
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.BadRequest,
                            message = "Amount must be greated than zero"
                        });
                    }

                    if (model.PlanDuration == 0)
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.BadRequest,
                            message = "Please specify plan duration in month"
                        });
                    }
                }
                

                if (service.AddPlan(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        /// update plan
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("update{planId}")]
        [HasPermission(Permissions.UpdatePlan)]
        public IActionResult EditSubscriptionPlan([FromRoute] int planId, [FromBody] AddPlanViewModel model)
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

                if (planId == 0)
                    errorMessage = "Valid PlanId is required";

                if (model.PlanDuration < 1)
                    errorMessage = "Please specify duration in month eg 1 year will be 12";

                if (string.IsNullOrWhiteSpace(model.PlanName))
                    errorMessage = "Plan Name is a required field";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (service.UpdatePlan(planId, model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        /// delete plan
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.DeletePlan)]
        public IActionResult DeleteSubscriptionPlan(int id)
        {
            var exMessage = string.Empty;
            try
            {
                if (service.DeletePlan(id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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

        ///  <summary>
        /// enable or disable plan
        ///  </summary>
        ///  <param name="Id"></param>
        ///  <returns></returns>
        [HttpPut("plantoggle{Id}")]
        public IActionResult TogglePlan([FromRoute] int Id)
        {
            try
            {
                var errorMessage = string.Empty;
                if (Id == 0)
                    errorMessage = "Id is a required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                var reponseMessage = service.TogglePlan(Id);
                return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.OK,
                    data = reponseMessage
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
