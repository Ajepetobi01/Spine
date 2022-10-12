using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoice
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid InvoiceId { get; set; }

        }

        public class Response
        {
            public Guid Id { get; set; }
            public Guid CustomerId { get; set; }
            public string Email { get; set; }
            public string PhoneNo { get; set; }
            public string Recipient { get; set; }
            public string Subject { get; set; }
            public string InvoiceNo { get; set; }
            public decimal Amount { get; set; }
            public decimal SubTotal { get; set; }
            public decimal BalanceDue { get; set; }

            public DateTime InvoiceDate { get; set; }
            public DateTime? DueDate { get; set; }
            public string Status { get; set; }

            public int InvoiceTypeId { get; set; }
            public string CustomerNotes { get; set; }
            public decimal DiscountRate { get; set; }
            public string TaxLabel { get; set; }
            public decimal TaxRate { get; set; }
            public decimal TaxAmount { get; set; }
            public DiscountType DiscountType { get; set; }
            public decimal DiscountAmount { get; set; }
            public bool IsRecurring { get; set; }
            public InvoiceFrequency RecurringFrequency { get; set; }
            public InvoiceFrequency CustomerReminder { get; set; }
            public DateTime? ReminderTime { get; set; }

            public AddressModel BillingAddress { get; set; }
            public AddressModel ShippingAddress { get; set; }
            public List<Guid> Documents { get; set; }
            public List<LineItemModel> LineItems { get; set; }
            //      public List<InvoicePaymentModel> Payments { get; set; }

            public Customization Customization { get; set; }

            [JsonIgnore]
            public InvoiceStatus StatusEnum { get; set; }

            public int CurrencyId { get; set; }
            public decimal RateToBaseCurrency { get; set; }

            public CurrencyModel Currency { get; set; }
        }

        public class LineItemModel
        {
            public Guid Id { get; set; }
            public string Item { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public decimal Rate { get; set; }
            public string TaxLabel { get; set; }
            public decimal TaxRate { get; set; }
            public DiscountType DiscountType { get; set; }
            public decimal DiscountRate { get; set; }
            public decimal Amount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal DiscountAmount { get; set; }
        }

        public class AddressModel
        {
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
            public string State { get; set; }
        }

        public class Customization
        {
            public string Theme { get; set; }
            public Guid? CompanyLogo { get; set; }
            public Guid? Banner { get; set; }
            public Guid? Signature { get; set; }
            public string SignatureName { get; set; }
            public bool LogoEnabled { get; set; }
            public bool SignatureEnabled { get; set; }

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

                var item = await (from invoice in _dbContext.Invoices.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.InvoiceId)
                                  join cur in _dbContext.Currencies on invoice.CurrencyId equals cur.Id
                                  //  join customer in _dbContext.Customers on invoice.CustomerId equals customer.Id
                                  select new Response
                                  {
                                      Id = invoice.Id,
                                      CustomerId = invoice.CustomerId,
                                      Email = invoice.CustomerEmail,
                                      Recipient = invoice.CustomerName,
                                      Subject = invoice.Subject,
                                      InvoiceDate = invoice.InvoiceDate,
                                      DueDate = invoice.DueDate != DateTime.MinValue ? invoice.DueDate : null,
                                      CustomerNotes = invoice.CustomerNotes,
                                      CustomerReminder = invoice.CustomerReminder,
                                      DiscountType = invoice.DiscountType,
                                      InvoiceTypeId = invoice.InvoiceTypeId,
                                      IsRecurring = invoice.IsRecurring,
                                      StatusEnum = invoice.InvoiceStatus,
                                      RecurringFrequency = invoice.RecurringFrequency,
                                      ReminderTime = invoice.ReminderTime,
                                      CurrencyId = invoice.CurrencyId,
                                      RateToBaseCurrency = invoice.RateToBaseCurrency,
                                      DiscountRate = invoice.DiscountType == DiscountType.Percentage ? invoice.DiscountRate : invoice.DiscountRate / invoice.RateToBaseCurrency,
                                      SubTotal = invoice.InvoiceAmount / invoice.RateToBaseCurrency,
                                      Amount = invoice.InvoiceTotalAmount / invoice.RateToBaseCurrency,
                                      BalanceDue = invoice.InvoiceBalance / invoice.RateToBaseCurrency,
                                      TaxLabel = invoice.TaxLabel,
                                      TaxRate = invoice.TaxRate,
                                      TaxAmount = invoice.TaxAmount / invoice.RateToBaseCurrency,
                                      DiscountAmount = invoice.DiscountAmount / invoice.RateToBaseCurrency,
                                      PhoneNo = invoice.PhoneNo,
                                      InvoiceNo = invoice.InvoiceNoString,
                                      Currency = new CurrencyModel
                                      {
                                          Code = cur.Code,
                                          Id = cur.Id,
                                          Symbol = cur.Symbol,
                                          Name = cur.Name
                                      },
                                      BillingAddress = new AddressModel
                                      {
                                          AddressLine1 = invoice.BillingAddressLine1,
                                          AddressLine2 = invoice.BillingAddressLine2,
                                          State = invoice.BillingState,
                                          PostalCode = invoice.BillingPostalCode,
                                          Country = invoice.BillingCountry
                                      },
                                      ShippingAddress = new AddressModel
                                      {
                                          AddressLine1 = invoice.ShippingAddressLine1,
                                          AddressLine2 = invoice.ShippingAddressLine2,
                                          State = invoice.ShippingState,
                                          PostalCode = invoice.ShippingPostalCode,
                                          Country = invoice.ShippingCountry
                                      }
                                  }).SingleOrDefaultAsync();

                if (item == null) return null;

                var lineItems = await _dbContext.LineItems.Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == item.Id)
                    .OrderBy(x => x.CreatedOn)
                    .Select(d => new LineItemModel
                    {
                        DiscountType = d.DiscountType,
                        DiscountRate = d.DiscountType == DiscountType.Percentage ? d.DiscountRate : d.DiscountRate / item.RateToBaseCurrency,
                        Description = d.Description,
                        Item = d.Item,
                        Amount = d.Amount / item.RateToBaseCurrency,
                        Quantity = d.Quantity,
                        Rate = d.Rate / item.RateToBaseCurrency,
                        Id = d.Id,
                        TaxRate = d.TaxRate,
                        TaxAmount = d.TaxAmount / item.RateToBaseCurrency,
                        DiscountAmount = d.DiscountAmount / item.RateToBaseCurrency,
                        TaxLabel = d.TaxLabel
                    }).ToListAsync();

                item.LineItems = lineItems;

                var docs = await _dbContext.Documents.Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == item.Id)
                    .Select(x => x.DocumentId).ToListAsync();
                item.Documents = docs;

                if (item.BalanceDue == 0)
                    item.Status = "Completed";

                else
                {
                    var currentDate = Constants.GetCurrentDateTime().Date;
                    if (item.DueDate.HasValue && item.DueDate.Value.Date < currentDate)
                    {
                        var dateDiff = (currentDate - item.DueDate.Value.Date).Duration().Days;
                        item.Status = $"Overdue by {dateDiff} day(s)";
                    }
                    else
                    {
                        item.Status = "Not due";
                    }
                }

                item.Customization = new Customization();
                if (item.StatusEnum >= InvoiceStatus.Sent)
                {
                    var sentSettings = await (from sent in _dbContext.SentInvoices.Where(x => x.InvoiceId == item.Id && x.CompanyId == request.CompanyId)
                                              join cust in _dbContext.InvoiceCustomizations on sent.CustomizationId equals cust.Id
                                              join theme in _dbContext.InvoiceColorThemes on cust.ColorThemeId equals theme.Id
                                              select new
                                              {
                                                  cust.LogoImageId,
                                                  cust.BannerImageId,
                                                  cust.SignatureImageId,
                                                  cust.SignatureName,
                                                  cust.LogoEnabled,
                                                  cust.SignatureEnabled,
                                                  theme.Theme
                                              }).SingleOrDefaultAsync();

                    if (sentSettings != null)
                    {
                        item.Customization.CompanyLogo = sentSettings.LogoImageId;
                        item.Customization.Banner = sentSettings.BannerImageId;
                        item.Customization.Signature = sentSettings.SignatureImageId;
                        item.Customization.Theme = sentSettings.Theme;
                        item.Customization.SignatureName = sentSettings.SignatureName;
                        item.Customization.LogoEnabled = sentSettings.LogoEnabled;
                        item.Customization.SignatureEnabled = sentSettings.SignatureEnabled;
                    }
                }

                return item;

            }
        }

    }
}
