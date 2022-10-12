using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spine.Core.Inventories.Queries;
using Spine.Core.Inventories.Commands;
using Spine.Common.Models;
using Inventory.Api.Filters;
using Inventory.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Data.Entities.Transactions;

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
    public class InventoryController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InventoryController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public InventoryController(IMediator mediator, ILogger<InventoryController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Inventory 

        /// <summary>
        /// get inventory stat
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryStat.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryStat()
        {
            var request = new GetInventoryStat.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get inventory list for dropdown
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("slim")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoriesSlim.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoriesSlim([FromQuery] GetInventoriesSlim.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        ///  <summary>
        /// update inventory status
        ///  </summary>
        ///  <param name="id"></param>
        ///  <param name="request"></param>
        ///  <returns></returns>
        [HttpPut("{id}/update-status")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateInventoryStatus.Response))]
        [ProducesResponseType(typeof(UpdateInventoryStatus.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> UpdateInventoryStatus(Guid id, UpdateInventoryStatus.Command request)
        {
            request.Id = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete inventory
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteInventory.Response))]
        [ProducesResponseType(typeof(DeleteInventory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.DeleteInventory)]
        public async Task<IActionResult> DeleteInventory(Guid id)
        {
            var request = new DeleteInventory.Command
            {
                Id = id,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get inventory QR code by inventoryId
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <returns></returns>
        [HttpGet("{inventoryId}/qrcode")]
        [AllowAnonymous]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryQRCode.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryQrCode([FromRoute] Guid inventoryId)
        {
            var request = new GetInventoryQRCode.Query
            {
                InventoryId = inventoryId
            };
            var result = await _mediator.Send(request);
            if (result == null) return NotFound("Inventory item not found");

            return new FileStreamResult(result.OutputStream, "image/jpeg");
        }
        
        ///  <summary>
        /// adjust inventory quantity (product) 
        ///  </summary>
        ///  <param name="request"></param>
        ///  <returns></returns>
        [HttpPut("adjust-quantity")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AdjustInventory.Response))]
        [ProducesResponseType(typeof(AdjustInventory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> AdjustInventoryQuantity(AdjustInventory.Command request)
        {
            request.AdjustmentType = AdjustmentType.Quantity;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }
        
        ///  <summary>
        /// adjust inventory cost (product & service)
        ///  </summary>
        ///  <param name="request"></param>
        ///  <returns></returns>
        [HttpPut("adjust-cost")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AdjustInventory.Response))]
        [ProducesResponseType(typeof(AdjustInventory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> AdjustInventoryCost(AdjustInventory.Command request)
        {
            request.AdjustmentType = AdjustmentType.Cost;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        #endregion

        #region Inventory Location

        /// <summary>
        /// get inventory locations for dropdown
        /// </summary>
        /// <returns></returns>
        [HttpGet("locations/slim")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryLocationsSlim.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryLocationsSlim()
        {
            var request = new GetInventoryLocationsSlim.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get inventory locations
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("locations")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryLocations.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewInventoryLocation)]
        public async Task<IActionResult> GetInventoryLocations([FromQuery] GetInventoryLocations.Query request)
        {
            if (request == null) request = new GetInventoryLocations.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get inventory location by id
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        [HttpGet("locations/{locationId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryLocation.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewInventoryLocation)]
        public async Task<IActionResult> GetInventoryLocation(Guid locationId)
        {
            var result = await _mediator.Send(new GetInventoryLocation.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = locationId
            });
            return QueryResponse(result);
        }


        /// <summary>
        /// add inventory location 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("locations")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddInventoryLocation.Response))]
        [ProducesResponseType(typeof(AddInventoryLocation.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AddInventoryLocation)]
        public async Task<IActionResult> AddInventoryLocation(AddInventoryLocation.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update inventory location
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("locations/{locationId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateInventoryLocation.Response))]
        [ProducesResponseType(typeof(UpdateInventoryLocation.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventoryLocation)]
        public async Task<IActionResult> UpdateInventoryLocation(Guid locationId, UpdateInventoryLocation.Command request)
        {
            request.Id = locationId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete inventory location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        [HttpDelete("locations/{locationId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteInventoryLocation.Response))]
        [ProducesResponseType(typeof(DeleteInventoryLocation.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.DeleteInventoryLocation)]
        public async Task<IActionResult> DeleteInventoryLocation(Guid locationId)
        {
            var request = new DeleteInventoryLocation.Command
            {
                Id = locationId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        #endregion

        #region Inventory Note

        /// <summary>
        /// add inventory note 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{id}/add-note")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddInventoryNote.Response))]
        [ProducesResponseType(typeof(AddInventoryNote.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> AddInventoryNote(Guid id, AddInventoryNote.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = id;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update inventory note
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("notes/{noteId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateInventoryNote.Response))]
        [ProducesResponseType(typeof(UpdateInventoryNote.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> UpdateInventoryNote(Guid noteId, UpdateInventoryNote.Command request)
        {
            request.Id = noteId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete inventory note
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns></returns>
        [HttpDelete("notes/{noteId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteInventoryNote.Response))]
        [ProducesResponseType(typeof(DeleteInventoryNote.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> DeleteInventoryNote(Guid noteId)
        {
            var request = new DeleteInventoryNote.Command
            {
                Id = noteId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        #endregion

        #region Ledger Accounts

        /// <summary>
        /// get sales accounts ledgers
        /// </summary>
        /// <returns></returns>
        [HttpGet("sales-ledgers")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryLedgerAccounts.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryLedgerAccounts_Sales()
        {
            var request = new GetInventoryLedgerAccounts.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                AccountTypeId = AccountTypeConstants.Income
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get cost of sales accounts ledgers
        /// </summary>
        /// <returns></returns>
        [HttpGet("cost-of-sales-ledgers")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryLedgerAccounts.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryLedgerAccounts_CostOfSales()
        {
            var request = new GetInventoryLedgerAccounts.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                AccountTypeId = AccountTypeConstants.CostOfSales
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get inventory accounts ledgers
        /// </summary>
        /// <returns></returns>
        [HttpGet("inventory-ledgers")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetInventoryLedgerAccounts.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryLedgerAccounts_Inventory()
        {
            var request = new GetInventoryLedgerAccounts.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                AccountTypeId = AccountTypeConstants.Inventories
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }


        #endregion
    }
}
