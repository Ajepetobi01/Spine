using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Common.Extensions;
using Spine.Common.Enums;

namespace Inventory.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api")]
    [AllowAnonymous]
    public class CommonController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CommonController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public CommonController(IMediator mediator, ILogger<CommonController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// vendor type
        /// </summary>
        /// <returns></returns>
        [HttpGet("vendor-type")]
        public IActionResult VendorType()
        {
            return Ok(EnumExtensions.GetValues<TypeOfVendor>());
        }

        /// <summary>
        /// purchase order status
        /// </summary>
        /// <returns></returns>
        [HttpGet("order-status")]
        public IActionResult PurchaseOrderStatus()
        {
            return Ok(EnumExtensions.GetValues<PurchaseOrderStatus>());
        }

        /// <summary>
        /// inventory status
        /// </summary>
        /// <returns></returns>
        [HttpGet("inventory-status")]
        public IActionResult InventoryStatus()
        {
            return Ok(EnumExtensions.GetValues<InventoryStatus>());
        }
        
        /// <summary>
        /// status (active/inactive)
        /// </summary>
        /// <returns></returns>
        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(EnumExtensions.GetValues<Status>());
        }
    }
}
