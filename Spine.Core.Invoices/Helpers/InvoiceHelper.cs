using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Core.Invoices.Commands;
using Spine.Core.Invoices.Jobs;
using Spine.Data;
using Spine.Data.Documents.Service.Interfaces;
using Spine.Data.Entities;
using Spine.Data.Entities.Invoices;
using Spine.PdfGenerator;
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Common;
using Spine.Services.Paystack.Transactions;

namespace Spine.Core.Invoices.Helpers
{
    public interface IInvoiceHelper
    {
        Task<string> GenerateInvoiceNo(SpineContext dbContext, Guid companyId);
        Task HandleInvoice(SpineContext dbContext, CommandsScheduler scheduler, Invoice newInvoice, List<LineItem> lineItems, bool isNew = false);
        Task<PaymentLinkModel> GeneratePaymentLink(SpineContext dbContext, string invoiceNo, DateTime invoiceDate, decimal invoiceBalance, 
                                                                    string customerName, string customerEmail, Guid? paymentIntegrationId);
        Task<(byte[], string)> GenerateInvoicePdf(IPdfGenerator pdfGenerator, SpineContext dbContext, Guid companyId, Invoice invoice, InvoiceCustomization customization);

        Task<byte[]> GenerateInvoicePdf(IPdfGenerator pdfGenerator, SpineContext dbContext, Invoice invoice, List<LineItem> LineItems);
    }

    public class InvoiceHelper : IInvoiceHelper
    {
        //private readonly IInvoiceCustomizationHelper _customizationHelper; // this calls the customization service as an API call
        private readonly IInvoiceCustomizationService _customizationHelper;
        private readonly PaystackClient _paystackClient;
        public InvoiceHelper(IInvoiceCustomizationService customizationHelper, PaystackClient paystack)
        {
            _customizationHelper = customizationHelper;
            _paystackClient = paystack;
        }

        public async Task<string> GenerateInvoiceNo(SpineContext dbContext, Guid companyId)
        {
            var settings = await dbContext.InvoiceNoSettings.SingleAsync(x => x.CompanyId == companyId);

            var nextNo = 1;
            if (settings.LastGeneratedDate == DateTime.Today)
            {
                nextNo = settings.LastGenerated + 1;
            }

            settings.LastGenerated = nextNo;
            settings.LastGeneratedDate = DateTime.Today;
            await dbContext.SaveChangesAsync();

            return $"{settings.Prefix}{settings.Separator}{DateTime.Today:ddMMyy}{settings.Separator}{nextNo:D3}";
        }

        public async Task<byte[]> GenerateInvoicePdf(IPdfGenerator pdfGenerator, SpineContext _dbContext, Invoice invoice, List<LineItem> lineItems)
        {
            var preference = await (from pref in _dbContext.InvoicePreferences.Where(x => x.CompanyId == invoice.CompanyId)
                                    select pref).SingleAsync();

            var details = await (from cur in _dbContext.Currencies.Where(x => x.Id == invoice.CurrencyId)
                                 join busi in _dbContext.Companies on invoice.CompanyId equals busi.Id
                                 select new { cur, busi.Name, busi.City, busi.Address, busi.PhoneNumber }).SingleOrDefaultAsync();

            var invoiceModel = new InvoicePreview
            {
                Subject = invoice.Subject,
                Recipient = invoice.CustomerName,
                RecipientAddress1 = invoice.BillingAddressLine1 ?? "",
                RecipientAddress2 = invoice.BillingAddressLine2 ?? "",
                RecipientState = invoice.BillingState ?? "",
                RecipientCountry = invoice.BillingCountry ?? "",
                Notes = invoice.CustomerNotes,
                InvoiceNo = invoice.InvoiceNoString,
                DueDate = invoice.DueDate != DateTime.MinValue ? invoice.DueDate : null,
                InvoiceDate = invoice.InvoiceDate,
                Amount = invoice.InvoiceTotalAmount,
                SubTotal = invoice.InvoiceAmount,
                BalanceDue = invoice.InvoiceBalance,
                TaxAmount = invoice.TaxAmount,
                DiscountType = invoice.DiscountType,
                DiscountAmount = invoice.DiscountAmount,
                DiscountRate = invoice.DiscountRate,
                TaxRate = invoice.TaxRate,
                TaxLabel = invoice.TaxLabel,
                Business = new CompanyModel
                {
                    Name = details.Name,
                    City = details.City,
                    Address = details.Address,
                    Phone = details.PhoneNumber
                },
                Preference = new PreferenceModel
                {
                    Currency = new CurrencyModel { Id = details.cur.Id, Code = details.cur.Code, Name = details.cur.Name, Symbol = details.cur.Symbol },
                    Discount = preference.Discount,
                    EnableDueDate = preference.EnableDueDate,
                    Tax = preference.Tax,
                    ApplyTax = preference.ApplyTax,
                },
                LineItems = lineItems.Select(x => new NewLineItem
                {
                    Amount = x.Amount,
                    Item = x.Item,
                    Description = x.Description,
                    DiscountType = x.DiscountType,
                    DiscountRate = x.DiscountRate,
                    Quantity = x.Quantity,
                    Rate = x.Rate,
                    TaxLabel = x.TaxLabel,
                    TaxRate = x.TaxRate
                }).ToList(),
            };

            var pdfBytes = await pdfGenerator.GeneratePdfByte("InvoicePreview", invoiceModel);
            return (pdfBytes);

        }

        public async Task<(byte[], string)> GenerateInvoicePdf(IPdfGenerator pdfGenerator, SpineContext _dbContext, Guid companyId, Invoice invoice, InvoiceCustomization customization)
        {
            var preference = await (from pref in _dbContext.InvoicePreferences.Where(x => x.CompanyId == companyId)
                                    select pref).SingleAsync();

            var currency = await _dbContext.Currencies.SingleOrDefaultAsync(x => x.Id == invoice.CurrencyId);
            var theme = await _dbContext.InvoiceColorThemes.Where(x => x.Id == customization.ColorThemeId).Select(x => x.Theme).SingleOrDefaultAsync();
            var sentInvoice = await _dbContext.SentInvoices.SingleOrDefaultAsync(x => x.InvoiceId == invoice.Id);

            var business = await _dbContext.Companies.SingleAsync(x => x.Id == companyId && !x.IsDeleted);
            if (sentInvoice == null || sentInvoice.PaymentLink.IsNullOrEmpty())
            {
                var paymentLinkModel = new PaymentLinkModel();
                if (preference.PaymentLinkEnabled && currency.Code == Constants.NigerianCurrencyCode) // we are only currently showing only nigerian bank accounts, so if currency is not NGN, no payment link
                {
                    paymentLinkModel = await GeneratePaymentLink(_dbContext, invoice.InvoiceNoString, invoice.InvoiceDate, invoice.InvoiceBalance, invoice.CustomerName, invoice.CustomerEmail, preference.PaymentIntegrationId);
                }
                if (sentInvoice == null)
                {
                    sentInvoice = new SentInvoice
                    {
                        CompanyId = invoice.CompanyId,
                        InvoiceId = invoice.Id,
                        CustomizationId = customization.Id,
                        PaymentLink = paymentLinkModel.AuthorizationUrl,
                        PaymentLinkAccessCode = paymentLinkModel.AccessCode,
                        PaymentLinkReference = paymentLinkModel.Reference,
                        DateSent = Constants.GetCurrentDateTime()
                    };
                    _dbContext.SentInvoices.Add(sentInvoice);
                }
                else
                {
                    sentInvoice.PaymentLink = paymentLinkModel.AuthorizationUrl;
                    sentInvoice.PaymentLinkAccessCode = paymentLinkModel.AccessCode;
                    sentInvoice.PaymentLinkReference = paymentLinkModel.Reference;
                }
            }

            var custBase64 = await _customizationHelper.GetCustomizationBase64(customization.BannerImageId, customization.LogoImageId, customization.SignatureImageId);

            var invoiceModel = new InvoicePreview
            {
                Subject = invoice.Subject,
                Recipient = invoice.CustomerName,
                RecipientAddress1 = invoice.BillingAddressLine1 ?? "",
                RecipientAddress2 = invoice.BillingAddressLine2 ?? "",
                RecipientState = invoice.BillingState ?? "",
                RecipientCountry = invoice.BillingCountry ?? "",
                Notes = invoice.CustomerNotes,
                InvoiceNo = invoice.InvoiceNoString,
                DueDate = invoice.DueDate != DateTime.MinValue ? invoice.DueDate : null,
                InvoiceDate = invoice.InvoiceDate,
                Amount = invoice.InvoiceTotalAmount / invoice.RateToBaseCurrency,
                SubTotal = invoice.InvoiceAmount / invoice.RateToBaseCurrency,
                BalanceDue = invoice.InvoiceBalance / invoice.RateToBaseCurrency,
                TaxAmount = invoice.TaxAmount / invoice.RateToBaseCurrency,
                DiscountType = invoice.DiscountType,
                DiscountAmount = invoice.DiscountAmount / invoice.RateToBaseCurrency,
                DiscountRate = invoice.DiscountType == DiscountType.Percentage
                    ? invoice.DiscountRate
                    : invoice.DiscountRate / invoice.RateToBaseCurrency,
                TaxRate = invoice.TaxRate,
                TaxLabel = invoice.TaxLabel,
                PaymentLink = sentInvoice.PaymentLink,
                Business = new CompanyModel
                {
                    Name = business.Name,
                    City = business.City,
                    Address = business.Address,
                    Phone = business.PhoneNumber
                },
                Preference = new PreferenceModel
                {
                    PaymentLinkEnabled = preference.PaymentLinkEnabled,
                    ShareMessage = preference.ShareMessage,
                    Currency = new CurrencyModel
                        {Id = currency.Id, Code = currency.Code, Name = currency.Name, Symbol = currency.Symbol},
                    Discount = preference.Discount,
                    //     DueDate = preference.DueDate,
                    EnableDueDate = preference.EnableDueDate,
                    PaymentTerms = preference.PaymentTerms,
                    Tax = preference.Tax,
                    ApplyTax = preference.ApplyTax,
                },
                Customization = new CustomizationModel
                {
                    Theme = theme,
                    Banner = customization.BannerImageId.HasValue
                        ? await _customizationHelper.GetInvoiceBannerBase64(customization.BannerImageId.Value)
                        : "", //custBase64?.BannerBase64,
                    CompanyLogo = customization.LogoImageId.HasValue
                        ? await _customizationHelper.GetInvoiceCompanyLogoBase64(customization.LogoImageId.Value)
                        : "", // custBase64?.CompanyLogoBase64,
                    Signature = customization.SignatureImageId.HasValue
                        ? await _customizationHelper.GetInvoiceSignatureBase64(customization.SignatureImageId.Value)
                        : "", // custBase64?.SignatureBase64,
                    SignatureEnabled = customization.SignatureEnabled,
                    SignatureName = customization.SignatureName,
                    LogoEnabled = customization.LogoEnabled
                },
                LineItems = await _dbContext.LineItems
                    .Where(l => l.CompanyId == invoice.CompanyId && l.ParentItemId == invoice.Id)
                    .OrderBy(x => x.CreatedOn)
                    .Select(x => new NewLineItem
                    {
                        Amount = x.Amount / invoice.RateToBaseCurrency,
                        Item = x.Item,
                        Description = x.Description,
                        DiscountType = x.DiscountType,
                        DiscountRate = x.DiscountType == DiscountType.Percentage ? x.DiscountRate : x.DiscountRate / invoice.RateToBaseCurrency,
                        Quantity = x.Quantity,
                        Rate = x.Rate / invoice.RateToBaseCurrency,
                        TaxLabel = x.TaxLabel,
                        TaxRate = x.TaxRate,
                        TaxAmount = x.TaxAmount / invoice.RateToBaseCurrency,
                        DiscountAmount = x.DiscountAmount / invoice.RateToBaseCurrency,
                    }).ToListAsync(),
            };

            var pdfBytes = await pdfGenerator.GeneratePdfByte("Invoice", invoiceModel);
            return (pdfBytes, sentInvoice.PaymentLink);

        }

        public async Task<PaymentLinkModel> GeneratePaymentLink(SpineContext dbContext, string invoiceNo, DateTime invoiceDate, 
                                    decimal invoiceBalance, string customerName, string customerEmail, Guid? paymentIntegrationId)
        {
            var model = new PaymentLinkModel();
            var integration = await dbContext.PaymentIntegrations.SingleOrDefaultAsync(x => x.Id == paymentIntegrationId);
            if (integration == null) return model;

            if (integration.IntegrationProvider == PaymentIntegrationProvider.Paystack)
            {
                if (integration.IntegrationType == PaymentIntegrationType.Customer)
                {
                    var request = new InitializePaystackTransaction.Request
                    {
                        Subaccount = integration.SubaccountCode,
                        Bearer = "subaccount",
                        AmountInKobo = Convert.ToInt32(invoiceBalance * 100),
                        Email = customerEmail,
                        TransactionCharge = 0,
                        Metadata = new MetaDataObject[]
                        {
                            new MetaDataObject { custom_fields =  new [] {
                                new CustomFields { display_name = "Payer Name", variable_name = "payer_name", value = customerName },
                                new CustomFields { display_name = "Invoice No", variable_name = "invoice_no", value = invoiceNo },
                                new CustomFields { display_name = "Invoice Date", variable_name = "invoice_date", value = invoiceDate.ToLongDateString() },
                                new CustomFields { display_name = "Date Initialized", variable_name = "date_initialized", value = DateTime.Today.ToLongDateString() }
                                }
                            }
                        }
                    };

                    var handler = new InitializePaystackTransaction.Handler();
                    var response = await handler.Handle(request, _paystackClient);
                    if (response != null && response.Status)
                    {
                        model.AuthorizationUrl = response.Data.AuthorizationUrl;
                        model.AccessCode = response.Data.AccessCode;
                        model.Reference = response.Data.Reference;

                        return model;
                    }
                }

                if (integration.IntegrationType == PaymentIntegrationType.Spine)
                {
                }
            }


            if (integration.IntegrationProvider == PaymentIntegrationProvider.Flutterwave)
            {
                if (integration.IntegrationType == PaymentIntegrationType.Customer)
                {
                    return model;
                }
                if (integration.IntegrationType == PaymentIntegrationType.Spine)
                {
                    return model;
                }
            }

            return model;
        }

        public async Task HandleInvoice(SpineContext dbContext, CommandsScheduler scheduler, Invoice newInvoice,
            List<LineItem> lineItems, bool isNew = false)
        {
            //only schedule recurring jobs for new invoice, and not for a recurring invoice
            if (isNew && newInvoice.IsRecurring && newInvoice.RecurringFrequency != InvoiceFrequency.None)
            {
                string schedule = "";
                switch (newInvoice.RecurringFrequency)
                {
                    case InvoiceFrequency.Daily:
                        schedule = Cron.Daily(8);
                        break;
                    case InvoiceFrequency.Weekly:
                        schedule = Cron.Weekly(DayOfWeek.Monday, 8);
                        break;
                    case InvoiceFrequency.Monthly:
                        schedule = Cron.Monthly(1, 8);
                        break;
                    case InvoiceFrequency.Yearly:
                        schedule = Cron.Yearly(1, 1, 8);
                        break;
                    default:
                        break;
                }

                scheduler.ScheduleRecurring(new CreateRecurringInvoiceCommand
                    {
                        CompanyId = newInvoice.CompanyId,
                        Id = newInvoice.Id
                    },
                    $"Recurring Invoice {newInvoice.Id}", schedule, $"Invoice No {newInvoice.InvoiceNoString}");
            }

            if (newInvoice.CustomerReminder != InvoiceFrequency.None && newInvoice.ReminderTime != null)
            {
                string schedule = "";
                var timeHour = newInvoice.ReminderTime.Value.Hour;
                var timeMinute = newInvoice.ReminderTime.Value.Minute;

                switch (newInvoice.CustomerReminder)
                {
                    case InvoiceFrequency.Daily:
                        schedule = Cron.Daily(timeHour, timeMinute);
                        break;
                    case InvoiceFrequency.Weekly:
                        schedule = Cron.Weekly(DayOfWeek.Monday, timeHour, timeMinute);
                        break;
                    case InvoiceFrequency.Monthly:
                        schedule = Cron.Monthly(1, timeHour, timeMinute);
                        break;
                    case InvoiceFrequency.Yearly:
                        schedule = Cron.Yearly(1, 1, timeHour, timeMinute);
                        break;
                    default:
                        break;
                }

                scheduler.ScheduleRecurring(new CreateRecurringReminderCommand
                    {
                        CompanyId = newInvoice.CompanyId,
                        Id = newInvoice.Id
                    },
                    $"Invoice Reminder {newInvoice.Id}", schedule, $"Invoice No {newInvoice.InvoiceNoString}");
            }

            var customer = await dbContext.Customers.SingleOrDefaultAsync(x =>
                x.CompanyId == newInvoice.CompanyId && x.Id == newInvoice.CustomerId && !x.IsDeleted);
            if (customer != null)
            {
                customer.TotalPurchases += newInvoice.InvoiceTotalAmount;
                customer.AmountOwed += newInvoice.InvoiceTotalAmount;
            }

        }

    }
}
