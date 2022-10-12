using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
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
using Spine.Core.Inventories.Commands.Vendor;
using Spine.Core.Inventories.Queries.Order;
using Spine.Core.Inventories.Queries.Vendor;
using Spine.Services;

namespace Inventory.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/vendor")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class VendorController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VendorController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public VendorController(IMediator mediator, ILogger<VendorController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region vendor
        
        /// <summary>
        /// get vendors
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetVendors.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewVendor)]
        public async Task<IActionResult> GetVendors([FromQuery]GetVendors.Query request)
        {
            request ??= new GetVendors.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get vendors slim (for dropdowns)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("slim")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetVendorsSlim.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        //[HasPermission(Permissions.ViewVendor)]
        public async Task<IActionResult> GetVendorSlim([FromQuery]GetVendorsSlim.Query request)
        {
            request ??= new GetVendorsSlim.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get vendor by id
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        [HttpGet("{vendorId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetVendor.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewVendor)]
        public async Task<IActionResult> GetVendorById(Guid vendorId)
        {
            var request = new GetVendor.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = vendorId
            };

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get vendor by id (slim)
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        [HttpGet("{vendorId}/slim")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetVendorSlim.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewVendor)]
        public async Task<IActionResult> GetVendorByIdSlim(Guid vendorId)
        {
            var request = new GetVendorSlim.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = vendorId
            };

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get vendors purchase orders
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{vendorId}/purchase-orders")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetPurchaseOrders.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> GetVendorPurchaseOrders(Guid vendorId, [FromQuery]GetPurchaseOrders.Query request)
        {
            request ??= new GetPurchaseOrders.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.VendorId = vendorId;
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get vendors received goods
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{vendorId}/received-goods")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetReceivedGoods.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewGoodsReceived)]
        public async Task<IActionResult> GetVendorReceivedGoods(Guid vendorId, [FromQuery]GetReceivedGoods.Query request)
        {
            request ??= new GetReceivedGoods.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.VendorId = vendorId;
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get vendors received goods for purchase order
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="orderId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{vendorId}/received-goods/{orderId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetReceivedGoods.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewGoodsReceived)]
        public async Task<IActionResult> GetVendorReceivedGoodsForPO(Guid vendorId, Guid orderId, [FromQuery]GetReceivedGoods.Query request)
        {
            request ??= new GetReceivedGoods.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.VendorId = vendorId;
            request.OrderId = orderId;
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        
        /// <summary>
        /// add vendor
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddVendor.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddVendor)]
        [ValidateModel]
        public async Task<IActionResult> AddVendor([FromBody]AddVendor.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update vendor
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{vendorId}")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateVendor.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateVendor)]
        public async Task<IActionResult> UpdateVendor([FromRoute] Guid vendorId, [FromBody] UpdateVendor.Command request)
        {
            request.Id = vendorId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update vendor status
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{vendorId}/update-status")]
        //[ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateVendorStatus.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateVendor)]
        public async Task<IActionResult> UpdateVendorStatus([FromRoute] Guid vendorId)
        {
            var request = new UpdateVendorStatus.Command
            {
                Id = vendorId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        /// <summary>
        /// delete vendor 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteVendor.Response))]
        [ProducesResponseType(typeof(DeleteVendor.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        //      [ValidateModel]
        [HasPermission(Permissions.DeleteVendor)]
        public async Task<IActionResult> DeleteVendor([FromRoute] Guid id)
        {
            var request = new DeleteVendor.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get vendor upload template
        /// </summary>
        /// <param name="vendorType"></param>
        /// <param name="templateGenerator"></param>
        /// <returns></returns>
        [HttpGet("{vendorType}/bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVendorUploadTemplate(TypeOfVendor vendorType, [FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var bulkImport = BulkImportType.BusinessVendor;
            if (vendorType == TypeOfVendor.Individual)
            {
                bulkImport = BulkImportType.IndividualVendor;
            }
            
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(bulkImport, CompanyId.GetValueOrDefault());

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// upload bulk vendor
        /// </summary>
        /// <param name="file"></param>
        /// <param name="excelReader"></param>
        /// <returns></returns>
        [HttpPost("upload-bulk")]
        [ProducesResponseType(typeof(AddBulkVendor.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddVendor)]
        public async Task<IActionResult> UploadBulkVendor(IFormFile file, [FromServices] IExcelReader excelReader)
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
                
                if (excelData.Columns.Count == 0) //|| excelData.Columns[5].ColumnName != "BusinessName")
                {
                    result.ErrorMessage = "Use the template provided";
                    return CommandResponse(result);
                }
            }

            var jsonString = excelReader.SerializeDataTableToJSON(excelData);
            var vendors = JsonSerializer.Deserialize<List<AddBulkVendor.VendorModel>>(jsonString);
            if (vendors == null) return CommandResponse(result);

            if (vendors.Any(x => x.FullName.IsNullOrEmpty()
                                 || x.DisplayName.IsNullOrEmpty() || x.PhoneNumber.IsNullOrEmpty()
                                 || x.EmailAddress.IsNullOrEmpty()
                                 || x.OperatingSector.IsNullOrEmpty()))
            {
                result.ErrorMessage = "Fill all required fields";
                return CommandResponse(result);
            }
            
            var request = new AddBulkVendor.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Vendors = vendors,
                UserId = UserId.GetValueOrDefault()
            };

            result = await _mediator.Send(request);
            return CommandResponse(result);
            
        }

        #endregion
        
        #region payments
        
        /// <summary>
        /// get vendor payments (pass in a vendorId to get only for a specific vendor)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("payments")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetVendorPayments.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
       [HasPermission(Permissions.ViewVendorPayment)]
        public async Task<IActionResult> GetVendorPayments([FromQuery]GetVendorPayments.Query request)
        {
            request ??= new GetVendorPayments.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// add vendor payment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("add-payment")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddVendorPayment.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddVendorPayment)]
        public async Task<IActionResult> ConfirmGoodsReceipt(AddVendorPayment.AddPaymentModel model)
        {
            var request = new AddVendorPayment.Command
            {
                UserId = UserId.GetValueOrDefault(),
                CompanyId = CompanyId.GetValueOrDefault(),
                Model = model
            };

            var result = await _mediator.Send(request);
            return CommandResponse(result);

        }
        
        #endregion

        #region Address

         /// <summary>
        /// add vendor address
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{vendorId}/address")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddVendorAddress.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateVendor)]
        [ValidateModel]
        public async Task<IActionResult> AddVendorAddress([FromRoute] Guid vendorId, [FromBody] AddVendorAddress.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.VendorId = vendorId;
            request.UserId = UserId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update vendor address
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{addressId}/address")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdateVendor.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateVendor)]
        [ValidateModel]
        public async Task<IActionResult> UpdateVendorAddress([FromRoute] Guid addressId, [FromBody] UpdateVendorAddress.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = addressId;

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete vendor address
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpDelete("{addressId}/address")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeleteVendorAddress.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdateVendor)]
        public async Task<IActionResult> DeleteVendorAddress([FromRoute] Guid addressId)
        {
            var request = new DeleteVendorAddress.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = addressId
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
    

        #endregion
    }
}
