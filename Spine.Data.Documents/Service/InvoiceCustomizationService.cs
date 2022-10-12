using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json.Serialization;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Data.Documents.Service.Interfaces;
using Spine.Data.Documents.ViewModels;

namespace Spine.Data.Documents.Service
{
    public class InvoiceCustomizationService : IInvoiceCustomizationService
    {
        private readonly UploadsDbContext _context;
        private readonly IDistributedCache _distributedCache;
        public InvoiceCustomizationService(UploadsDbContext context, IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<InvoiceCustomizationViewModel> GetCustomizationBase64(Guid? bannerImageId, Guid? logoImageId, Guid? signatureImageId)
        {
            try
            {
                var bannerCache = Constants.BannerCache + bannerImageId;
                var signatureCache = Constants.SignatureCache + signatureImageId;
                var logoCache = Constants.LogoCache + logoImageId;

                var banner64 = "";
                var logo64 = "";
                var signature64 = "";

                if (bannerImageId.HasValue)
                {
                    var cachedBanner = await _distributedCache.GetAsync(bannerCache);
                    if (cachedBanner != null)
                    {
                        banner64 = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(cachedBanner));
                    }
                }

                if (signatureImageId.HasValue)
                {
                    var cachedSignature = await _distributedCache.GetAsync(signatureCache);
                    if (cachedSignature != null)
                    {
                        signature64 = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(cachedSignature));
                    }
                }

                if (logoImageId.HasValue)
                {
                    var cachedLogo = await _distributedCache.GetAsync(logoCache);
                    if (cachedLogo != null)
                    {
                        logo64 = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(cachedLogo));
                    }
                }

                if ((bannerImageId.HasValue && banner64.IsNullOrEmpty())
                    || (signatureImageId.HasValue && signature64.IsNullOrEmpty())
                    || (logoImageId.HasValue && logo64.IsNullOrEmpty()))
                {
                    var data = await (from banner in _context.InvoiceBanners.Where(x =>
                            !bannerImageId.HasValue || x.Id == bannerImageId)
                        join logo in _context.CompanyInvoiceLogos on logoImageId equals logo.Id into logos
                        from logo in logos.DefaultIfEmpty()
                        join sign in _context.CompanyInvoiceSignatures on signatureImageId equals sign.Id into signs
                        from sign in signs.DefaultIfEmpty()
                        select new InvoiceCustomizationViewModel
                        {
                            BannerBase64 = banner.Base64string ?? "",
                            SignatureBase64 = sign.Base64string ?? "",
                            CompanyLogoBase64 = logo.Base64string ?? ""
                        }).FirstOrDefaultAsync();

                    var options = new DistributedCacheEntryOptions();
                    //.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    // .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    await _distributedCache.SetAsync(bannerCache,
                        Encoding.UTF8.GetBytes( JsonSerializer.Serialize(data.BannerBase64)), options);
                    await _distributedCache.SetAsync(signatureCache,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data.SignatureBase64)), options);
                    await _distributedCache.SetAsync(logoCache,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data.CompanyLogoBase64)), options);

                    return data;
                }

                return new InvoiceCustomizationViewModel
                {
                    BannerBase64 = banner64,
                    CompanyLogoBase64 = logo64,
                    SignatureBase64 = signature64,
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CustomizationBanner>> GetInvoiceCustomizationBanners()
        {
            try
            {
                List<CustomizationBanner> data;
                string serializedData;
            
                var cacheData = await _distributedCache.GetAsync(Constants.AllBannerCache);
                if (cacheData != null)
                {
                    serializedData = Encoding.UTF8.GetString(cacheData);
                    data = JsonSerializer.Deserialize<List<CustomizationBanner>>(serializedData);
                }
                else
                {
                    data = await _context.InvoiceBanners.Select(x =>
                        new CustomizationBanner
                        {
                            BannerBase64 = x.Base64string,
                            Id = x.Id
                        }).ToListAsync();

                    serializedData = JsonSerializer.Serialize(data);
                    cacheData = Encoding.UTF8.GetBytes(serializedData);

                    var options = new DistributedCacheEntryOptions();
                    //.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    // .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    await _distributedCache.SetAsync(Constants.AllBannerCache, cacheData, options);
                }

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GetInvoiceBannerBase64(Guid bannerImageId)
        {
            try
            {
                string data;
                var cacheKey = Constants.BannerCache + bannerImageId;
                string serializedData;

                var cacheData = await _distributedCache.GetAsync(cacheKey);
                if (cacheData != null)
                {
                    serializedData = Encoding.UTF8.GetString(cacheData);
                    data = JsonSerializer.Deserialize<string>(serializedData);
                }
                else
                {
                    data = await _context.InvoiceBanners.Where(x => x.Id == bannerImageId)
                        .Select(x => x.Base64string)
                        .SingleOrDefaultAsync();

                    serializedData = JsonSerializer.Serialize(data);
                    cacheData = Encoding.UTF8.GetBytes(serializedData);

                    var options = new DistributedCacheEntryOptions();
                    //.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    // .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    await _distributedCache.SetAsync(cacheKey, cacheData, options);
                }

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GetInvoiceCompanyLogoBase64(Guid logoImageId)
        {
            try
            {
                string data;
                var cacheKey = Constants.LogoCache + logoImageId;
                string serializedData;

                var cacheData = await _distributedCache.GetAsync(cacheKey);
                if (cacheData != null)
                {
                    serializedData = Encoding.UTF8.GetString(cacheData);
                    data = JsonSerializer.Deserialize<string>(serializedData);
                }
                else
                {
                    data = await _context.CompanyInvoiceLogos.Where(x => x.Id == logoImageId)
                        .Select(x => x.Base64string)
                        .SingleOrDefaultAsync();

                    serializedData = JsonSerializer.Serialize(data);
                    cacheData = Encoding.UTF8.GetBytes(serializedData);

                    var options = new DistributedCacheEntryOptions();
                    //.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    // .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    await _distributedCache.SetAsync(cacheKey, cacheData, options);
                }
                
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GetInvoiceSignatureBase64(Guid signatureImageId)
        {
            try
            {
                string data;
                var cacheKey = Constants.SignatureCache + signatureImageId;
                string serializedData;

                var cacheData = await _distributedCache.GetAsync(cacheKey);
                if (cacheData != null)
                {
                    serializedData = Encoding.UTF8.GetString(cacheData);
                    data = JsonSerializer.Deserialize<string>(serializedData);
                }
                else
                {
                    data = await _context.CompanyInvoiceSignatures.Where(x => x.Id == signatureImageId)
                        .Select(x => x.Base64string)
                        .SingleOrDefaultAsync();

                    serializedData = JsonSerializer.Serialize(data);
                    cacheData = Encoding.UTF8.GetBytes(serializedData);

                    var options = new DistributedCacheEntryOptions();
                    //.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    // .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    await _distributedCache.SetAsync(cacheKey, cacheData, options);
                }
                
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        public async Task<Guid> SaveCustomizationBanner(BaseUploadModel model)
        {
            try
            {
                var id = Guid.NewGuid();
                _context.InvoiceBanners.Add(new Models.InvoiceBanner
                {
                    Base64string = model.Base64string,
                    Id = id,
                    CreatedOn = Constants.GetCurrentDateTime()
                });

                if (await _context.SaveChangesAsync() > 0)
                {
                    await _distributedCache.RemoveAsync(Constants.AllBannerCache);
                    return id;
                }
                
                return Guid.Empty;

            }
            catch (Exception ex)
            {
                return Guid.Empty;
            }

        }

        public async Task<Guid> SaveInvoiceCompanyLogo(BaseUploadModel model)
        {
            try
            {
                var id = Guid.NewGuid();
                _context.CompanyInvoiceLogos.Add(new Models.CompanyInvoiceLogo
                {
                    CompanyId = model.CompanyId,
                    Base64string = model.Base64string,
                    Id = id,
                    CreatedOn = Constants.GetCurrentDateTime(),
                    CreatedBy = model.UserId
                });

                return await _context.SaveChangesAsync() > 0 ? id : Guid.Empty;

            }
            catch (Exception ex)
            {
                return Guid.Empty;
            }

        }

        public async Task<Guid> SaveInvoiceSignature(BaseUploadModel model)
        {
            try
            {
                var id = Guid.NewGuid();
                _context.CompanyInvoiceSignatures.Add(new Models.CompanyInvoiceSignature
                {
                    CompanyId = model.CompanyId,
                    Base64string = model.Base64string,
                    Id = id,
                    CreatedOn = Constants.GetCurrentDateTime(),
                    CreatedBy = model.UserId
                });

                return await _context.SaveChangesAsync() > 0 ? id : Guid.Empty;

            }
            catch (Exception ex)
            {
                return Guid.Empty;
            }
        }

        public async Task<Guid> UpdateCustomizationBanner(Guid id, BaseUploadModel model)
        {
            try
            {
                var banner = await _context.InvoiceBanners.SingleOrDefaultAsync(x => x.Id == id);
                if (banner != null)
                {
                    banner.Base64string = model.Base64string;
                }

                if (await _context.SaveChangesAsync() > 0)
                {
                    await _distributedCache.RemoveAsync(Constants.AllBannerCache);
                    return id;
                }
                
                return Guid.Empty;

            }
            catch (Exception ex)
            {
                return Guid.Empty;
            }

        }
    }
}
