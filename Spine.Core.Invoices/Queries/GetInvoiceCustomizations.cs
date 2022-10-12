using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoiceCustomizations
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response : List<CustomizationModel>
        {
        }

        public class CustomizationModel
        {
            public Guid Id { get; set; }
            public bool LogoEnabled { get; set; }
            public bool SignatureEnabled { get; set; }
            public string SignatureName { get; set; }

            public Guid? BannerId { get; set; }
            public Guid? CompanyLogoId { get; set; }
            public Guid? SignatureId { get; set; }
            public Guid? ThemeId { get; set; }
            public string Theme { get; set; }
            public string TextColor { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IMapper _mapper;

            public Handler(SpineContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var settings = await (from cust in _dbContext.InvoiceCustomizations
                                      where cust.CompanyId == request.CompanyId
                                      join theme in _dbContext.InvoiceColorThemes on cust.ColorThemeId equals theme.Id into invTheme
                                      from theme in invTheme.DefaultIfEmpty()
                                      select new CustomizationModel
                                      {
                                          Id = cust.Id,
                                          BannerId = cust.BannerImageId,
                                          ThemeId = cust.ColorThemeId,
                                          Theme = theme.Theme ?? "",
                                          TextColor = theme.TextColor ?? "",
                                          SignatureId = cust.SignatureImageId,
                                          CompanyLogoId = cust.LogoImageId,

                                          SignatureEnabled = cust.SignatureEnabled,
                                          SignatureName = cust.SignatureName,
                                          LogoEnabled = cust.LogoEnabled
                                      }).ToListAsync();

                return _mapper.Map<Response>(settings);
            }
        }

    }
}
