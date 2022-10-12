using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spine.Common.Enums;
using Spine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Controllers
{
    [Route("api/v1/downloadtemplate")]
    [ApiController]
    public class DownloadTemplateController : ControllerBase
    {
        [HttpGet("bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubscriberUploadTemplate([FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(BulkImportType.Subscriber, null);

            return File(stream, contentType, fileName);
        }
    }
}
