using System;
using System.Threading.Tasks;
using Spine.Data.Documents.ViewModels;

namespace Spine.Data.Documents.Service.Interfaces
{
    public interface IUploadService
    {
        Task<UploadModel> GetUpload(Guid companyId, string uploadId);
        Task<string> GetUploadBase64(string uploadId);
        Task<string> SaveUpload(UploadModel model);
    }
}
