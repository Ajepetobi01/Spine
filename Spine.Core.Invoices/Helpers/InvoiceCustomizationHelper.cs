using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spine.Services.HttpClients;

namespace Spine.Core.Invoices.Helpers
{
    public class InvoiceCustomizationViewModel
    {
        public Guid Id { get; set; }
        public string BannerBase64 { get; set; }
        public string CompanyLogoBase64 { get; set; }
        public string SignatureBase64 { get; set; }
    }

    public interface IInvoiceCustomizationHelper
    {
        Task<InvoiceCustomizationViewModel> GetCustomizationBase64(Guid? bannerImageId, Guid? logoImageId, Guid? signatureImageId);
    }

    public class InvoiceCustomizationHelper : IInvoiceCustomizationHelper
    {
        private readonly ApiCaller _apiCaller;
        private readonly string uploadServiceUrl;
        public InvoiceCustomizationHelper(ApiCaller apiCaller, IConfiguration config)
        {
            _apiCaller = apiCaller;
            uploadServiceUrl = config["SpineUploadService"];
        }

        public async Task<InvoiceCustomizationViewModel> GetCustomizationBase64(Guid? bannerImageId, Guid? logoImageId, Guid? signatureImageId)
        {
            try
            {
                string url = $"{uploadServiceUrl}api/invoice-customization";
                var param = "?";

                if (bannerImageId.HasValue) param += $"bannerImageId={bannerImageId}&";
                if (bannerImageId.HasValue) param += $"logoImageId={logoImageId}&";
                if (bannerImageId.HasValue) param += $"signatureImageId={signatureImageId}";

                var response = await _apiCaller.Get<InvoiceCustomizationViewModel>($"{url}{param}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var successModel = ((ApiSuccessModel<InvoiceCustomizationViewModel>)response);
                    return successModel.Model;
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }

    }
}
