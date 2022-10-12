using System;
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
using Spine.Data;
using Spine.Data.Entities.Transactions;
using Spine.Services;
using Spine.Services.HttpClients;
using Spine.Services.Mono;

namespace Spine.Core.Transactions.Commands
{
    public static class ConnectBankAccount
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [RequiredNotEmpty]
            public string Code { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Guid Id { get; set; }
            public Response(Guid id)
            {
                Id = id;
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
            private readonly MonoClient _monoClient;
            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper, MonoClient monoClient)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _monoClient = monoClient;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                //authenticate to get account Id
                var authHandler = new Authenticate.Handler();
                var authResponse = await authHandler.Handle(new Authenticate.Request { Code = request.Code, }, _monoClient);
                if (authResponse.Message.IsNullOrEmpty())
                {
                    //get account with account Id
                    var accountHandler = new GetAccount.Handler();
                    var accountResponse = await accountHandler.Handle(new GetAccount.Request { AccountId = authResponse.Id }, _monoClient);
                    if (accountResponse.Message.IsNullOrEmpty())
                    {
                        if (await _dbContext.BankAccounts.AnyAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                                                && x.AccountNumber == accountResponse.Account.AccountNumber))
                            return new Response("Account number already exist");

                        var cashAccountType =
                            await _dbContext.AccountTypes.SingleOrDefaultAsync(x =>
                                x.Id == AccountTypeConstants.Cash);

                        LedgerAccount ledgerAccount = null;
                        if (cashAccountType != null)
                        {
                            var lastSerial = await _dbContext.LedgerAccounts.Where(x =>
                                    x.CompanyId == request.CompanyId && x.AccountTypeId == cashAccountType.Id)
                                .MaxAsync(x => x.SerialNo);

                            var nextSerial = lastSerial + 1;
                            //create Ledger Account
                            ledgerAccount = new LedgerAccount
                            {
                                Id = SequentialGuid.Create(),
                                CompanyId = request.CompanyId,
                                CreatedBy = request.UserId,
                                AccountName = accountResponse.Account.Institution.Name + " - " + accountResponse.Account.AccountNumber,
                                AccountTypeId = cashAccountType.Id,
                                CreatedOn = DateTime.Today,
                                GLAccountNo = $"GL-{cashAccountType.Id:D1}{cashAccountType.AccountClassId:d2}{cashAccountType.AccountSubClassId:D2}{nextSerial:D2}",
                                SerialNo = nextSerial,
                            };
                            _dbContext.LedgerAccounts.Add(ledgerAccount);

                        }
                        
                        var bankAccount = new BankAccount
                        {
                            Id = SequentialGuid.Create(),
                            AccountId = accountResponse.Account.Id,
                            AccountCode = request.Code,
                            CompanyId = request.CompanyId,
                            AccountName = accountResponse.Account.Name,
                            AccountNumber = accountResponse.Account.AccountNumber,
                            Currency = accountResponse.Account.Currency,
                            //  CurrentBalance = accountResponse.Account.Balance,
                            AccountType = accountResponse.Account.Type,
                            BankCode = accountResponse.Account.Institution.BankCode,
                            BankName = accountResponse.Account.Institution.Name,
                            CreatedBy = request.UserId,
                            Description = "",
                            IsActive = true,
                            IntegrationProvider = BankAccountIntegrationProvider.Mono,
                            LedgerAccountId = ledgerAccount?.Id ?? Guid.Empty,
                        };
                        _dbContext.BankAccounts.Add(bankAccount);

                        _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                        {
                            EntityType = (int)AuditLogEntityType.Transactions,
                            Action = (int)AuditLogTransactionAction.CreateBankAccount,
                            UserId = request.UserId,
                            Description = $"Created new bank account via connect to bank {bankAccount.BankName} - {bankAccount.AccountNumber}"
                        });

                        return await _dbContext.SaveChangesAsync() > 0
                            ? new Response(bankAccount.Id)
                            : new Response("Bank account could not be saved");
                    }

                    return new Response($"Connect to bank failed. {accountResponse.Message}");
                }

                return new Response($"Connect to bank failed. {authResponse.Message}");
            }
        }
    }
}
