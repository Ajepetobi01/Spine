using System;
using System.Text.Json.Serialization;

namespace Spine.Data.Documents.ViewModels
{

    public class BaseUploadModel
    {
        [JsonIgnore]
        public Guid CompanyId { get; set; }
        [JsonIgnore]
        public Guid UserId { get; set; }

        public string Base64string { get; set; }
    }

    public class UploadModel : BaseUploadModel
    {
        public string FileType { get; set; }
        public string DocumentName { get; set; }
    }

    public class InvoiceCustomizationViewModel
    {
        public Guid Id { get; set; }
        public string BannerBase64 { get; set; }
        public string CompanyLogoBase64 { get; set; }
        public string SignatureBase64 { get; set; }
    }

    public class CustomizationBanner
    {
        public Guid Id { get; set; }
        public string BannerBase64 { get; set; }
    }
}
