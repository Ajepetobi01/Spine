using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spine.Core.Invoices.Queries;
using Invoices.Api.Filters;
using Invoices.Api.Authorizations;
using Spine.Common.Enums;
using Spine.Core.Invoices.Commands;
//using ExcelCsvExport.Helpers;
using System.Net;
using System.IO;
using Spine.Common.ActionResults;
using Spine.Common.Extensions;
using Spine.Services;
using System.Collections.Generic;
using Spine.Common.Models;

namespace Invoices.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public class InvoiceController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvoiceController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public InvoiceController(IMediator mediator, ILogger<InvoiceController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get invoice stat
        /// </summary>
        /// <returns></returns>
        [HttpGet("dashboard")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoiceStat.Response))]
        [ProducesResponseType(typeof(GetInvoiceStat.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInvoiceStat()
        {
            var request = new GetInvoiceStat.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get all invoices
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoices.Response))]
        [ProducesResponseType(typeof(GetInvoices.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ValidateModel]
        [HasPermission(Permissions.ViewInvoice)]
        public async Task<IActionResult> GetInvoices([FromQuery] GetInvoices.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get all customer invoices
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("customer/{customerId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoices.Response))]
        [ProducesResponseType(typeof(GetInvoices.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ValidateModel]
        [HasPermission(Permissions.ViewInvoice)]
        public async Task<IActionResult> GetCustomerInvoices([FromRoute] Guid customerId, [FromQuery] GetInvoices.Query request)
        {
            request.CustomerId = customerId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get single invoice by id
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{invoiceId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoice.Response))]
        [ProducesResponseType(typeof(GetInvoice.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ValidateModel]
        [HasPermission(Permissions.ViewInvoice)]
        public async Task<IActionResult> GetInvoiceById([FromRoute] Guid invoiceId, [FromQuery] GetInvoice.Query request)
        {
            request.InvoiceId = invoiceId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get invoice via a share link (anonymous)
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        [HttpGet("{invoiceId}/share")]
        [AllowAnonymous]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetInvoiceForAnonymousShare.Response))]
        [ProducesResponseType(typeof(GetInvoiceForAnonymousShare.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInvoiceForAnonymousShare([FromRoute] Guid invoiceId)
        {
            var request = new GetInvoiceForAnonymousShare.Query
            {
                InvoiceId = invoiceId
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// download invoice preview (doesn't save the invoice, just returns the pdf)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("download-preview")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(FileResult))]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AddInvoice)]
        public async Task<IActionResult> DownloadInvoicePreview([FromBody] DownloadInvoicePreview.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();

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
                return File(stream, "application/octet-steam", $"Invoice-preview.pdf");
            }

            return CommandResponse(result);
        }

        /// <summary>
        /// add invoice 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddInvoice.Response))]
        [ProducesResponseType(typeof(AddInvoice.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AddInvoice)]
        public async Task<IActionResult> AddInvoice([FromBody] AddInvoice.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// send invoice 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}/send")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(SendInvoice.Response))]
        [ProducesResponseType(typeof(SendInvoice.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> SendInvoice([FromRoute] Guid id, [FromBody] SendInvoice.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = id;
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// download invoice 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("{id}/download")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(FileResult))]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.ViewInvoice)]
        public async Task<IActionResult> DownloadInvoice([FromRoute] Guid id, [FromQuery] DownloadInvoice.Command request)
        {
            if (request == null) request = new DownloadInvoice.Command();

            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = id;

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
                return File(stream, "application/octet-steam", $"Invoice-{result.InvoiceNo}.pdf");
            }

            return CommandResponse(result);
        }

        /// <summary>
        ///  send invoice to multiple contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{id}/send-to-multiple")]
        // [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(SendInvoiceToMultiple.Response))]
        [ProducesResponseType(typeof(SendInvoice.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> SendInvoiceToMultiple([FromRoute] Guid id, [FromForm] SendInvoiceToMultiple.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            request.Id = id;

            var result = new BasicActionResult();
            if (!request.Attachments.IsNullOrEmpty())
            {
                var toAttach = new List<AttachmentModel>();
                foreach (var item in request.Attachments)
                {
                    //var extension = "." + item.FileName.Split('.')[item.FileName.Split('.').Length - 1];
                    //if (extension == ".exe" || extension == ".lnk")
                    //{
                    //    result.ErrorMessage = "Upload file not allowed";
                    //    return CommandResponse(result);
                    //}
                    using var stream = new MemoryStream();
                    item.CopyTo(stream);
                    stream.Position = 0;
                    stream.Seek(0, SeekOrigin.Begin);

                    toAttach.Add(new AttachmentModel { fileStream = stream, fileName = item.FileName });
                }

                request.ConvertedAttachments = toAttach;
            }

            result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// cancel invoice 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(CancelInvoice.Response))]
        [ProducesResponseType(typeof(CancelInvoice.Response), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.CancelInvoice)]
        public async Task<IActionResult> DeleteInvoice([FromRoute] Guid id)
        {
            var request = new CancelInvoice.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        ///// <summary>
        ///// export invoice (export Type should be excel)
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpGet("export")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //[ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        //[ValidateModel]
        //[HasPermission(Permissions.ExportInvoice)]
        //public async Task<IActionResult> ExportInvoice([FromQuery] string exportType, [FromQuery] ExportInvoices.Query request, [FromServices] IExcelGenerator excelGenerator)
        //{
        //    if (exportType is not ("excel")) return BadRequest(new { ErrorMessage = "Invalid Export Type" });
        //    if (request == null) request = new ExportInvoices.Query();

        //    request.CompanyId = CompanyId.GetValueOrDefault();
        //    request.UserId = UserId.GetValueOrDefault();
        //    var result = await _mediator.Send(request);

        //    var (stream, fileName) = await excelGenerator.Generate(result.CompanyName, Constants.GetCurrentDateTime().ToLongDateString(), request, result.Items);

        //    return ExportResponse(exportType, stream, fileName);

        //}

    }
}
