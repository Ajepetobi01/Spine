using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Models;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoiceSettings
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public string InvoiceNoPrefix { get; set; }
            public string InvoiceNoSeparator { get; set; }
            public DiscountSettings Discount { get; set; }
            public TaxSettings Tax { get; set; }
            public ApplyTaxSettings ApplyTax { get; set; }
            public int CurrencyId { get; set; }
            public decimal RateToBaseCurrency { get; set; }
            public bool RoundAmountToNearestWhole { get; set; }
            public int DueDate { get; set; }
            public bool EnableDueDate { get; set; }
            public CurrencyModel Currency { get; set; }
            public CustomizationModel Customization { get; set; }
            public PaymentIntegrationModel PaymentIntegration { get; set; }

            public string PaymentTerms { get; set; }
            public string ShareMessage { get; set; }
            public bool PaymentLinkEnabled { get; set; }

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

        public class PaymentIntegrationModel
        {
            public PaymentIntegrationProvider IntegrationProvider { get; set; }
            public PaymentIntegrationType IntegrationType { get; set; }
            public string SettlementBankCode { get; set; }
            public string SettlementBankName { get; set; }
            public string SettlementBankCurrency { get; set; }
            public string SettlementAccountNumber { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var settings = await (from pref in _dbContext.InvoicePreferences
                                      where pref.CompanyId == request.CompanyId
                                      join cur in _dbContext.Currencies on pref.CurrencyId equals cur.Id
                                      join cust in _dbContext.InvoiceCustomizations on pref.CustomizationId equals cust.Id
                                      join sett in _dbContext.InvoiceNoSettings on pref.CompanyId equals sett.CompanyId
                                      join theme in _dbContext.InvoiceColorThemes on cust.ColorThemeId equals theme.Id into invTheme
                                      from theme in invTheme.DefaultIfEmpty()
                                      join payment in _dbContext.PaymentIntegrations on pref.PaymentIntegrationId equals payment.Id into integrations
                                      from payment in integrations.DefaultIfEmpty()
                                      select new Response
                                      {
                                          EnableDueDate = pref.EnableDueDate,
                                          DueDate = pref.DueDate,
                                          InvoiceNoPrefix = sett.Prefix,
                                          InvoiceNoSeparator = sett.Separator,
                                          ShareMessage = pref.ShareMessage,
                                          CurrencyId = pref.CurrencyId,
                                          RateToBaseCurrency = pref.RateToCompanyBaseCurrency,
                                          Currency = new CurrencyModel
                                          {
                                              Id = cur.Id,
                                              Name = cur.Name,
                                              Symbol = cur.Symbol,
                                              Code = cur.Code
                                          },
                                          Discount = pref.Discount,
                                          PaymentLinkEnabled = pref.PaymentLinkEnabled,
                                          PaymentTerms = pref.PaymentTerms,
                                          RoundAmountToNearestWhole = pref.RoundAmountToNearestWhole,
                                          ApplyTax = pref.ApplyTax,
                                          Tax = pref.Tax,
                                          Customization = new CustomizationModel
                                          {
                                              Id = cust.Id,

                                              ThemeId = cust.ColorThemeId,
                                              Theme = theme.Theme ?? "",
                                              TextColor = theme.TextColor ?? "",
                                              SignatureId = cust.SignatureImageId,
                                              BannerId = cust.BannerImageId,
                                              CompanyLogoId = cust.LogoImageId,
                                              SignatureEnabled = cust.SignatureEnabled,
                                              SignatureName = cust.SignatureName,
                                              LogoEnabled = cust.LogoEnabled
                                          },
                                          PaymentIntegration = payment == null ? null : new PaymentIntegrationModel
                                          {
                                              IntegrationProvider = payment.IntegrationProvider,
                                              IntegrationType = payment.IntegrationType,
                                              SettlementAccountNumber = payment.SettlementAccountNumber,
                                              SettlementBankCode = payment.SettlementBankCode,
                                              SettlementBankName = payment.SettlementBankName,
                                              SettlementBankCurrency = payment.SettlementBankCurrency
                                          }
                                      }).SingleOrDefaultAsync();

                return settings;
            }
        }

    }
}
