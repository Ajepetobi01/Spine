using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spine.Common.Models;
using Spine.Common.Extensions;
using Spine.Data.Documents.Service.Interfaces;
using Spine.Data.Documents.ViewModels;

namespace Spine.DocumentService.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/invoice-customization")]
    [AllowAnonymous]
    public class InvoiceCustomizationController : MyControllerBase
    {
        private readonly ILogger<InvoiceCustomizationController> _logger;
        private readonly IInvoiceCustomizationService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public InvoiceCustomizationController(ILogger<InvoiceCustomizationController> logger, IInvoiceCustomizationService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// get all banners
        /// </summary>
        /// <returns></returns>
        [HttpGet("banners")]
        [ProducesResponseType(typeof(List<CustomizationBanner>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBanners()
        {
            var data = await _service.GetInvoiceCustomizationBanners();

            return Ok(data);
        }

        /// <summary>
        /// get base64 of banner, logo and/or signature
        /// </summary>
        /// <param name="bannerImageId"></param>
        /// <param name="logoImageId"></param>
        /// <param name="signatureImageId"></param>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(InvoiceCustomizationViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomizationBase64([FromQuery] Guid bannerImageId, [FromQuery] Guid logoImageId, [FromQuery] Guid signatureImageId)
        {
            var data = await _service.GetCustomizationBase64(bannerImageId, logoImageId, signatureImageId);
            if (data == null) return NotFound();

            return Ok(data);
        }


        /// <summary>
        /// get banner base64
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        [HttpGet("banner/{imageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBannerBase64([FromRoute] Guid imageId)
        {
            var data = await _service.GetInvoiceBannerBase64(imageId);
            if (data.IsNullOrEmpty()) return NotFound();

            return Ok(data);
        }

        /// <summary>
        /// get company logog base64
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        [HttpGet("logo/{imageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyLogo([FromRoute] Guid imageId)
        {
            var data = await _service.GetInvoiceCompanyLogoBase64(imageId);
            if (data.IsNullOrEmpty()) return NotFound();

            return Ok(data);
        }

        /// <summary>
        /// get signature base 64
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        [HttpGet("signature/{imageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSignature([FromRoute] Guid imageId)
        {
            var data = await _service.GetInvoiceSignatureBase64(imageId);
            if (data.IsNullOrEmpty()) return NotFound();

            return Ok(data);
        }



        /// <summary>
        /// add new customization banner
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("banner")]
        [ValidateModel]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveCustomizationBanner([FromBody] BaseUploadModel model)
        {
            var save = await _service.SaveCustomizationBanner(model);
            if (save == Guid.Empty) return BadRequest("Could not save image");

            return Ok(save);
        }

        // /// <summary>
        // /// update customization banner
        // /// </summary>
        // /// <param name="id"></param>
        // /// <param name="model"></param>
        // /// <returns></returns>
        // [HttpPut("{id}/banner")]
        // [ValidateModel]
        // [AllowAnonymous]
        // [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        // [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        // public async Task<IActionResult> UpdateCustomizationBanner(Guid id, [FromBody] BaseUploadModel model)
        // {
        //     var save = await _service.UpdateCustomizationBanner(id, model);
        //     if (save == Guid.Empty) return BadRequest("Could not save image");
        //
        //     return Ok(save);
        // }

        /// <summary>
        /// upload logo for invoice
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("logo")]
        [ValidateModel]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveInvoiceCompanyLogo([FromBody] BaseUploadModel model)
        {
            model.CompanyId = CompanyId.GetValueOrDefault();
            model.UserId = UserId.GetValueOrDefault();

            var save = await _service.SaveInvoiceCompanyLogo(model);
            if (save == Guid.Empty) return BadRequest("Could not save image");

            return Ok(save);
        }

        /// <summary>
        /// upload invoice signature
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("signature")]
        [ValidateModel]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveInvoiceSignature([FromBody] BaseUploadModel model)
        {
            model.CompanyId = CompanyId.GetValueOrDefault();
            model.UserId = UserId.GetValueOrDefault();

            var save = await _service.SaveInvoiceSignature(model);
            if (save == Guid.Empty) return BadRequest("Could not save image");

            return Ok(save);
        }

    }
}
