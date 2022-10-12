using AutoMapper;
using ManageSubcription.Api.Helpers;
using ManageSubcription.Api.Model;
using ManageSubcription.Api.Services;
using ManageSubcription.Api.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    [Route("api/v1/notification")]
    [ApiController]
    [Authorize]
    public class NotificationController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<NotificationController> _logger;
        private readonly IConfiguration config;
        private readonly IMapper mapper;
        private readonly IUriService uriService;

        public NotificationController(IManageSubcriptionRepository service, ILogger<NotificationController> logger, IConfiguration config
            , IMapper mapper, IUriService uriService) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.config = config;
            this.mapper = mapper;
            this.uriService = uriService;
        }

        /// <summary>
        /// get all notifications
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paginationQuery"></param>
        /// <returns></returns>
        [HttpGet("all-record")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AdminNotificationVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetNotifications([FromQuery] QueryAdminNotification query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<FilterAdminNotification>(query);
            var posts = _service.GetNotefications(filter, pagination).ToList();
            var totalRecords = _service.GetNotefications(filter).Count();
            var postsReponse = mapper.Map<List<SubscriberNotificationViewModel>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<SubscriberNotificationViewModel>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }

        [Route("fetch/{notificationid}")]
        [HttpGet]
        public IActionResult GetNotification(Guid notificationid)
        {
            try
            {
                if(notificationid == Guid.Empty)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = $"Id is required"
                    });
                }

                object notification = _service.GetNotefication(notificationid);
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = notification
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

        [Route("fetch/by-company/{companyid}")]
        [HttpGet]
        public IActionResult GetCompanyNotification(Guid companyid)
        {
            try
            {
                var errorMessage = string.Empty;

                if (companyid == Guid.Empty)
                    errorMessage = "Company Id is a required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                object notification = _service.GetCompanyNotefication(companyid);
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = notification
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

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> AddNotification([FromBody] NotificationRequest model)
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

                if (model.CompanyId == Guid.Empty)
                    errorMessage = "Company Id is a required";
                if (string.IsNullOrWhiteSpace(model.ReminderDate))
                    errorMessage = "ReminderDate and time is required";
                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description message is required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                var dateofbirth = new DateTime();
                if (!string.IsNullOrWhiteSpace(model.ReminderDate))
                {
                    try
                    {
                        model.ReminderDate = CovertDate(model.ReminderDate);
                        dateofbirth = Convert.ToDateTime(model.ReminderDate);
                    }
                    catch
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.InternalServerError,
                            message = "Reminder Date not valid expected date is dd/MM/yyyy"
                        });
                    }
                }

                if (await _service.AddNotification(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        public async Task<IActionResult> UpdateNotification([FromBody] NotificationRequest model)
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

                if (model.NotificationId < 1)
                    errorMessage = "Notification Id is a required";

                if (model.CompanyId == Guid.Empty)
                    errorMessage = "Company Id is a required";
                if (string.IsNullOrWhiteSpace(model.ReminderDate))
                    errorMessage = "ReminderDate and time is required";
                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description message is required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                var dateofbirth = new DateTime();
                if (!string.IsNullOrWhiteSpace(model.ReminderDate))
                {
                    try
                    {
                        model.ReminderDate = CovertDate(model.ReminderDate);
                        dateofbirth = Convert.ToDateTime(model.ReminderDate);
                    }
                    catch
                    {
                        return Ok(new APIResponseModel
                        {
                            hasError = true,
                            statusCode = (int)HttpStatusCode.InternalServerError,
                            message = "Reminder Date not valid expected date is dd/MM/yyyy"
                        });
                    }
                }
                if (await _service.UpdateNotification(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        public IActionResult DeleteNote(int id)
        {
            var exMessage = string.Empty;
            try
            {
                var errorMessage = string.Empty;

                if (id < 1)
                    errorMessage = "Notification Id is a required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });


                if (_service.DeleteNotification(id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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


        [Route("count-notification")]
        [HttpGet]
        public IActionResult CountNotification()
        {
            try
            {
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = _service.CountNotifications()
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
        /// enable or disable notification
        ///  </summary>
        ///  <param name="Id"></param>
        ///  <returns></returns>
        [HttpPut("notificationtoggle{Id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(APIResponseModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public IActionResult ToggleNotification([FromRoute] Guid Id)
        {
            try
            {
                var errorMessage = string.Empty;
                if (Id == Guid.Empty)
                    errorMessage = "Id is a required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                var reponseMessage = _service.ToggleNotification(Id);
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

        private static string CovertDate(string getdate)
        {
            try
            {
                string[] dateTokens = getdate.Split('/', '-');
                string strDay = dateTokens[0];
                string strMonth = dateTokens[1];
                string strYear = dateTokens[2];
                string date = strMonth + "/" + strDay + "/" + strYear;

                return date;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
