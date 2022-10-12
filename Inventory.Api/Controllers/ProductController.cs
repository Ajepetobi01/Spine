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
using Spine.Core.Inventories.Commands.Product;
using Spine.Core.Inventories.Queries.Product;
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
    public class ProductController : MyControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public ProductController(IMediator mediator, ILogger<ProductController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("measurement-units")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetMeasurementUnits.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMeasurementUnits()
        {
            var result = await _mediator.Send(new GetMeasurementUnits.Query());
            return QueryResponse(result);
        }

        #region Product

        /// <summary>
        /// get products for dropdown
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("slim")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetProductsSlim.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductsSlim([FromQuery] GetProductsSlim.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }
        
        /// <summary>
        /// get products for dropdown in PO. returns whether tax should be applied
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("slim-for-po")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetProductsSlimForPO.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductsSlimForPO([FromQuery] GetProductsSlimForPO.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get products
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [ValidateModel]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetProducts.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProducts([FromQuery] GetProducts.Query request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get product by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetProductById.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var request = new GetProductById.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = id
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// add product
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddProduct.Response))]
        [ProducesResponseType(typeof(AddProduct.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AddInventory)]
        public async Task<IActionResult> AddProduct(AddProduct.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// get product upload template
        /// </summary>
        /// <param name="templateGenerator"></param>
        /// <returns></returns>
        [HttpGet("bulk-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductUploadTemplate([FromServices] IExcelTemplateGenerator templateGenerator)
        {
            var (stream, contentType, fileName) = await templateGenerator.GenerateTemplate(BulkImportType.Product, CompanyId.GetValueOrDefault());

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// upload bulk products
        /// </summary>
        /// <param name="file"></param>
        /// <param name="excelReader"></param>
        /// <returns></returns>
        [HttpPost("upload-bulk")]
        [ProducesResponseType(typeof(AddBulkProduct.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [HasPermission(Permissions.AddInventory)]
        public async Task<IActionResult> UploadBulkProduct(IFormFile file, [FromServices] IExcelReader excelReader)
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
                if (excelData.Columns.Count == 0 || excelData.Columns[3].ColumnName != "SerialNumber")
                {
                    result.ErrorMessage = "Use the template provided";
                    return CommandResponse(result);
                }
            }

            var jsonString = excelReader.SerializeDataTableToJSON(excelData);
            var products = JsonSerializer.Deserialize<List<AddBulkProduct.ProductModel>>(jsonString);
            if (products == null) return CommandResponse(result);
            
            if (products.Any(x => x.Name.IsNullOrEmpty()
                                || x.Category.IsNullOrEmpty() || x.SerialNumber.IsNullOrEmpty()
                                || x.Quantity < 1))
            {
                result.ErrorMessage = "Fill all required fields";
                return CommandResponse(result);
            }
            var request = new AddBulkProduct.Command
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Products = products,
                UserId = UserId.GetValueOrDefault()
            };

            result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateProduct.Response))]
        [ProducesResponseType(typeof(UpdateProduct.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventory)]
        public async Task<IActionResult> UpdateProduct(Guid id, UpdateProduct.Command request)
        {
            request.Id = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// restock product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}/restock")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(RestockProduct.Response))]
        [ProducesResponseType(typeof(RestockProduct.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.RestockInventory)]
        public async Task<IActionResult> RestockProduct(Guid id, RestockProduct.Command request)
        {
            request.InventoryId = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// allocate product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}/allocate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AllocateProduct.Response))]
        [ProducesResponseType(typeof(AllocateProduct.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AllocateInventory)]
        public async Task<IActionResult> AllocateProduct(Guid id, AllocateProduct.Command request)
        {
            request.InventoryId = id;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        ///get allocations for all product
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("allocations")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetProductAllocations.Response))]
        [ProducesResponseType(typeof(GetProductAllocations.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ValidateModel]
        [HasPermission(Permissions.ViewInventory)]
        public async Task<IActionResult> GetProductAllocations([FromQuery] GetProductAllocations.Query request)
        {
            if (request == null) request = new GetProductAllocations.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        ///  <summary>
        /// get allocations for a product by product id
        ///  </summary>
        ///  <param name="id"></param>
        ///  <param name="request"></param>
        ///  <returns></returns>
        [HttpGet("{id}/allocations")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(GetProductAllocations.Response))]
        [ProducesResponseType(typeof(GetProductAllocations.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ValidateModel]
        [HasPermission(Permissions.ViewInventory)]
        public async Task<IActionResult> GetProductAllocations(Guid id, [FromQuery] GetProductAllocations.Query request)
        {
            if (request == null) request = new GetProductAllocations.Query();

            request.ProductId = id;
            request.CompanyId = CompanyId.GetValueOrDefault();

            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        #endregion

        #region Product Category

        /// <summary>
        /// get product categories for dropdown
        /// </summary>
        /// <returns></returns>
        [HttpGet("categories/slim")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetProductCategoriesSlim.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductCategoriesSlim()
        {
            var request = new GetProductCategoriesSlim.Query
            {
                CompanyId = CompanyId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get product categories
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("categories")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetProductCategories.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewInventoryCategory)]
        public async Task<IActionResult> GetProductCategories([FromQuery] GetProductCategories.Query request)
        {
            if (request == null) request = new GetProductCategories.Query();

            request.CompanyId = CompanyId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return QueryResponse(result);
        }

        /// <summary>
        /// get product category by id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("categories/{categoryId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GetProductCategory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HasPermission(Permissions.ViewInventoryCategory)]
        public async Task<IActionResult> GetProductCategory(Guid categoryId)
        {
            var result = await _mediator.Send(new GetProductCategory.Query
            {
                CompanyId = CompanyId.GetValueOrDefault(),
                Id = categoryId
            });
            return QueryResponse(result);
        }


        /// <summary>
        /// add product category 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("categories")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(AddProductCategory.Response))]
        [ProducesResponseType(typeof(AddProductCategory.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.AddInventoryCategory)]
        public async Task<IActionResult> AddProductCategory(AddProductCategory.Command request)
        {
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// update product category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("categories/{categoryId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(UpdateProductCategory.Response))]
        [ProducesResponseType(typeof(UpdateProductCategory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventoryCategory)]
        public async Task<IActionResult> UpdateProductCategory(Guid categoryId, UpdateProductCategory.Command request)
        {
            request.Id = categoryId;
            request.CompanyId = CompanyId.GetValueOrDefault();
            request.UserId = UserId.GetValueOrDefault();
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// toggle status of product category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpPut("categories/{categoryId}/update-status")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(ToggleProductCategoryStatus.Response))]
        [ProducesResponseType(typeof(ToggleProductCategoryStatus.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.UpdateInventoryCategory)]
        public async Task<IActionResult> UpdateProductCategoryStatus(Guid categoryId)
        {
            var request = new ToggleProductCategoryStatus.Command
            {
                Id = categoryId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        /// <summary>
        /// delete product category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpDelete("categories/{categoryId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(typeof(DeleteProductCategory.Response))]
        [ProducesResponseType(typeof(DeleteProductCategory.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ValidateModel]
        [HasPermission(Permissions.DeleteInventoryCategory)]
        public async Task<IActionResult> DeleteProductCategory(Guid categoryId)
        {
            var request = new DeleteProductCategory.Command
            {
                Id = categoryId,
                CompanyId = CompanyId.GetValueOrDefault(),
                UserId = UserId.GetValueOrDefault()
            };
            var result = await _mediator.Send(request);
            return CommandResponse(result);
        }

        #endregion

    }
}
