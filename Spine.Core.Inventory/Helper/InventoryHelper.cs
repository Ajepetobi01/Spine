using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spine.Common.Extensions;
using Spine.Common.Models;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Inventories;
using Spine.PdfGenerator;
using Spine.Services.HttpClients;

namespace Spine.Core.Inventories.Helper
{
    public interface IInventoryHelper
    {
        Task<(byte[], string)> GeneratePurchaseOrderPdf(IPdfGenerator pdfGenerator, SpineContext dbContext, Guid companyId, Guid orderId);
        
    }

    public class InventoryHelper : IInventoryHelper
    {
        private readonly ApiCaller _apiCaller;
        private readonly string uploadServiceUrl;
        public InventoryHelper(ApiCaller apiCaller, IConfiguration config)
        {
            _apiCaller = apiCaller;
            uploadServiceUrl = config["SpineUploadService"];
        }
        
        public async Task<(byte[], string)> GeneratePurchaseOrderPdf(IPdfGenerator pdfGenerator, SpineContext _dbContext, Guid companyId, Guid orderId)
        {
            var details = await (from ord in _dbContext.PurchaseOrders.Where(x => x.CompanyId == companyId && x.Id == orderId && !x.IsDeleted)
                                 join comp in _dbContext.Companies on ord.CompanyId equals comp.Id
                                 where !comp.IsDeleted
                                 join cur in _dbContext.Currencies on comp.BaseCurrencyId equals cur.Id
                                 select new { ord, comp, cur }).SingleOrDefaultAsync();

            var order = details.ord;
            var business = details.comp;

            if (order == null) return (null, string.Empty);

            var companyLogo = "";
            if (!business.LogoId.IsNullOrEmpty())
            {
                try
                {
                    var response = await _apiCaller.Get<string>($"{uploadServiceUrl}api/uploads/get-base64/{business.LogoId}");
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var successModel = ((ApiSuccessModel<string>)response);
                        companyLogo = successModel.Model;
                    }
                }
                catch (Exception ex)
                {
                }

            }

            var model = new PurchaseOrderPreview
            {
                VendorName = order.VendorName,
                Notes = order.AdditionalNote,
                ExpectedDate = order.ExpectedDate != DateTime.MinValue ? order.ExpectedDate : null,
                OrderDate = order.OrderDate,
                Amount = order.OrderAmount,
                CurrencySymbol = details.cur.Symbol,
                CompanyLogo = companyLogo,
                Business = new CompanyModel
                {
                    Name = business.Name,
                    City = business.City,
                    Address = business.Address,
                    Phone = business.PhoneNumber
                },
                LineItems = await _dbContext.LineItems
                    .Where(l => l.CompanyId == order.CompanyId && l.ParentItemId == order.Id)
                    .OrderBy(x => x.CreatedOn)
                    .Select(x => new OrderLineItem
                    {
                        Amount = x.Amount,
                        Item = x.Item,
                        Description = x.Description,
                        Quantity = x.Quantity,
                        Rate = x.Rate,
                    }).ToListAsync(),
            };

            var pdfBytes = await pdfGenerator.GeneratePdfByte("PurchaseOrder", model);
            return (pdfBytes, order.OrderDate.ToString("dd-MM-yyyy"));
        }
        
    }
}
