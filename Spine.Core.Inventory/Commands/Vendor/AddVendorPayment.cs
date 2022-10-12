using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Transactions;
using Spine.Data.Entities.Vendor;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Vendor
{
    public static class AddVendorPayment
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            [Required] public AddPaymentModel Model { get; set; }

        }
        public class AddPaymentModel
        {
            [Required] public DateTime? PaymentDate { get; set; }
            [Required] public PaymentMode? PaymentSource { get; set; }
            [Required] public Guid? BankAccountId { get; set; }
            
            [RequiredNotEmpty]
            public List<PaymentModel> Payments { get; set; }
        }
        
        public class PaymentModel
        {
            [Required] public Guid? ReceivedGoodItemId { get; set; }
            [Required] public decimal? AmountPaid { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.OK;
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
            private readonly ISerialNumberHelper _serialHelper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, CommandsScheduler scheduler, IAuditLogHelper auditHelper, ISerialNumberHelper serialHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
                _serialHelper = serialHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.Model.PaymentDate.Value.Date >= x.StartDate &&
                    request.Model.PaymentDate.Value.Date <= x.EndDate);

                if (accountingPeriod == null) return new Response("Payment date does not have an accounting period");
                if (accountingPeriod.IsClosed) return new Response("Accounting period for this payment date is closed");

                var bankAccount = await _dbContext.BankAccounts.Where(x =>
                        x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsActive &&
                        x.Id == request.Model.BankAccountId)
                    .SingleOrDefaultAsync();

                if (bankAccount == null) return new Response("Bank account does not exist or has been deactivated");

                if (bankAccount.IsCash && request.Model.PaymentSource != PaymentMode.Cash)
                    return new Response("You cannot select a cash account for any payment source other than cash");

                if (request.Model.PaymentSource == PaymentMode.Cash && !bankAccount.IsCash)
                    return new Response("Payment source for a cash account can only be Cash");

                var transGroupId = SequentialGuid.Create();

                var paymentItems = new List<VendorPaymentModel>();

                var goodIds = request.Model.Payments.Select(x => x.ReceivedGoodItemId).ToList();
                var goods = await (from good in _dbContext.ReceivedGoodsLineItems.Where(x =>
                        x.CompanyId == request.CompanyId && goodIds.Contains(x.Id))
                    join gr in _dbContext.ReceivedGoods on good.GoodReceivedId equals gr.Id
                    join vend in _dbContext.Vendors on gr.VendorId equals vend.Id into goodVendor
                    from vend in goodVendor.DefaultIfEmpty()
                    select new {good, gr, vend}).ToDictionaryAsync(x=>x.good.Id);

                var today = DateTime.Today.Date;
                var lastUsed = await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, today, request.Model.Payments.Count);
                foreach (var payment in request.Model.Payments)
                {
                    var data = goods[payment.ReceivedGoodItemId.Value];
                    if (data == null) return new Response("Goods received not found");

                    var receivedItem = data.good;
                    var vendor = data.vend;

                    if (receivedItem.Balance < payment.AmountPaid)
                        return new Response(
                            $"Goods received {data.gr.GoodReceivedNo} cannot have a payment exceeding {receivedItem.Balance} for item {receivedItem.Item}");
                    
                    lastUsed++;
                    var refNo = Constants.GenerateSerialNo(Constants.SerialNoType.GeneralLedger, lastUsed);
                    var transRef = Constants.GenerateTransactionReference(today, lastUsed);

                    var description =
                        $"Payment for item {receivedItem.Item} in Goods Received {data.gr.GoodReceivedNo}. Amount => {payment.AmountPaid} ";

                    _dbContext.VendorPayments.Add(new VendorPayment
                    {
                        CompanyId = request.CompanyId,
                        VendorName = vendor?.Name,
                        VendorId = data.gr.VendorId,
                        PurchaseOrderId = data.gr.PurchaseOrderId,
                        ReceivedGoodId = data.gr.Id,
                        ReceivedGoodItemId = receivedItem.Id,
                        RemainingBalance = receivedItem.Balance - payment.AmountPaid,
                        PaymentSource = request.Model.PaymentSource.Value,
                        AmountPaid = payment.AmountPaid.Value,
                        BankAccountId = request.Model.BankAccountId,
                        ReferenceNo = transRef,
                        PaymentDate = request.Model.PaymentDate.Value,
                        CreatedBy = request.UserId
                    });

                    paymentItems.Add(new VendorPaymentModel
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        Amount = payment.AmountPaid.Value,
                        Description = description,
                        VendorId = data.gr.VendorId,
                        ReferenceNo = refNo,
                        GoodsReceivedId = receivedItem.Id,
                        PaymentDate = request.Model.PaymentDate.Value,
                        GoodsReceivedNumber = data.gr.GoodReceivedNo,
                        BankLedgerAccountId = bankAccount.LedgerAccountId
                    });
                    
                    if (vendor != null)
                    {
                        var newOwed = vendor.AmountOwed - payment.AmountPaid.Value;
                        vendor.AmountOwed = newOwed < 0 ? 0 : newOwed;
                        vendor.AmountReceived += payment.AmountPaid.Value;
                        vendor.LastTransactionDate = request.Model.PaymentDate;
                    }

                    bankAccount.CurrentBalance += payment.AmountPaid.Value;
                    receivedItem.Balance = receivedItem.Balance - payment.AmountPaid.Value < 0
                        ? 0
                        : receivedItem.Balance - payment.AmountPaid.Value;

                    data.gr.PaymentStatus = receivedItem.Balance == 0m 
                        ? PaymentStatus.Paid 
                        : PaymentStatus.PartiallyPaid;

                    // add to transactions table
                    _dbContext.Transactions.Add(new Transaction
                    {
                        CompanyId = request.CompanyId,
                        Source = PaymentMode.Account,
                        // CategoryId = ,
                        BankAccountId = request.Model.BankAccountId,
                        Amount = payment.AmountPaid.Value,
                        CreatedBy = request.UserId,
                        Description = description,
                        TransactionDate = request.Model.PaymentDate.Value,
                        ReferenceNo = transRef,
                        UserReferenceNo = transRef,
                        Type = TransactionType.PaySupplier,
                        TransactionGroupId = transGroupId,
                        Credit = 0,
                        Debit = payment.AmountPaid.Value,
                        Payee = vendor?.Name
                    });

                    _auditHelper.SaveAction(_dbContext, request.CompanyId,
                        new AuditModel
                        {
                            EntityType = (int) AuditLogEntityType.PurchaseOrder,
                            Action = (int) AuditLogPurchaseOrderAction.PaySupplier,
                            Description =
                                $"Pay supplier for {data.gr.GoodReceivedNo}. Amount = {payment.AmountPaid.Value}",
                            UserId = request.UserId
                        });
                }

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForVendorPayment
                    {
                        CompanyId = request.CompanyId,
                        Payments = paymentItems,
                        UserId = request.UserId,
                    });

                    return new Response();
                }

                return new Response("Could not add vendor payment");
            }
        }
    }
}
