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
using Spine.Common.Extensions;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Invoices.Jobs;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Invoices;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class AddInvoicePayment
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid InvoiceId { get; set; }

            [Required]
            public PaymentMode? PaymentSource { get; set; }
            [Required]
            public Guid? BankAccountId { get; set; }
            [Required]
            public decimal? AmountPaid { get; set; }
            [Required]
            public DateTime? PaymentDate { get; set; }

            public decimal? BankCharges { get; set; }
            public string Notes { get; set; }

            public List<Guid> Documents { get; set; }

            [RequiredIf(nameof(PaymentSource), PaymentMode.Cash, IsInverted = true)]
            public string ReferenceNo { get; set; }

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
                _serialHelper = serialHelper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.PaymentDate.Value.Date >= x.StartDate && request.PaymentDate.Value.Date <= x.EndDate);

                if (accountingPeriod == null) return new Response("Order date does not have an accounting period");
                if (accountingPeriod.IsClosed) return new Response("Accounting period for this order date is closed");

                var transGroupId = SequentialGuid.Create();
                var paymentId = SequentialGuid.Create();

                var data = await (from inv in _dbContext.Invoices.Where(x =>
                        x.CompanyId == request.CompanyId && x.Id == request.InvoiceId && !x.IsDeleted)
                    join cust in _dbContext.Customers on inv.CustomerId equals cust.Id into invoiceCustomer
                    from cust in invoiceCustomer.DefaultIfEmpty()
                    select new {inv, cust}).SingleAsync();

                var invoice = data.inv;
                if (invoice.InvoiceBalance < request.AmountPaid)
                    return new Response("Amount paid for this invoice is more than the balance due");

                var bankAccount = await _dbContext.BankAccounts.Where(x =>
                        x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsActive && x.Id == request.BankAccountId)
                    .SingleOrDefaultAsync();

                if (bankAccount == null) return new Response("Bank account does not exist or has been deactivated");

                if (bankAccount.IsCash && request.PaymentSource != PaymentMode.Cash)
                    return new Response("You cannot select a cash account for any payment source other than cash");

                if (request.PaymentSource == PaymentMode.Cash && !bankAccount.IsCash)
                    return new Response("Payment source for a cash account can only be Cash");

                var today = DateTime.Today;
                var lastUsed = await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, today, 1);
                lastUsed++;
                var refNo = Constants.GenerateSerialNo(Constants.SerialNoType.GeneralLedger, lastUsed);
                var transRef = Constants.GenerateTransactionReference(today, lastUsed);

                request.ReferenceNo ??= transRef;
                
                var description = $"Payment for invoice {invoice.InvoiceNoString}. Amount = {request.AmountPaid} ";

                _dbContext.InvoicePayments.Add(new InvoicePayment
                {
                    CompanyId = request.CompanyId,
                    InvoiceId = request.InvoiceId,
                    PaymentSource = request.PaymentSource.Value,
                    AmountPaid = request.AmountPaid.Value,
                    BankAccountId = request.BankAccountId,
                    BankCharges = request.BankCharges ?? 0,
                    Notes = request.Notes,
                    ReferenceNo = transRef,
                    UserReferenceNo = request.ReferenceNo,
                    PaymentDate = request.PaymentDate.Value,
                    IsPartPayment = invoice.InvoiceBalance != request.AmountPaid, //already prevented greater than from above
                    CreatedBy = request.UserId
                });

                var customer = data.cust;
                if (customer != null)
                {
                    var newOwed = customer.AmountOwed - request.AmountPaid.Value;
                    customer.AmountOwed = newOwed < 0 ? 0 : newOwed;
                    customer.AmountReceived += request.AmountPaid.Value;
                    customer.LastTransactionDate = request.PaymentDate;
                }

                bankAccount.CurrentBalance += request.AmountPaid.Value;
                invoice.InvoiceBalance -= request.AmountPaid.Value;

                //add payment  document
                if (!request.Documents.IsNullOrEmpty())
                {
                    foreach (var item in request.Documents)
                    {
                        _dbContext.Documents.Add(new Document
                        {
                            CompanyId = request.CompanyId,
                            DocumentId = item,
                            ParentItemId = paymentId
                        });
                    }
                }

                // add to transactions table
                _dbContext.Transactions.Add(new Transaction
                {
                    CompanyId = request.CompanyId,
                    Source = PaymentMode.Account,
                    // CategoryId = ,
                    BankAccountId = request.BankAccountId,
                    Amount = request.AmountPaid.Value,
                    CreatedBy = request.UserId,
                    Description = description,
                    TransactionDate = request.PaymentDate.Value,
                    ReferenceNo = transRef,
                    UserReferenceNo = request.ReferenceNo,
                    Type = TransactionType.ReceiveInvoicePayment,
                    TransactionGroupId = transGroupId,
                    Debit = 0,
                    Credit = request.AmountPaid.Value,
                    Payee = customer?.Name
                });

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int) AuditLogEntityType.Invoice,
                        Action = (int) AuditLogInvoiceAction.AddPayment,
                        Description =
                            $"Add payment for invoice {invoice.InvoiceNoString}. Amount = {request.AmountPaid.Value}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForInvoicePayment
                    {
                        CompanyId = request.CompanyId,
                        UserId = request.UserId,
                        BankLedgerAccountId = bankAccount.LedgerAccountId,
                        AccountingPeriodId = accountingPeriod.Id,
                        Model = new InvoicePaymentModel
                        {
                            InvoiceId = request.InvoiceId,
                            CustomerId = customer?.Id,
                            Amount = request.AmountPaid.Value,
                            Date = request.PaymentDate.Value,
                            Narration = description,
                            RefNo = refNo
                        }
                    });
                    return new Response();
                }

                return new Response("Could not add invoice payment");
            }
        }
    }
}
