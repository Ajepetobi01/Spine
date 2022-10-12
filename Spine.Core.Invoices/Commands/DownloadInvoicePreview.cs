using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Core.Invoices.Helpers;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Invoices;
using Spine.PdfGenerator;

namespace Spine.Core.Invoices.Commands
{
    public static class DownloadInvoicePreview
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public Guid? CustomerId { get; set; }
            [Required]
            public string CustomerName { get; set; }
            [Required]
            public string CustomerEmail { get; set; }

            public string PhoneNo { get; set; }

            [Required]
            public string BillingAddressLine1 { get; set; }
            public string BillingAddressLine2 { get; set; }
            [Required]
            public string BillingState { get; set; }
            [Required]
            public string BillingCountry { get; set; }
            public string BillingPostalCode { get; set; }

            public string ShippingAddressLine1 { get; set; }
            public string ShippingAddressLine2 { get; set; }
            public string ShippingState { get; set; }
            public string ShippingCountry { get; set; }
            public string ShippingPostalCode { get; set; }

            [RequiredNonDefault]
            public int? InvoiceTypeId { get; set; }
            [Required]
            public DateTime? InvoiceDate { get; set; }
            public DateTime? DueDate { get; set; }

            [Required]
            public string Subject { get; set; }
            public string CustomerNotes { get; set; }

            public DiscountType? DiscountType { get; set; }
            public decimal DiscountRate { get; set; }

            public string TaxLabel { get; set; }
            [Range(0, 100)]
            public decimal TaxRate { get; set; }

            [RequiredNotEmpty(ErrorMessage = "Invoice must contain at least one item")]
            public List<AddInvoice.LineItemModel> LineItems { get; set; }

            [JsonIgnore]
           // [RequiredNonDefault(ErrorMessage = "Select a currency")]
            public int? CurrencyId { get; set; }

            [JsonIgnore]
            public decimal? RateToBaseCurrency { get; set; }
        }

        public class Response : BasicActionResult
        {
            public byte[] PdfByte { get; set; }
            public Response(byte[] pdfByte)
            {
                PdfByte = pdfByte;
                Status = HttpStatusCode.OK;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IMapper _mapper;
            private readonly IInvoiceHelper _invoiceHelper;
            private readonly IPdfGenerator _pdfGenerator;

            public Handler(SpineContext context, IMapper mapper, IInvoiceHelper invoiceHelper, IPdfGenerator pdfGenerator)
            {
                _dbContext = context;
                _mapper = mapper;
                _invoiceHelper = invoiceHelper;
                _pdfGenerator = pdfGenerator;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                if (request.DiscountType == DiscountType.Percentage && request.DiscountRate > 100)
                    return new Response("Discount rate cannot be above 100% if the discount type is percentage");

                var preference =
                    await _dbContext.InvoicePreferences.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId);
                if (preference == null)
                    return new Response("You must set up invoice settings before adding an invoice");

                request.CurrencyId = preference.CurrencyId;
                request.RateToBaseCurrency = preference.RateToCompanyBaseCurrency;

                var newInvoice = _mapper.Map<Invoice>(request);
                
                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();
                newInvoice.BaseCurrencyId = baseCurrency;
                
                if (!preference.EnableDueDate) newInvoice.DueDate = null;

                var lineItems = _mapper.Map<List<LineItem>>(request.LineItems);
                foreach (var item in lineItems)
                {
                    if (item.DiscountType == DiscountType.Percentage && item.DiscountRate > 100)
                        return new Response(
                            "Discount rate cannot be above 100% if the discount type is percentage off");

                    var amount = (item.Rate * item.Quantity);
                    var discountAmount = 0.00m;
                    if (preference.Discount == DiscountSettings.OnLineItem)
                    {
                        if (item.DiscountType == DiscountType.Percentage)
                            discountAmount = (item.DiscountRate * amount) / 100;
                        if (item.DiscountType == DiscountType.Amount) discountAmount = item.DiscountRate;
                    }

                    if (preference.Tax == TaxSettings.Exclusive || preference.ApplyTax == ApplyTaxSettings.None ||
                        preference.ApplyTax == ApplyTaxSettings.OnTotal)
                    {
                        item.TaxRate = 0.00m;
                        item.TaxLabel = "";
                    }

                    var taxAmount = (item.TaxRate * amount) / 100;
                    item.ParentItemId = newInvoice.Id;
                    item.CompanyId = newInvoice.CompanyId;
                    item.Amount = amount - discountAmount + taxAmount;
                    item.TaxAmount = taxAmount;
                    item.DiscountAmount = discountAmount;
                }

                newInvoice.InvoiceAmount = lineItems.Sum(x => x.Amount);
                if (preference.Discount == DiscountSettings.OnTotal)
                {
                    if (newInvoice.DiscountType == DiscountType.Percentage)
                        newInvoice.DiscountAmount = (newInvoice.DiscountRate * newInvoice.InvoiceAmount) / 100;
                    if (newInvoice.DiscountType == DiscountType.Amount)
                        newInvoice.DiscountAmount = newInvoice.DiscountRate;
                }

                if (preference.Tax == TaxSettings.Exclusive || preference.ApplyTax == ApplyTaxSettings.None ||
                    preference.ApplyTax == ApplyTaxSettings.OnLineItem)
                {
                    newInvoice.TaxLabel = "";
                    newInvoice.TaxRate = 0;
                    newInvoice.TaxAmount = 0;
                }

                newInvoice.TaxAmount = (newInvoice.TaxRate * newInvoice.InvoiceAmount) / 100;
                newInvoice.InvoiceTotalAmount =
                    newInvoice.InvoiceAmount - newInvoice.DiscountAmount + newInvoice.TaxAmount;
                if (preference.RoundAmountToNearestWhole)
                    newInvoice.InvoiceTotalAmount = newInvoice.InvoiceTotalAmount.RoundToWhole();
                newInvoice.InvoiceBalance = newInvoice.InvoiceTotalAmount;

                var pdfByte = await _invoiceHelper.GenerateInvoicePdf(_pdfGenerator, _dbContext, newInvoice, lineItems);

                return pdfByte != null
                    ? new Response(pdfByte)
                    : new Response("Invoice  preview could not be generated");
            }
        }

    }
}
