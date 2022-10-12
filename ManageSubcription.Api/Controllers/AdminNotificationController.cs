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
    [ApiController]
    [Route("api/v1/admin-notification")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class AdminNotificationController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<AdminNotificationController> _logger;
        private readonly IMapper mapper;
        private readonly IUriService uriService;
        public AdminNotificationController(IManageSubcriptionRepository service, ILogger<AdminNotificationController> logger
            , IMapper mapper, IUriService uriService) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.mapper = mapper;
            this.uriService = uriService;
        }

        /// <summary>
        /// get all notifications
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paginationQuery"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AdminNotificationVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewSubscriber)]
        public IActionResult GetNotifications([FromQuery] QueryAdminNotification query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<FilterAdminNotification>(query);
            var posts = _service.GetAdminNotification(filter, pagination).ToList();
            var totalRecords = _service.GetAdminNotification(filter).Count();
            var postsReponse = mapper.Map<List<AdminNotificationVM>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<AdminNotificationVM>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }

        /// <summary>
        /// get notification by notification id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("Id")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UserRoleViewModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult GetNotification(Guid Id)
        {
            try
            {
                if(Id == Guid.Empty)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = $"Id is required"
                    });
                }

                var companyId = CompanyId.GetValueOrDefault();

                object notification = _service.GetAdminNotificationById(Id);
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

        ///  <summary>
        ///  Create Notification
        ///  </summary>
        ///  <param name="model"></param>
        ///  <returns></returns>
        ///  [Consumes(MediaTypeNames.Application.Json)]
        [HttpPost("create")]
        [Produces(typeof(AdminNotificationDTO))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult CreateNotification([FromBody] AdminNotificationDTO model)
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
                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description is required";
                if (string.IsNullOrWhiteSpace(model.ReminderDate))
                    errorMessage = "ReminderDate and time is required";

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

                var reponseMessage = _service.CreateAdmiNotification(model, CompanyId.GetValueOrDefault());
                return Ok(new APIResponseModel
                {
                    hasError = false,
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

        ///  <summary>
        /// update Notification
        ///  </summary>
        ///  <param name="Id"></param>
        ///  <param name="model"></param>
        ///  <returns></returns>
        [HttpPut("update{Id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AdminNotificationDTO))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult UpdateNotification([FromRoute] Guid Id, [FromBody] AdminNotificationDTO model)
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
                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description is required";
                if (string.IsNullOrWhiteSpace(model.ReminderDate))
                    errorMessage = "Reminder date and time is required";
                if (Id == Guid.Empty)
                    errorMessage = "Id is a required";

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

                var reponseMessage = _service.UpdateAdminNotification(Id, model, CompanyId.GetValueOrDefault());
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

        ///  <summary>
        /// push Reminder
        ///  </summary>
        ///  <param name="Id"></param>
        ///  <param name="model"></param>
        ///  <returns></returns>
        [HttpPut("remindLater{Id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(NotificationReminder))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult PushReminder([FromRoute] Guid Id, [FromBody] NotificationReminder model)
        {
            try
            {
                var errorMessage = string.Empty;
                if (Id == Guid.Empty)
                    errorMessage = "Id is a required";
                if (string.IsNullOrWhiteSpace(model.ReminderDate))
                    errorMessage = "Reminder date and time is required";

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

                var reponseMessage = _service.RemindmeLater(Id, model, CompanyId.GetValueOrDefault());
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

        ///  <summary>
        /// delete notification
        ///  </summary>
        ///  <param name="Id"></param>
        ///  <returns></returns>
        [HttpDelete("delete{Id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(APIResponseModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public IActionResult DeleteAdmiNotification([FromRoute] Guid Id)
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

                var reponseMessage = _service.DeleteAdmiNotification(Id, CompanyId.GetValueOrDefault());
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
