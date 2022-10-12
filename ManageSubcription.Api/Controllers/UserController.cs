using AutoMapper;
using ManageSubcription.Api.Authorizations;
using ManageSubcription.Api.Helpers;
using ManageSubcription.Api.Model;
using ManageSubcription.Api.Services;
using ManageSubcription.Api.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
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
    [Route("api/v1/user")]
    [ApiController]
    [Authorize]
    public class UserController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper mapper;
        private readonly IUriService uriService;
        public UserController(IManageSubcriptionRepository service, ILogger<UserController> logger
            , IMapper mapper, IUriService uriService) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.mapper = mapper;
            this.uriService = uriService;
        }
        [HttpGet("all")]
        [HasPermission(Permissions.ViewUsers)]
        public async Task<IActionResult> GetUsers([FromQuery] PostUserQuery query, [FromQuery] PaginationQuery paginationQuery)
        {
            var pagination = mapper.Map<PaginationFilter>(paginationQuery);
            var filter = mapper.Map<PostUserFilter>(query);
            var posts = _service.GetUsers(filter, pagination).ToList();
            var totalRecords = await _service.GetUsers(filter).CountAsync();
            var postsReponse = mapper.Map<List<ListUserVM>>(posts);
            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<ListUserVM>(postsReponse));
            }
            var paginationResponse = PaginationHelper.PagedResponse(uriService, pagination, postsReponse, totalRecords);
            return Ok(paginationResponse);
        }
        [HttpGet("Id")]
        [HasPermission(Permissions.ViewUsers)]
        public IActionResult GetUser(Guid Id)
        {
            try
            {
                if (Id != Guid.Empty)
                {
                    object role = _service.GetUser(Id);
                    return Ok(new APIResponseModel
                    {
                        statusCode = (int)HttpStatusCode.OK,
                        data = role
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
        [HttpPost("create")]
        [HasPermission(Permissions.UpdateUser)]
        public async Task<IActionResult> CreateUser([FromBody] UserVM model)
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
                if (string.IsNullOrWhiteSpace(model.UserName))
                    errorMessage = "User Name is a required field";
                if (string.IsNullOrWhiteSpace(model.Email))
                    errorMessage = "Email message is required";
                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (model.Role == Guid.Empty)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Valid roleId is required"
                    });
                }

                var reponseMessage = await _service.CreateUser(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault());
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
        [HttpPut("{Id}")]
        [HasPermission(Permissions.UpdateUser)]
        public IActionResult UpdateUser([FromRoute] Guid Id, [FromBody] UpdateUserVM model)
        {
            try
            {
                if (model == null) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Bad Request"
                });


                if (model.Role == Guid.Empty)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Valid roleId is required"
                    });
                }

                if (Id == Guid.Empty)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Valid Id is required"
                    });
                }

                var reponseMessage = _service.UpdateUser(Id, model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault());
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
        //[HasPermission(Permissions.UpdateUserRole)]
        //public async Task<IActionResult> RoleTransfer(RoleTransferVM model)
        //{
        //    try
        //    {
        //        if (model == null) return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.BadRequest,
        //            message = "Bad Request"
        //        });


        //        if (model.FromUserId == Guid.Empty)
        //        {
        //            return Ok(new APIResponseModel
        //            {
        //                hasError = true,
        //                statusCode = (int)HttpStatusCode.BadRequest,
        //                message = "From UserId is required"
        //            });
        //        }

        //        if (model.ToUserId == Guid.Empty)
        //        {
        //            return Ok(new APIResponseModel
        //            {
        //                hasError = true,
        //                statusCode = (int)HttpStatusCode.BadRequest,
        //                message = "ToUser Id is required"
        //            });
        //        }

        //        var reponseMessage = _service.RoleTransfer(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault());
        //        return Ok(new APIResponseModel
        //        {
        //            hasError = false,
        //            statusCode = (int)HttpStatusCode.OK,
        //            data = reponseMessage
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new APIResponseModel
        //        {
        //            hasError = true,
        //            statusCode = (int)HttpStatusCode.InternalServerError,
        //            message = ex.Message
        //        });
        //    }
        //}
        [HttpDelete("Id")]
        [HasPermission(Permissions.DeleteUsers)]
        public IActionResult DeleteUser(Guid Id)
        {
            var exMessage = string.Empty;
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


                if (_service.DeleteUser(Id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        [HttpPut("usertoggle{Id}")]
        public IActionResult ToggleUser([FromRoute] Guid Id)
        {
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

                var reponseMessage = _service.ToggleUser(Id);
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
