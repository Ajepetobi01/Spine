using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Invoices.Helpers;
using Spine.Core.Invoices.Jobs;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Invoices;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public class CreateRecurringInvoiceCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }

    }

    public class CreateRecurringInvoiceHandler : IRequestHandler<CreateRecurringInvoiceCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly IAuditLogHelper _auditHelper;
        private readonly IMapper _mapper;
        private readonly IInvoiceHelper _invoiceHelper;
        private readonly CommandsScheduler _scheduler;
        private readonly ILogger<CreateRecurringInvoiceHandler> _logger;

        public CreateRecurringInvoiceHandler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper,
                            IInvoiceHelper invoiceHelper, CommandsScheduler scheduler, ILogger<CreateRecurringInvoiceHandler> logger)
        {
            _dbContext = context;
            _auditHelper = auditHelper;
            _mapper = mapper;
            _scheduler = scheduler;
            _logger = logger;
            _invoiceHelper = invoiceHelper;
        }

        public async Task<Unit> Handle(CreateRecurringInvoiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var invoice = await _dbContext.Invoices.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (invoice == null)
                {
                    _logger.LogInformation(
                        $"Invoice with Invoice Id {request.Id} for company Id {request.CompanyId} not found");
                    return Unit.Value;
                }

                if (invoice.InvoiceStatus == InvoiceStatus.Cancelled)
                {
                    _logger.LogInformation(
                        $"Invoice with Invoice Id {request.Id} for company Id {request.CompanyId} has been cancelled");
                    return Unit.Value;
                }
                
                var preference = await _dbContext.InvoicePreferences.SingleAsync(x => x.CompanyId == request.CompanyId);
                var today = Constants.GetCurrentDateTime();

                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && today.Date >= x.StartDate && today.Date <= x.EndDate);

                if (accountingPeriod == null || accountingPeriod.IsClosed)
                {
                    _logger.LogInformation(
                        $"Could not create recurring invoice for invoice no {invoice.InvoiceNoString}. Invoice date does not have an open accounting period");
                    return Unit.Value;
                }
                 
                var newInvoice = _mapper.Map<Invoice>(invoice);
                var oldInvoiceId = invoice.Id;
                newInvoice.Id = SequentialGuid.Create();
                newInvoice.CreatedOn = today;
                newInvoice.LastModifiedBy = null;
                newInvoice.InvoiceDate = today.Date;
                newInvoice.InvoiceStatus = InvoiceStatus.Generated;
                newInvoice.InvoiceNoString = await _invoiceHelper.GenerateInvoiceNo(_dbContext, newInvoice.CompanyId);

                if (preference.EnableDueDate)
                {
                    newInvoice.DueDate = today.Date.AddDays(preference.DueDate);
                }
                else
                {
                    newInvoice.DueDate = null;
                }


                var lineItems = await _dbContext.LineItems
                    .Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == oldInvoiceId).OrderBy(x => x.CreatedOn)
                    .ToListAsync();
                var inventoryIds = lineItems.Select(x => x.ItemId).ToHashSet();

                var inventories = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId
                                                                          && inventoryIds.Contains(x.Id) &&
                                                                          x.Status == InventoryStatus.Active &&
                                                                          !x.IsDeleted)
                    .ToDictionaryAsync(x => x.Id);

                var newLineItems = new List<LineItem>();
                foreach (var item in lineItems)
                {
                    // doing this in case the same inventory is being added multiple times in the invoice
                    var allInvQty = lineItems.GroupBy(x => x.ItemId)
                        .Select(x => new {InvId = x.Key, Qty = x.Sum(d => d.Quantity)}).ToList();

                    if (item.ItemId.HasValue &&
                        inventories.TryGetValue(item.ItemId.Value, out var inventory) && inventory != null)
                    {
                        var newItem = _mapper.Map<LineItem>(item);
                        if (inventory.InventoryType == InventoryType.Product)
                        {
                            if (inventory.QuantityInStock < allInvQty.Single(x => x.InvId == newItem.ItemId).Qty)
                            {
                                _logger.LogInformation(
                                    $"Item {item.Item} has {inventory.QuantityInStock} quantity left");
                                return Unit.Value;
                            }

                            inventory.QuantityInStock -= newItem.Quantity;
                            if (inventory.QuantityInStock <= inventory.ReorderLevel)
                            {
                                _scheduler.SendNow(new LowStockNotificationCommand
                                        {CompanyId = inventory.CompanyId, InventoryId = inventory.Id}
                                    , $"Low Stock Notification for Inventory  {inventory.Name} on {DateTime.Today}");
                            }
                        }

                        newItem.Rate = inventory.UnitSalesPrice;
                        if (invoice.CurrencyId !=
                            preference.CurrencyId) // base currency has changed since invoice was created.
                            // can't create new since we don't know new rate
                        {
                            _logger.LogInformation(
                                $"Could not create recurring invoice for invoice no {invoice.InvoiceNoString}. Currency has changed");
                            return Unit.Value;
                        }
                        else
                        {
                            newItem.Rate = inventory.UnitSalesPrice / invoice.RateToBaseCurrency;
                        }

                        var amount = (newItem.Rate * newItem.Quantity);
                        var discountAmount = 0.00m;
                        if (preference.Discount == DiscountSettings.OnLineItem)
                        {
                            if (newItem.DiscountType == DiscountType.Percentage)
                                discountAmount = (newItem.DiscountRate * amount) / 100;
                            if (newItem.DiscountType == DiscountType.Amount) discountAmount = newItem.DiscountRate;
                        }

                        if (preference.Tax == TaxSettings.Exclusive || preference.ApplyTax == ApplyTaxSettings.None ||
                            preference.ApplyTax == ApplyTaxSettings.OnTotal)
                        {
                            newItem.TaxRate = 0.00m;
                            newItem.TaxLabel = "";
                        }

                        var taxAmount = (newItem.TaxRate * amount) / 100;
                        newItem.ParentItemId = newInvoice.Id;
                        newItem.CompanyId = newInvoice.CompanyId;
                        newItem.TaxAmount = taxAmount;
                        newItem.DiscountAmount = discountAmount;
                        newItem.Amount = amount - discountAmount + taxAmount;
                        newItem.CreatedOn = DateTime.Now;
                        newLineItems.Add(newItem);
                    }

                    else
                    {
                        _logger.LogInformation(
                            $"Product/Service  {item.Item} not found or discontinued while creating recurring invoice with invoice no {newInvoice.InvoiceNoString}");
                        return Unit.Value;
                    }
                }

                newInvoice.InvoiceAmount = newLineItems.Sum(x => x.Amount);
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

                _dbContext.Invoices.Add(newInvoice);
                _dbContext.LineItems.AddRange(newLineItems);

                var docs = await _dbContext.Documents
                    .Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == oldInvoiceId).ToListAsync();
                lineItems.ForEach(x => x.ParentItemId = newInvoice.Id);
                _dbContext.Documents.AddRange(docs);

                await _invoiceHelper.HandleInvoice(_dbContext, _scheduler, newInvoice, newLineItems);

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int) AuditLogEntityType.Invoice,
                    Action = (int) AuditLogInvoiceAction.Create,
                    UserId = newInvoice.CreatedBy,
                    Description =
                        $"Created recurring invoice {newInvoice.InvoiceNoString} from invoice with invoice no {invoice.InvoiceNoString} with {lineItems.Count} items"
                });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _logger.LogInformation($"Created recurring invoice with invoice no {newInvoice.InvoiceNoString}");
                    _scheduler.SendNow(new HandleAccountingForInvoiceCreation
                    {
                        NewInvoice = newInvoice,
                        LineItems = lineItems,
                        AccountingPeriodId = accountingPeriod.Id
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while creating recurring invoice for " + request.Id + " " + ex.Message);
            }

            return Unit.Value;
        }
    }
}
