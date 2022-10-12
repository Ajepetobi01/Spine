using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Transactions.Commands
{
    public static class DeleteBankAccount
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
                Status = HttpStatusCode.NoContent;
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
                var account = await _dbContext.BankAccounts.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (account == null) return new Response("Account not found");

                if (account.IsCash) return new Response("Cash account cannot be deleted");

                account.IsDeleted = true;
                account.DeletedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Transactions,
                    Action = (int)AuditLogTransactionAction.DeleteBankAccount,
                    UserId = request.UserId,
                    Description = $"Deleted Bank account type {account.AccountNumber}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Bank account could not be deleted");
            }
        }

    }
}
