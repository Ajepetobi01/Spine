using System;
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
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Data;
using Spine.Data.Entities.Transactions;
using Spine.Services;
using Spine.Services.HttpClients;

namespace Spine.Core.Transactions.Commands
{
    public static class CreateBankAccount
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

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

            //public decimal BankBalance { get; set; }
            //public decimal CurrentBalance { get; set; }
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
            private readonly PaystackClient _paystackClient;
            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper, PaystackClient paystackClient)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _paystackClient = paystackClient;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                if (int.TryParse(request.Currency, out var cur))
                {
                    return new Response("Invalid currency selected");
                }

                if (await _dbContext.BankAccounts.AnyAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.AccountNumber == request.AccountNumber))
                    return new Response("Account number already exist");
                
                // //verify account name
                // var handler = new VerifyAccountNumber.Handler();
                // var response = await handler.Handle(new VerifyAccountNumber.Request
                //     {AccountNo = request.AccountNumber, BankCode = request.BankCode}, _paystackClient);
                // if (response != null && response.Status)
                // {
                //     request.AccountName = response.Data.AccountName;
                // }
                // else
                //     return new Response($"Unable to verify Account Number. {response?.Message}");

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
                        AccountName = request.BankName + " - " + request.AccountNumber,
                        AccountTypeId = cashAccountType.Id,
                        CreatedOn = DateTime.Today,
                        GLAccountNo = $"GL-{cashAccountType.Id:D1}{cashAccountType.AccountClassId:d2}{cashAccountType.AccountSubClassId:D2}{nextSerial:D2}",
                        SerialNo = nextSerial,
                    };
                    _dbContext.LedgerAccounts.Add(ledgerAccount);

                }
                
                //create bankAccount
                var account = _mapper.Map<BankAccount>(request);
                account.LedgerAccountId = ledgerAccount.Id;
                account.IsActive = true;

                //bank balance

                _dbContext.BankAccounts.Add(account);

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Transactions,
                    Action = (int)AuditLogTransactionAction.CreateBankAccount,
                    UserId = request.UserId,
                    Description = $"Created new bank account {request.BankName} - {request.AccountNumber}"
                });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    return new Response(account.Id);
                }

                return new Response("Bank account could not be saved");
            }
        }

    }
}
