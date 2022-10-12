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
using Spine.Core.Inventories.Commands.Order;
using Spine.Core.Inventories.Queries.Order;
using Spine.Services;

namespace Inventory.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/purchase-order")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class PurchaseOrderController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PurchaseOrderController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public PurchaseOrderController(IMediator mediator, ILogger<PurchaseOrderController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region purchase order

        /// <summary>
        /// get purchase orders
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetPurchaseOrders.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> GetPurchaseOrders([FromQuery] GetPurchaseOrders.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get purchase order by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetPurchaseOrder.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> GetPurchaseOrderById(Guid id)
        {
            var request = new GetPurchaseOrder.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get available purchase orders for GR linking
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("open-orders")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetAvailablePurchaseOrders.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> GetAvailablePurchaseOrders([FromQuery] GetAvailablePurchaseOrders.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// add purchase order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddPurchaseOrder.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.CreatePurchaseOrder)]
        [ValidateModel]
        public async Task<IActionResult> AddPurchaseOrder([FromBody]AddPurchaseOrder.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// add and send purchase order to vendor
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("send")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddPurchaseOrder.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        [ValidateModel]
        public async Task<IActionResult> AddAndSendPurchaseOrder([FromBody]AddPurchaseOrder.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.Send = true;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update purchase order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdatePurchaseOrder.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdatePurchaseOrder)]
        [ValidateModel]
        public async Task<IActionResult> UpdatePurchaseOrder(Guid id, [FromBody]UpdatePurchaseOrder.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.PurchaseOrderId = id;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete purchase order
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeletePurchaseOrder.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.DeletePurchaseOrder)]
        public async Task<IActionResult> DeletePurchaseOrder(Guid id)
        {
            var request = new DeletePurchaseOrder.Command
            {
                UserId = UserId.GetValueOrDefault(),
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// download pruchase order 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/download")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(FileResult))]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> DownloadPurchaseOrder(Guid id)
        {
            var request = new DownloadPurchaseOrder.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = id
            };

            var result = await _mediator.Send(request);
            if (result.Status == HttpStatusCode.OK)
            {
                if (result.PdfByte.IsNullOrEmpty())
                {
                    result.ErrorMessage = "An error occured while generating the pdf";
                    return CommandResponse(result);
                }

                var stream = new MemoryStream(result.PdfByte);
                stream.Position = 0;
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/octet-steam", $"PurchaseOrder-{result.OrderDate}.pdf");
            }

            return CommandResponse(result);
        }

        /// <summary>
        /// get purchase order via a share link (anonymous)
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("{orderId}/share")]
        [AllowAnonymous]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetPurchaseOrderForAnonymousShare.Response))]
        [ProducesResponseType(typeof(GetPurchaseOrderForAnonymousShare.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPurchaseOrderForAnonymousShare([FromRoute] Guid orderId)
        {
            var request = new GetPurchaseOrderForAnonymousShare.Query
            {
                OrderId = orderId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// send purchase order to vendor email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/send")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(SendPurchaseOrder.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> SendPurchaseOrder(Guid id)
        {
            var request = new SendPurchaseOrder.Command
            {
                UserId = UserId.GetValueOrDefault(),
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// send purchase order to multiple contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{id}/send-to-multiple")]
        // [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(SendPurchaseOrderToMultiple.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> SendPurchaseOrderToMultiple([FromRoute] Guid id, [FromForm] SendPurchaseOrderToMultiple.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.Id = id;

            if (!request.Attachments.IsNullOrEmpty())
            {
                var toAttach = new List<AttachmentModel>();
                foreach (var item in request.Attachments)
                {
                    using var stream = new MemoryStream();
                    item.CopyTo(stream);
                    stream.Position = 0;
                    stream.Seek(0, SeekOrigin.Begin);

                    toAttach.Add(new AttachmentModel { fileStream = stream, fileName = item.FileName });
                }

                request.ConvertedAttachments = toAttach;
            }

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get purchase order upload template
        /// </summary>
        /// <param name="templateGenerator"></param>
        /// <returns></returns>
        [HttpGet("bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPurchaseOrderUploadTemplate([FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(BulkImportType.PurchaseOrder, CompanyId.GetValueOrDefault());

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// upload bulk purchase order
        /// </summary>
        /// <param name="file"></param>
        /// <param name="excelReader"></param>
        /// <returns></returns>
        [HttpPost("upload-bulk")]
        [ProducesResponseType(typeof(AddBulkPurchaseOrder.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.CreatePurchaseOrder)]
        public async Task<IActionResult> AddBulkPurchaseOrder(IFormFile file, [FromServices] IExcelReader excelReader)
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
                if (excelData.Columns.Count == 0 || excelData.Columns[0].ColumnName != "VendorName")
                {
                    result.ErrorMessage = "Use the template provided";
                    return CommandResponse(result);
                }
            }

            var jsonString = excelReader.SerializeDataTableToJSON(excelData);
            var orders = JsonSerializer.Deserialize<List<AddBulkPurchaseOrder.PurchaseOrderModel>>(jsonString);
            if (orders == null) return CommandResponse(result);
            
            if (orders.Any(x => x.Vendor.IsNullOrEmpty()
                                || x.Product.IsNullOrEmpty() || x.Quantity < 1
                                || x.OrderDate == null))
            {
                result.ErrorMessage = "Fill all required fields";
                return CommandResponse(result);
            }
            
            var request = new AddBulkPurchaseOrder.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                OrderItems = orders,
                UserId = UserId.GetValueOrDefault()
            };

            result = await _mediator.Send(request);
            return CommandResponse(result);

        }
        #endregion

        #region purchase order line items

        /// <summary>
        /// add item to purchase order
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{orderId}/items")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddPurchaseOrderLineItem.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdatePurchaseOrder)]
        [ValidateModel]
        public async Task<IActionResult> AddPurchaseOrderLineItem(Guid orderId, [FromBody]AddPurchaseOrderLineItem.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.PurchaseOrderId = orderId;

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        /// <summary>
        /// update purchase order line item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("items/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UpdatePurchaseOrderLineItem.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdatePurchaseOrder)]
        [ValidateModel]
        public async Task<IActionResult> UpdatePurchaseOrderLineItem(Guid id, [FromBody]UpdatePurchaseOrderLineItem.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.Id = id;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete purchase order line item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("items/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(DeletePurchaseOrderLineItem.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.UpdatePurchaseOrder)]
        public async Task<IActionResult> DeletePurchaseOrderItems(Guid id)
        {
            var request = new DeletePurchaseOrderLineItem.Command
            {
                UserId = UserId.GetValueOrDefault(),
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        #endregion

        #region received goods

        /// <summary>
        /// get purchase order by id for confirm receipt
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("{orderId}/confirm-receipt")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetPurchaseOrderForConfirmReceipt.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ConfirmGoodsReceived)]
        public async Task<IActionResult> GetPurchaseOrderForConfirmReceipt(Guid orderId)
        {
            var request = new GetPurchaseOrderForConfirmReceipt.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = orderId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get purchase order with received items
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("{orderId}/po-and-received")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetPurchaseOrderWithReceivedItems.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewPurchaseOrder)]
        public async Task<IActionResult> GetPurchaseOrderWithReceivedItems(Guid orderId)
        {
            var request = new GetPurchaseOrderWithReceivedItems.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = orderId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// confirm goods receipt (from PO)
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{orderId}/confirm-receipt")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConfirmGoodsReceived.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ConfirmGoodsReceived)]
        [ValidateModel]
        public async Task<IActionResult> ConfirmGoodsReceipt(Guid orderId, [FromBody]ConfirmGoodsReceived.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.PurchaseOrderId = orderId;

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        /// <summary>
        /// get received goods
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("received-goods")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetReceivedGoods.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewGoodsReceived)]
        public async Task<IActionResult> GetReceivedGoods([FromQuery]GetReceivedGoods.Query request)
        {
            request ??= new GetReceivedGoods.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get received goods for purchase order
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{orderId}/received-goods")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetReceivedGoods.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewGoodsReceived)]
        public async Task<IActionResult> GetReceivedGoodsForPO(Guid orderId, [FromQuery]GetReceivedGoods.Query request)
        {
            request ??= new GetReceivedGoods.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.OrderId = orderId;
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// receive goods (without PO)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("receive-goods")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConfirmGoodsReceived.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ConfirmGoodsReceived)]
        [ValidateModel]
        public async Task<IActionResult> ReceiveGoods([FromBody] AddGoodsReceived.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get received goods for return
        /// </summary>
        /// <param name="goodsReceivedId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{goodsReceivedId}/received-goods-for-return")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetReceivedGoods.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ReturnGoodsReceived)]
        public async Task<IActionResult> GetReceivedGoodsForReturn([FromQuery] Guid goodsReceivedId, [FromQuery]GetReceivedGoodsForReturn.Query request)
        {
            request ??= new GetReceivedGoodsForReturn.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.GoodsReceivedId = goodsReceivedId;
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// return received goods
        /// </summary>
        /// <param name="goodsReceivedId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{goodsReceivedId}/return")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConfirmGoodsReceived.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ReturnGoodsReceived)]
        [ValidateModel]
        public async Task<IActionResult> ReturnGoodsReceived(Guid goodsReceivedId, [FromBody] List<ReturnGoodsReceived.ReturnModel> request)
        {
            var req = new ReturnGoodsReceived.Command
            {
                UserId = UserId.GetValueOrDefault(),
                CompanyId = CompanyId.GetValueOrDefault(),
                GoodsReceivedId = goodsReceivedId,
                Data = request
            };

            var result = await _mediator.Send(req);
            return CommandResponse(result);
        }
        
        /// <summary>
        /// link goods received to PO
        /// </summary>
        /// <param name="goodsReceivedId"></param>
        /// <returns></returns>
        [HttpPost("{goodsReceivedId}/link-to-order")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(LinkGoodsReceivedToPurchaseOrder.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.ConfirmGoodsReceived)]
        [ValidateModel]
        public async Task<IActionResult> LinkGoodsReceivedToPurchaseOrder(Guid goodsReceivedId, [FromBody] LinkGoodsReceivedToPurchaseOrder.Command request)
        {
            request.UserId = UserId.GetValueOrDefault();
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.GoodsReceivedId = goodsReceivedId;
            
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        #endregion
    }
}
