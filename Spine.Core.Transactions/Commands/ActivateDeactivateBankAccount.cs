using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Transactions.Commands
{
    public static class ActivateDeactivateBankAccount
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

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
                var account = await _dbContext.BankAccounts.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id);
                if (account == null)
                    return new Response("Account not found");

                if (account.IsCash) return new Response("This account cannot be marked (in)active");

                var action = "";
                if (account.IsActive) // deactivate active account
                {
                    account.DateDeactivated = Constants.GetCurrentDateTime();
                    action = $"Marked bank account {account.BankName} - {account.AccountNumber} as inactive";
                }
                else // activate inactive account
                {
                    action = $"Marked bank account {account.BankName} - {account.AccountNumber} as active ";
                    account.DateDeactivated = null;
                }

                account.IsActive = !account.IsActive; //toggle active status
                account.LastModifiedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Transactions,
                    Action = (int)AuditLogTransactionAction.ActivateDeactivateBankAccount,
                    UserId = request.UserId,
                    Description = action
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Bank account status could not be changed");
            }
        }

    }
}
