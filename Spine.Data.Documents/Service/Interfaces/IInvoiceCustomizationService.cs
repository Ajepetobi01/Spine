using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spine.Data.Documents.ViewModels;

namespace Spine.Data.Documents.Service.Interfaces
{
    public interface IInvoiceCustomizationService
    {
        Task<List<CustomizationBanner>> GetInvoiceCustomizationBanners();

        Task<string> GetInvoiceBannerBase64(Guid bannerImageId);
        Task<string> GetInvoiceCompanyLogoBase64(Guid logoImageId);
        Task<string> GetInvoiceSignatureBase64(Guid signatureImageId);
        Task<InvoiceCustomizationViewModel> GetCustomizationBase64(Guid? bannerImageId, Guid? logoImageId, Guid? signatureImageId);

        Task<Guid> SaveCustomizationBanner(BaseUploadModel model);
        Task<Guid> UpdateCustomizationBanner(Guid id, BaseUploadModel model);
        Task<Guid> SaveInvoiceCompanyLogo(BaseUploadModel model);
        Task<Guid> SaveInvoiceSignature(BaseUploadModel model);

    }
}
