using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Collections.Generic;
using Spine.Core.Customers.Queries;
using Spine.Core.Customers.Commands;
using Microsoft.AspNetCore.Http;
using Customers.Api.Filters;
using Spine.Common.ActionResults;
using System.IO;
using Spine.Services;
using System.Data;
using System.Linq;
using Customers.Api.Authorizations;
using Spine.Common.Enums;
//using ExcelCsvExport.Helpers;
using Spine.Common.Models;
using System.Text.Json;
using Spine.Common.Extensions;

namespace Customers.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class CustomersController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomersController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public CustomersController(IMediator mediator, ILogger<CustomersController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get  count of all customers 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("total-count")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCustomerCount.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewCustomers)]
        public async Task<IActionResult> CustomerCount([FromQuery] GetCustomerCount.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get all customers 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCustomers.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewCustomers)]
        public async Task<IActionResult> Customers([FromQuery] GetCustomers.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get customers search by name or email or business name ( for dropdown)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("slim")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCustomersByName.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CustomersByName([FromQuery] GetCustomersByName.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get customer by id
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("{customerId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCustomer.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewCustomers)]
        public async Task<IActionResult> Customer([FromRoute] Guid customerId)
        {
            var request = new GetCustomer.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = customerId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// add individual customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddCustomer.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddCustomer)]
        public async Task<IActionResult> AddCustomer([FromBody] AddCustomer.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// add individual customer from invoice
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("slim")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddQuickCustomer.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddCustomer)]
        public async Task<IActionResult> AddQuickCustomer([FromBody] AddQuickCustomer.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// add customer note
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{customerId}/add-note")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddCustomerNote.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddCustomer)]
        public async Task<IActionResult> AddCustomerNote([FromRoute] Guid customerId, [FromBody] AddCustomerNote.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = customerId;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update customer note
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{noteId}/update-note")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateCustomerNote.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateCustomer)]
        public async Task<IActionResult> UpdateCustomerNote([FromRoute] Guid noteId, [FromBody] UpdateCustomerNote.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = noteId;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete customer note
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns></returns>
        [HttpDelete("{noteId}/delete-note")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeleteCustomerNote.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateCustomer)]
        public async Task<IActionResult> DeleteCustomerNote([FromRoute] Guid noteId)
        {
            var request = new DeleteCustomerNote.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = noteId
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// set customer reminder
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{customerId}/add-reminder")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddCustomerReminder.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCustomerReminder([FromRoute] Guid customerId, [FromBody] AddCustomerReminder.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = customerId;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// extend customer reminder
        /// </summary>
        /// <param name="reminderId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{reminderId}/extend-reminder")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ExtendCustomerReminder.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExtendCustomerReminder([FromRoute] Guid reminderId, [FromBody] ExtendCustomerReminder.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = reminderId;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{customerId}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateCustomer.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateCustomer)]
        public async Task<IActionResult> UpdateCustomer([FromRoute] Guid customerId, [FromBody] UpdateCustomer.Command request)
        {
            request.Id = customerId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete customer 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteCustomer.Response))]
        [ProducesResponseType(typeof(DeleteCustomer.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        //      [ValidateModel]
        [HasPermission(Permissions.DeleteCustomer)]
        public async Task<IActionResult> DeleteCustomer([FromRoute] Guid id)
        {
            var request = new DeleteCustomer.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get customer upload template
        /// </summary>
        /// <param name="templateGenerator"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerUploadTemplate([FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(BulkImportType.Customer, CompanyId.GetValueOrDefault());

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// upload bulk customer
        /// </summary>
        /// <param name="file"></param>
        /// <param name="excelReader"></param>
        /// <returns></returns>
        [HttpPost("upload-bulk")]
        [ProducesResponseType(typeof(AddBulkCustomer.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddCustomer)]
        public async Task<IActionResult> UploadBulkCustomer(IFormFile file, [FromServices] IExcelReader excelReader)
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
                if (excelData.Columns.Count == 0 || excelData.Columns[6].ColumnName != "BusinessName")
                {
                    result.ErrorMessage = "Use the template provided";
                    return CommandResponse(result);
                }
            }

            var jsonString = excelReader.SerializeDataTableToJSON(excelData);
            var customers = JsonSerializer.Deserialize<List<AddBulkCustomer.CustomerModel>>(jsonString);
            if (customers == null) return CommandResponse(result);

            if (customers.Any(x => x.CustomerName.IsNullOrEmpty()
                                   || x.BusinessName.IsNullOrEmpty() || x.PhoneNumber.IsNullOrEmpty()
                                   || x.EmailAddress.IsNullOrEmpty() || x.Gender.IsNullOrEmpty()
                                   || x.OperatingSector.IsNullOrEmpty()))
            {
                result.ErrorMessage = "Fill all required fields";
                return CommandResponse(result);
            }
            
            var request = new AddBulkCustomer.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Customers = customers,
                UserId = UserId.GetValueOrDefault()
            };

            result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        ///// <summary>
        ///// export customers  (export Type should be excel)
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //[HttpGet("export")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        //[ValidateModel]
        //[HasPermission(Permissions.ExportCustomer)]
        //public async Task<IActionResult> ExportCustomers([FromQuery] string exportType, [FromQuery] ExportCustomers.Query message, [FromServices] IExcelGenerator excelGenerator)
        //{
        //    if (exportType is not ("excel")) return BadRequest(new { ErrorMessage = "Invalid Export Type" });
        //    if (message == null) message = new ExportCustomers.Query();

        //    message.CompanyId = CompanyId.GetValueOrDefault();
        //    message.UserId = UserId.GetValueOrDefault();
        //    var result = await _mediator.Send(message);

        //    var (stream, fileName) = await excelGenerator.Generate(result.CompanyName, Constants.GetCurrentDateTime().ToLongDateString(), message, result.Items);

        //    return ExportResponse(exportType, stream, fileName);
        //}

        /// <summary>
        /// get all customer addresses 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{customerId}/address")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetCustomerAddresses.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewCustomers)]
        public async Task<IActionResult> GetCustomerAddresses([FromRoute] Guid customerId, [FromQuery] GetCustomerAddresses.Query request)
        {
            if (request == null) request = new GetCustomerAddresses.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.CustomerId = customerId;

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// add customer address
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{customerId}/address")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddCustomerAddress.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateCustomer)]
        [ValidateModel]
        public async Task<IActionResult> AddCustomerAddress([FromRoute] Guid customerId, [FromBody] AddCustomerAddress.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.CustomerId = customerId;
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update customer address
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{addressId}/address")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateCustomerAddress.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateCustomer)]
        [ValidateModel]
        public async Task<IActionResult> UpdateCustomerAddress([FromRoute] Guid addressId, [FromBody] UpdateCustomerAddress.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = addressId;

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete customer address
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpDelete("{addressId}/address")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeleteCustomerAddress.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateCustomer)]
        public async Task<IActionResult> DeleteCustomerAddress([FromRoute] Guid addressId)
        {
            var request = new DeleteCustomerAddress.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = addressId
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
    }
}
