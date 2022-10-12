
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Common.Models;
using Spine.Data.Documents.Service.Interfaces;
using Spine.Data.Documents.ViewModels;

namespace Spine.DocumentService.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public class UploadsController : MyControllerBase
    {
        private readonly ILogger<UploadsController> _logger;
        private readonly IUploadService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public UploadsController(ILogger<UploadsController> logger, IUploadService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// get uploaded data
        /// </summary>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        [HttpGet("{uploadId}")]
        [ProducesResponseType(typeof(UploadModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUpload([FromRoute] string uploadId)
        {
            var companyId = CompanyId.GetValueOrDefault();
            var data = await _service.GetUpload(companyId, uploadId);
            if (data == null) return NotFound();

            return Ok(data);
        }

        /// <summary>
        /// get upload base64
        /// </summary>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-base64/{uploadId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImageBase64([FromRoute] string uploadId)
        {
            var data = await _service.GetUploadBase64(uploadId);
            return Ok(data);
        }

        /// <summary>
        /// upload file
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("")]
        [ValidateModel]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile([FromBody] UploadModel model)
        {
            model.CompanyId = CompanyId.GetValueOrDefault();
            model.UserId = UserId.GetValueOrDefault();

            var save = await _service.SaveUpload(model);
            if (string.IsNullOrEmpty(save)) return BadRequest("Could not save file");

            return Ok(save);
        }
    }
}
