using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Helpers;
using Spine.Data.Documents.Service.Interfaces;
using Spine.Data.Documents.ViewModels;

namespace Spine.Data.Documents.Service
{
    public class UploadService : IUploadService
    {
        private readonly UploadsDbContext _context;
        public UploadService(UploadsDbContext context)
        {
            _context = context;
        }
        public async Task<UploadModel> GetUpload(Guid companyId, string documentId)
        {
            try
            {
                var data = await _context.Documents.Where(x => x.CompanyId == companyId && x.Id.ToString() == documentId).Select(x => new UploadModel
                {
                    Base64string = x.Base64string,
                    FileType = x.FileType,
                    DocumentName = x.DocumentName
                }).SingleOrDefaultAsync();

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GetUploadBase64(string documentId)
        {
            try
            {
                var data = await _context.Documents.Where(x => x.Id.ToString() == documentId).Select(x => x.Base64string).SingleOrDefaultAsync();

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> SaveUpload(UploadModel model)
        {
            try
            {
                var id = Guid.NewGuid();
                _context.Documents.Add(new Models.Document
                {
                    CompanyId = model.CompanyId,
                    DocumentName = model.DocumentName,
                    Base64string = model.Base64string,
                    FileType = model.FileType,
                    Id = id,
                    CreatedOn = Constants.GetCurrentDateTime(),
                    CreatedBy = model.UserId
                });

                return await _context.SaveChangesAsync() > 0 ? id.ToString() : "";

            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
