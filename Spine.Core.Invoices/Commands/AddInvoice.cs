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
using Spine.Common.Helper;
using Spine.Core.Invoices.Helpers;
using Spine.Core.Invoices.Jobs;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Invoices;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class AddInvoice
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public bool IsRetainer { get; set; }

            [Required]
            public Guid? CustomerId { get; set; }
            [Required]
            public string CustomerName { get; set; }
            [Required]
            public string CustomerEmail { get; set; }

            public string PhoneNo { get; set; }

            //[Required]
            public string BillingAddressLine1 { get; set; }
            public string BillingAddressLine2 { get; set; }
            //[Required]
            public string BillingState { get; set; }
            //[Required]
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
            
          //  [RequiredIf(nameof(TaxRate), 0, IsInverted = true)]
            public Guid? TaxId { get; set; }
            public bool IsRecurring { get; set; }
            [RequiredIf(nameof(IsRecurring), true, ErrorMessage = "Frequency is required if invoice is recurring")]
            public InvoiceFrequency? RecurringFrequency { get; set; }
            public InvoiceFrequency CustomerReminder { get; set; }
            [RequiredIf(nameof(CustomerReminder), InvoiceFrequency.None, IsInverted = true, ErrorMessage = "Reminder time is required")]
            public DateTime? ReminderTime { get; set; }

            [RequiredNotEmpty(ErrorMessage = "Invoice must contain at least one item")]
            public List<LineItemModel> LineItems { get; set; }

            public List<Guid> Documents { get; set; }

            /// <summary>
            /// this will be the invoice base currency from invoice settings,
            /// since they do not select a currency while creating invoice
            /// </summary>
            //[RequiredNonDefault(ErrorMessage = "Select a currency")]
            [JsonIgnore]
            public int? CurrencyId { get; set; }
            [JsonIgnore]
            public decimal? RateToBaseCurrency { get; set; }
        }

        public class LineItemModel
        {
            [Required]
            public Guid? InventoryId { get; set; }
            [Required]
            public string Item { get; set; }
            public string Description { get; set; }
            [RequiredNonDefault]
            public int? Quantity { get; set; }
            public decimal Rate { get; set; }

            public DiscountType? DiscountType { get; set; }
            public decimal DiscountRate { get; set; }

            public string TaxLabel { get; set; }
            [Range(0, 100)]
            public decimal TaxRate { get; set; }
            
          //  [RequiredIf(nameof(TaxRate), 0, IsInverted = true)]
            public Guid? TaxId { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Guid InvoiceId { get; set; }
            public Response(Guid id)
            {
                InvoiceId = id;
                Status = HttpStatusCode.Created;
            }

            public Response(HttpStatusCode statusCode)
            {
                Status = statusCode;
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
            private readonly IAuditLogHelper _auditHelper;
            private readonly IMapper _mapper;
            private readonly IInvoiceHelper _invoiceHelper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper, IInvoiceHelper invoiceHelper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _invoiceHelper = invoiceHelper;
                _mapper = mapper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                if (request.DiscountType == DiscountType.Percentage && request.DiscountRate > 100)
                    return new Response("Discount rate cannot be above 100% if the discount type is percentage");

                if (request.TaxRate != 0 && !request.TaxId.HasValue)
                    return new Response("Tax id is required if tax rate is any value other than 0");

                if (request.LineItems.Any(x => x.TaxRate != 0 && !x.TaxId.HasValue))
                    return new Response("Tax id is required if tax rate is any value other than 0");
                
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.InvoiceDate.Value.Date >= x.StartDate && request.InvoiceDate.Value.Date <= x.EndDate);

                if (accountingPeriod == null || accountingPeriod.IsClosed)
                    return new Response("Invoice date does not have an open accounting period");

                var preference =
                    await _dbContext.InvoicePreferences.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId);
                if (preference == null)
                    return new Response("You must set up invoice settings before adding an invoice");

                var newInvoice = _mapper.Map<Invoice>(request);
                
                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();
                newInvoice.BaseCurrencyId = baseCurrency;
                
                //set the invoice currency = invoice preference currency
                newInvoice.CurrencyId = preference.CurrencyId;
                newInvoice.RateToBaseCurrency = preference.RateToCompanyBaseCurrency;
                
                // multiply by the rate to base currency to save the amounts in the company base currency
                
                //nothing is done on tax rate since it's just a percentage
                
                if (newInvoice.DiscountType == DiscountType.Amount)
                    newInvoice.DiscountRate *= newInvoice.RateToBaseCurrency;
                
                //   request.LineItems.Where(x => x.DiscountType == DiscountType.Amount).ToList()
                //              .ForEach(x => x.DiscountRate *= newInvoice.RateToBaseCurrency.Value);
                foreach (var item in request.LineItems)
                {
                    if (item.DiscountType == DiscountType.Amount)
                        item.DiscountRate *= newInvoice.RateToBaseCurrency;

                    item.Rate *= newInvoice.RateToBaseCurrency;
                }

                if (!preference.EnableDueDate) newInvoice.DueDate = null;

                newInvoice.InvoiceNoString = await _invoiceHelper.GenerateInvoiceNo(_dbContext, newInvoice.CompanyId);

                var inventoryIds = request.LineItems.Select(x => x.InventoryId).ToHashSet();

                var inventories = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId
                                        && inventoryIds.Contains(x.Id) && x.Status == InventoryStatus.Active && !x.IsDeleted)
                    .ToDictionaryAsync(x => x.Id);

                var lineItems = _mapper.Map<List<LineItem>>(request.LineItems);
                
                //TODO: confirm total line Items doesn't contain VAT if invoiceTax is VAT
                
                foreach (var item in lineItems)
                {
                    // doing this in case the same inventory is being added multiple times in the invoice
                    var allInvQty = lineItems.GroupBy(x => x.ItemId).Select(x => new { InvId = x.Key, Qty = x.Sum(d => d.Quantity) }).ToList();

                    if (item.ItemId.HasValue && inventories.TryGetValue(item.ItemId.Value, out var inventory) && inventory != null)
                    {
                        if (inventory.InventoryType == InventoryType.Product)
                        {
                            if (inventory.QuantityInStock < allInvQty.Single(x => x.InvId == item.ItemId).Qty)
                                return new Response($"Item {item.Item} has {inventory.QuantityInStock} quantity left");

                            inventory.QuantityInStock -= item.Quantity;
                            if (inventory.QuantityInStock <= inventory.ReorderLevel)
                            {
                                _scheduler.SendNow(new LowStockNotificationCommand { CompanyId = inventory.CompanyId, InventoryId = inventory.Id}
                                                                , $"Low Stock Notification for Inventory  {inventory.Name} on {DateTime.Today}");
                            }
                        }

                        if (item.DiscountType == DiscountType.Percentage && item.DiscountRate > 100)
                            return new Response("Discount rate cannot be above 100% if the discount type is percentage off");

                        var amount = (item.Rate * item.Quantity);
                        var discountAmount = 0.00m;
                        if (preference.Discount == DiscountSettings.OnLineItem)
                        {
                            if (item.DiscountType == DiscountType.Percentage) discountAmount = (item.DiscountRate * amount) / 100;
                            if (item.DiscountType == DiscountType.Amount) discountAmount = item.DiscountRate;
                        }

                        if (preference.Tax == TaxSettings.Exclusive || preference.ApplyTax == ApplyTaxSettings.None || preference.ApplyTax == ApplyTaxSettings.OnTotal)
                        {
                            item.TaxRate = 0.00m;
                            item.TaxLabel = "";
                            item.TaxId = null;
                        }
                        var taxAmount = (item.TaxRate * amount) / 100;
                        item.ParentItemId = newInvoice.Id;
                        item.CompanyId = newInvoice.CompanyId;
                        item.TaxAmount = taxAmount;
                        item.DiscountAmount = discountAmount;
                        item.Amount = amount - discountAmount + taxAmount;
                        item.CreatedOn = DateTime.Now;
                    }

                    else
                        return new Response("Product/Service not found or has been discontinued");
                }

                _dbContext.LineItems.AddRange(lineItems);

                newInvoice.InvoiceAmount = lineItems.Sum(x => x.Amount);
                if (preference.Discount == DiscountSettings.OnTotal)
                {
                    if (newInvoice.DiscountType == DiscountType.Percentage) newInvoice.DiscountAmount = (newInvoice.DiscountRate * newInvoice.InvoiceAmount) / 100;
                    if (newInvoice.DiscountType == DiscountType.Amount) newInvoice.DiscountAmount = newInvoice.DiscountRate;
                }

                if (preference.Tax == TaxSettings.Exclusive || preference.ApplyTax == ApplyTaxSettings.None || preference.ApplyTax == ApplyTaxSettings.OnLineItem)
                {
                    newInvoice.TaxLabel = "";
                    newInvoice.TaxRate = 0;
                    newInvoice.TaxAmount = 0;
                    newInvoice.TaxId = null;
                }

                newInvoice.TaxAmount =
                    (newInvoice.TaxRate * newInvoice.InvoiceAmount) / 100;
                newInvoice.InvoiceTotalAmount = newInvoice.InvoiceAmount - newInvoice.DiscountAmount + newInvoice.TaxAmount;
                if (preference.RoundAmountToNearestWhole) newInvoice.InvoiceTotalAmount = newInvoice.InvoiceTotalAmount.RoundToWhole();
                newInvoice.InvoiceBalance = newInvoice.InvoiceTotalAmount;
                _dbContext.Invoices.Add(newInvoice);

                foreach (var docId in request.Documents)
                {
                    _dbContext.Documents.Add(new Document
                    {
                        CompanyId = newInvoice.CompanyId,
                        ParentItemId = newInvoice.Id,
                        DocumentId = docId,
                        Id = SequentialGuid.Create()
                    });
                }

                await _invoiceHelper.HandleInvoice(_dbContext, _scheduler, newInvoice, lineItems, true);
                
                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.Create,
                    UserId = request.UserId,
                    Description = $"Created new invoice {newInvoice.InvoiceNoString} with {lineItems.Count} items"
                });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForInvoiceCreation
                    {
                        NewInvoice = newInvoice,
                        LineItems = lineItems,
                        AccountingPeriodId = accountingPeriod.Id
                    });
                    return new Response(newInvoice.Id);
                }

                return new Response("Invoice could not be saved");
            }
        }
    }
}
