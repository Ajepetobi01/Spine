using AutoMapper;
using ManageSubcription.Api.Authorizations;
using ManageSubcription.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Models;
using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [ApiController]
    [Route("api/v1/role")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class RoleController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<RoleController> _logger;
        private readonly IMapper mapper;
        private readonly IAuditLogHelper auditHelper;
        private readonly SpineContext _context;
        private readonly RoleManager<ApplicationRole> roleManager;

        public RoleController(IManageSubcriptionRepository service, ILogger<RoleController> logger, 
            IMapper mapper, IAuditLogHelper auditHelper, SpineContext context, RoleManager<ApplicationRole> roleManager) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.mapper = mapper;
            this.auditHelper = auditHelper;
            _context = context;
            this.roleManager = roleManager;
        }

        /// <summary>
        /// get all roles
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("all")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewRole)]
        public async Task<IActionResult> GetRoles([FromQuery] RolePostQuery query)
        {
            try
            {
                var companyId = CompanyId.GetValueOrDefault();

                var filter = mapper.Map<RolePostFilter>(query);
                var ListRole = await _service.GetRoles(filter);

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = ListRole
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
        /// get roles ( for dropdown)
        /// </summary>
        /// <returns></returns>
        [HttpGet("slim")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleforDrp()
        {
            var exMessage = string.Empty;
            try
            {
                var roles = await _service.GetSlimRoles();

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = roles
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
        /// get subscriber roles(for dropdown)
        /// </summary>
        /// <returns></returns>
        [HttpGet("subscriber-role")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubscriberRoleforDrp()
        {
            var exMessage = string.Empty;
            try
            {
                var roles = await _service.GetSubscriberRoles();

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = roles
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
        /// get admin roles(for dropdown)
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin-role")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAdminRoleforDrp()
        {
            var exMessage = string.Empty;
            try
            {
                var roles = await _service.GetAdminRoles();

                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = roles
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
        /// get role by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("Id")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UserRoleViewModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ViewRole)]
        public async Task<IActionResult> GetRole(Guid Id)
        {
            try
            {
                if (Id != Guid.Empty)
                {
                    var companyId = CompanyId.GetValueOrDefault();

                    object role = await _service.GetRoleById(Id);
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

        ///  <summary>
        ///  Create Role
        ///  </summary>
        ///  <param name="model"></param>
        ///  <returns></returns>
        [HttpPost("create")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(RoleViewModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddRole)]
        public async Task<IActionResult> CreateRole([FromBody] RoleViewModel model)
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
                if (string.IsNullOrWhiteSpace(model.Role))
                    errorMessage = "Role is a required field";
                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description message is required";
                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                var result = await _service.AddRole(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault());
                if (result)
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
        /// update role
        ///  </summary>
        ///  <param name="Id"></param>
        ///  <param name="model"></param>
        ///  <returns></returns>
        [HttpPut("{Id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(RoleViewModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        //[HasPermission(Permissions.UpdateRole)]
        public async Task<IActionResult> UpdateRole([FromRoute] Guid Id, [FromBody] RoleViewModel model)
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
                if (string.IsNullOrWhiteSpace(model.Role))
                    errorMessage = "Role is a required field";
                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description message is required";
                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (Id == Guid.Empty)
                {
                    return Ok(new APIResponseModel
                    {
                        hasError = true,
                        statusCode = (int)HttpStatusCode.BadRequest,
                        message = "Valid role id is required"
                    });
                }


                if (await _service.UpdateRole(Id, model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
                {
                    auditHelper.SaveAction(_context, CompanyId.GetValueOrDefault(),
                       new AuditModel
                       {
                           EntityType = (int)AuditLogEntityType.Role,
                           Action = (int)AuditLogRoleAction.Update,
                           Description = $"Updated Role {Id} Description: {model.Description}",
                           UserId = UserId.GetValueOrDefault()
                       });

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

        /// <summary>
        /// delete role
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("Id")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(APIResponseModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.DeleteRole)]
        public async Task<IActionResult> DeleteRole(Guid Id)
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


                if (await _service.DeleteRole(Id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        [HttpPut("exposetoggle{Id}")]
        public IActionResult ToggleRole([FromRoute] Guid Id)
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

                var reponseMessage = _service.ToggleRole(Id);
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

        ///// <summary>
        ///// get all roles
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //[HttpGet("all")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        ////[HasPermission(Permissions.ViewRole)]
        //public async Task<IActionResult> GetRoles([FromQuery] RolePostQuery query)
        //{
        //    try
        //    {
        //        var companyId = CompanyId.GetValueOrDefault();

        //        var filter = mapper.Map<RolePostFilter>(query);
        //        var ListRole = await _service.GetRoles(filter);

        //        return Ok(new APIResponseModel
        //        {
        //            statusCode = (int)HttpStatusCode.OK,
        //            data = ListRole
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
        ///// <summary>
        ///// get role by role id
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //[HttpGet("Id")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[Produces(typeof(UserRoleViewModel))]
        //[ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        ////[HasPermission(Permissions.ViewRole)]
        //public async Task<IActionResult> GetRole(Guid Id)
        //{
        //    try
        //    {
        //        if (Id != Guid.Empty)
        //        {
        //            var companyId = CompanyId.GetValueOrDefault();

        //            object role = await _service.GetRole(Id);
        //            return Ok(new APIResponseModel
        //            {
        //                statusCode = (int)HttpStatusCode.OK,
        //                data = role
        //            });
        //        }
        //        else
        //        {
        //            return Ok(new APIResponseModel
        //            {
        //                hasError = true,
        //                statusCode = (int)HttpStatusCode.BadRequest,
        //                message = $"Id is required"
        //            });
        //        }

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

    }
}
