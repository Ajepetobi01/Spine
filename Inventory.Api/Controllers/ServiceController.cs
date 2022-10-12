using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Inventory.Api.Authorizations;
using Inventory.Api.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Models;
using Spine.Core.Inventories.Commands.Service;
using Spine.Core.Inventories.Queries.Service;
using Spine.Services;

namespace Inventory.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class ServiceController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ServiceController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public ServiceController(IMediator mediator, ILogger<ServiceController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get services
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetServices.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServices([FromQuery] GetServices.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get service by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetServiceById.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceById(Guid id)
        {
            var request = new GetServiceById.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// add service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddService.Response))]
        [ProducesResponseType(typeof(AddService.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AddInventory)]
        public async Task<IActionResult> AddService(AddService.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get services upload template
        /// </summary>
        /// <param name="templateGenerator"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServicesUploadTemplate([FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(BulkImportType.Services, CompanyId.GetValueOrDefault());

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// upload bulk services
        /// </summary>
        /// <param name="file"></param>
        /// <param name="excelReader"></param>
        /// <returns></returns>
        [HttpPost("upload-bulk")]
        [ProducesResponseType(typeof(AddBulkServices.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddInventory)]
        public async Task<IActionResult> UploadBulkServices(IFormFile file, [FromServices] IExcelReader excelReader)
        {
            var result = new BasicActionResult();
            if (file == null || Path.GetExtension(file.FileName) is not (".xls" or ".xlsx" or ".xlsm"))
            {
                result.ErrorMessage = "Upload a valid excel file";
                return CommandResponse(result);
            }
            DataTable excelData;
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;
                excelData = await excelReader.ReadExcelFile(stream);
                if (excelData == null || excelData.Rows.Count < 1)
                {
                    result.ErrorMessage = "You cannot upload an empty file";
                    return CommandResponse(result);
                }
                if (excelData.Columns.Count == 0 || excelData.Columns[0].ColumnName != "Name")
                {
                    result.ErrorMessage = "Use the template provided";
                    return CommandResponse(result);
                }
            }

            var jsonString = excelReader.SerializeDataTableToJSON(excelData);
            var services = JsonSerializer.Deserialize<List<AddBulkServices.ServiceModel>>(jsonString);
            if (services == null) return CommandResponse(result);
            
            if (services.Any(x => x.Name.IsNullOrEmpty()))
            {
                result.ErrorMessage = "Fill all required fields";
                return CommandResponse(result);
            }
            var request = new AddBulkServices.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Services = services,
                UserId = UserId.GetValueOrDefault()
            };

            result = await _mediator.Send(request);
            return CommandResponse(result);
        }


        /// <summary>
        /// update service
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateService.Response))]
        [ProducesResponseType(typeof(UpdateService.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> UpdateService(Guid id, UpdateService.Command request)
        {
            request.Id = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

    }
}
