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
    [Route("api/v1/note")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class NoteController : MyControllerBase
    {
        private readonly IManageSubcriptionRepository _service;
        private readonly ILogger<NoteController> _logger;
        private readonly IConfiguration config;

        public NoteController(IManageSubcriptionRepository service, ILogger<NoteController> logger, IConfiguration config) : base(logger)
        {
            _service = service;
            _logger = logger;
            this.config = config;
        }

        /// <summary>
        /// get all notes
        /// </summary>
        /// <returns></returns>
        [HttpGet("all-record")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetNotes()
        {
            try
            {
                object notes = _service.GetNotes();
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = notes
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
        /// get Note by Note id
        /// </summary>
        /// <param name="noteid"></param>
        /// <returns></returns>
        [HttpGet("fetch/{noteid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(SubscriberNoteViewModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult GetNote(int noteid)
        {
            try
            {
                if (noteid > 0)
                {
                    object note = _service.GetNote(noteid);
                    return Ok(new APIResponseModel
                    {
                        statusCode = (int)HttpStatusCode.OK,
                        data = note
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
        /// get Note by company id
        /// </summary>
        /// <param name="companyid"></param>
        /// <returns></returns>
        [HttpGet("fetch/by-company/{companyid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(SubscriberNoteViewModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult GetNoteByCompaning(Guid companyid)
        {
            try
            {
                object note = _service.GetCompanyNote(companyid);
                return Ok(new APIResponseModel
                {
                    statusCode = (int)HttpStatusCode.OK,
                    data = note
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
        /// create note
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(NoteRequest))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult AddNote([FromBody] NoteRequest model)
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

                if(model.CompanyId == Guid.Empty) 
                    errorMessage = "Company Id is a required";

                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description message is required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (_service.AddNote(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        /// update note
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(NoteRequest))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult UpdateNote([FromBody] NoteRequest model)
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

                if(model.noteId < 1)
                    errorMessage = "Note Id is a required";

                if (model.CompanyId == Guid.Empty)
                    errorMessage = "Company Id is a required";

                if (string.IsNullOrWhiteSpace(model.Description))
                    errorMessage = "Description message is required";

                if (!string.IsNullOrWhiteSpace(errorMessage)) return Ok(new APIResponseModel
                {
                    hasError = true,
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = errorMessage
                });

                if (_service.UpdateNote(model, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
        [HttpDelete("delete")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(APIResponseModel))]
        [ProducesResponseType(typeof(APIResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public IActionResult DeleteNote(int id)
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


                if (_service.DeleteNote(id, CompanyId.GetValueOrDefault(), UserId.GetValueOrDefault()))
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
