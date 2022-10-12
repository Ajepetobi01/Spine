using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Verification;

namespace Spine.Core.Transactions.Commands
{
    public static class UpdateBankAccount
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
            public string BankName { get; set; }
            [Required]
            public string BankCode { get; set; }
            //[Required]
            //public string AccountType { get; set; }
            [Required]
            public string AccountName { get; set; }
            [Required]
            public string AccountNumber { get; set; }

            [RequiredNotEmpty]
            public string Currency { get; set; }
            public string Description { get; set; }
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
            private readonly PaystackClient _paystackClient;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, PaystackClient paystackClient)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _paystackClient = paystackClient;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var account = await _dbContext.BankAccounts.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id);
                if (account == null)
                    return new Response("Account not found");

                if (account.IsCash) return new Response("This account cannot be updated");

                if (account.CurrentBalance != 0) return new Response("This account can no longer be updated as transactions have been completed with it");

                if (account.IntegrationProvider != BankAccountIntegrationProvider.None) return new Response("You cannot update an account that was not added manually");

                if (account.AccountNumber != request.AccountNumber)
                {
                    //verify account name
                    var handler = new VerifyAccountNumber.Handler();
                    var response = await handler.Handle(new VerifyAccountNumber.Request
                    { AccountNo = request.AccountNumber, BankCode = request.BankCode },
                                                                                                        _paystackClient);
                    if (response != null && response.Status)
                    {
                        request.AccountName = response.Data.AccountName;

                        account.AccountNumber = request.AccountNumber;
                        account.AccountName = request.AccountName;
                    }
                    else
                        return new Response($"Unable to verify Account Number. {response?.Message}");
                }

                account.BankCode = request.BankCode;
                account.BankName = request.BankName;
                account.Currency = request.Currency;
                account.Description = request.Description;

                account.LastModifiedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Transactions,
                    Action = (int)AuditLogTransactionAction.UpdateBankAccount,
                    UserId = request.UserId,
                    Description = $"Updated bank account {account.Id}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Bank account could not be updated");
            }
        }

    }
}
