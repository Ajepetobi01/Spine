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
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Transactions.Jobs;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Transactions.Commands
{
    public static class AddManualTransaction
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public DateTime? TransactionDate { get; set; }

            //[Required]
            //public TransactionType? TransactionType { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public string Payee { get; set; }

            public Guid? TransactionCategoryId { get; set; }

            [Required]
            public Guid? AccountId { get; set; }

            [Required]
            public decimal CreditAmount { get; set; }

            [Required]
            public decimal DebitAmount { get; set; }

            public Guid? DocumentId { get; set; }

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

            public Handler(SpineContext context, ISerialNumberHelper serialHelper, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _serialHelper = serialHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var transGroupId = SequentialGuid.Create();
                var transactionId = SequentialGuid.Create();
                
                var today = DateTime.Today.Date;
                var lastUsed = await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, today, 1);
                var refNo = Constants.GenerateTransactionReference(today, lastUsed + 1);

                var account = await _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsActive && x.Id == request.AccountId)
                                                                                           .SingleOrDefaultAsync();

                if (account == null) return new Response("Bank account does not exist or has been deactivated");

                account.CurrentBalance += request.CreditAmount;
                account.CurrentBalance -= request.DebitAmount;

                // add to transactions table
                _dbContext.Transactions.Add(new Transaction
                {
                    Id = transactionId,
                    CompanyId = request.CompanyId,
                    Source = account.IsCash ? PaymentMode.Cash : PaymentMode.Account,
                    BankAccountId = account.Id,
                    CategoryId = request.TransactionCategoryId,
                    Amount = request.DebitAmount + request.CreditAmount,
                    CreatedBy = request.UserId,
                    Description = request.Description,
                    TransactionDate = request.TransactionDate.Value,
                    ReferenceNo = refNo,
                    UserReferenceNo = refNo,
                    Payee = request.Payee,
                    Type = TransactionType.None, // request.TransactionType.Value,
                    TransactionGroupId = transGroupId,
                    Debit = request.DebitAmount,
                    Credit = request.CreditAmount
                });

                // var items = new List<TransactionModel>();
                // items.Add(new TransactionModel
                // {
                //     AccountingPeriodId = accountingPeriod.Id,
                //     DebitAmount = request.DebitAmount,
                //     CreditAmount = request.CreditAmount,
                //     Description = request.Description,
                //     TransactionId = transactionId,
                //     PaymentDate = request.TransactionDate.Value,
                //     LedgerAccountId = account.LedgerAccountId,
                //     ReferenceNo = refNo
                // });
                
                //add transaction  document
                if (request.DocumentId != null)
                {
                    _dbContext.Documents.Add(new Document
                    {
                        CompanyId = request.CompanyId,
                        DocumentId = request.DocumentId.Value,
                        ParentItemId = transactionId
                    });
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                     new AuditModel
                     {
                         EntityType = (int)AuditLogEntityType.Transactions,
                         Action = (int)AuditLogTransactionAction.AddManualTransaction,
                         Description = $"Add manual transaction with reference number {refNo}",
                         UserId = request.UserId
                     });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    return new Response();
                }

                return new Response(HttpStatusCode.BadRequest);
            }
        }
    }
}
