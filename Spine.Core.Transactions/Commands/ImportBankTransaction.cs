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
using Spine.Common.Converters;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Transactions.Commands
{
    public static class ImportBankTransaction
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid BankAccountId { get; set; }

            public List<ImportTransactionModel> Transactions { get; set; }
        }

        public class ImportTransactionModel
        {
            [Required]
            [JsonConverter(typeof(DateTimeConverterFactory))]
            public DateTime TransactionDate { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public string ReferenceNumber { get; set; }

            [Range(0, Double.MaxValue)]
            [JsonConverter(typeof(StringToDecimalConverter))]
            public decimal AmountSpent { get; set; }

            [Range(0, Double.MaxValue)]
            [JsonConverter(typeof(StringToDecimalConverter))]
            public decimal AmountReceived { get; set; }

            public string Payee { get; set; }

            public string ChequeNumber { get; set; }

            public Guid CategoryId { get; set; }
        }

        public class Response : BasicActionResult
        {
            public string ReturnMessage { get; set; }
            public Response(HttpStatusCode status, string message)
            {
                Status = status;
                ReturnMessage = message;
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
            private readonly ISerialNumberHelper _serialHelper;

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper, ISerialNumberHelper serialHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _serialHelper = serialHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var bankAccount = await _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsActive && x.Id == request.BankAccountId)
                                                                                         .SingleOrDefaultAsync();

                if (bankAccount == null) return new Response("Bank account does not exist or has been deactivated");

                var bankImports = await _dbContext.BankTransactions.Where(x => x.CompanyId == request.CompanyId).Select(x => new
                {
                    x.TransactionDate,
                    x.Payee,
                    x.Amount,
                    x.Description,
                    x.UserReferenceNo
                }).ToListAsync();

                var refNos = bankImports.Select(x => x.UserReferenceNo).ToList();
                var duplicate = 0;

                var count = request.Transactions.Count(x => !refNos.Contains(x.ReferenceNumber));
                var today = DateTime.Today.Date;
                var lastUsed = await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, today, count);

                foreach (var item in request.Transactions)
                {
                    if (refNos.Contains(item.ReferenceNumber))
                    {
                        duplicate++;
                        continue;
                    }

                    lastUsed++;
                    var refNo = Constants.GenerateTransactionReference(today, lastUsed);
                    var bankTransaction = _mapper.Map<BankTransaction>(item);
                    bankTransaction.ReferenceNo = refNo;
                    bankTransaction.UserReferenceNo = item.ReferenceNumber;
                    bankTransaction.BankAccountId = request.BankAccountId;
                    bankTransaction.CreatedBy = request.UserId;
                    bankTransaction.CompanyId = request.CompanyId;
                    bankTransaction.Status = TransactionStatus.Processed;

                    _dbContext.BankTransactions.Add(bankTransaction);

                    var transaction = _mapper.Map<Transaction>(item);
                    transaction.BankAccountId = request.BankAccountId;
                    transaction.ReferenceNo = refNo;
                    transaction.UserReferenceNo = item.ReferenceNumber;
                    transaction.CreatedBy = request.UserId;
                    transaction.CompanyId = request.CompanyId;

                    // add to transactions table
                    _dbContext.Transactions.Add(transaction);

                    _auditHelper.SaveAction(_dbContext, request.CompanyId,
                      new AuditModel
                      {
                          EntityType = (int)AuditLogEntityType.Transactions,
                          Action = (int)AuditLogTransactionAction.ImportBankTransaction,
                          Description = $"Import {request.Transactions.Count - duplicate} bank transactions",
                          UserId = request.UserId
                      });
                }

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    var retMessage = duplicate == 0 ? "Import successful" : $"Import successful with {duplicate} reference number(s) skipped";
                    return new Response(HttpStatusCode.Created, retMessage);
                }

                return new Response(duplicate > 0 ? $"No record uploaded. {duplicate} reference numbers exist" : "No record uploaded");
            }
        }

    }
}
