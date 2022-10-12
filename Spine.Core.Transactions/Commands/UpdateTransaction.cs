using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Transactions.Commands
{
    public static class UpdateTransaction
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

            [Required]
            public DateTime? TransactionDate { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public string Payee { get; set; }

            public Guid? TransactionCategoryId { get; set; }

            //[Required]
            //public Guid? AccountId { get; set; }

            [Required]
            public decimal CreditAmount { get; set; }

            [Required]
            public decimal DebitAmount { get; set; }
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                //var account = await _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.AccountId).SingleOrDefaultAsync();
                //if (account == null) return new Response("Account does not exist");                

                var details = await (from trans in _dbContext.Transactions.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id)
                                     join account in _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsActive) on trans.BankAccountId equals account.Id
                                     select new { trans, account }).SingleOrDefaultAsync();

                if (details == null) return new Response("Transaction does not exist");

                var oldRecord = details.trans;

                var creditDiff = oldRecord.Credit - request.CreditAmount;
                var debitDiff = oldRecord.Debit - request.DebitAmount;

                details.account.CurrentBalance += creditDiff;
                details.account.CurrentBalance -= debitDiff;

                oldRecord.Credit = request.CreditAmount;
                oldRecord.Debit = request.DebitAmount;
                oldRecord.LastModifiedBy = request.UserId;
                oldRecord.CategoryId = request.TransactionCategoryId;
                oldRecord.Amount = request.CreditAmount + request.DebitAmount;
                oldRecord.Description = request.Description;
                oldRecord.TransactionDate = request.TransactionDate.Value;
                oldRecord.Payee = request.Payee;


                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                     new AuditModel
                     {
                         EntityType = (int)AuditLogEntityType.Transactions,
                         Action = (int)AuditLogTransactionAction.UpdateTransaction,
                         Description = $"Updated Transaction with reference number {oldRecord.ReferenceNo}",
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
