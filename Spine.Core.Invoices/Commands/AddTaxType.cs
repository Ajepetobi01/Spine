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
using Spine.Data;
using Spine.Data.Entities.Invoices;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class AddTaxType
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public string Tax { get; set; }

            [Range(0, 100)]
            public double TaxRate { get; set; }

            public bool IsCompound { get; set; }
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
                if (await _dbContext.TaxTypes.AnyAsync(x => x.CompanyId == request.CompanyId && x.Tax == request.Tax && !x.IsDeleted))
                    return new Response("Tax type exists");

                var taxAccountType =
                    await _dbContext.AccountTypes.SingleOrDefaultAsync(x =>
                        x.Id == AccountTypeConstants.TaxPayable);

                LedgerAccount ledgerAccount = null;
                if (taxAccountType != null)
                {
                    var lastSerial = await _dbContext.LedgerAccounts.Where(x =>
                            x.CompanyId == request.CompanyId && x.AccountTypeId == taxAccountType.Id)
                        .MaxAsync(x => x.SerialNo);

                    var nextSerial = lastSerial + 1;
                    //create Ledger Account
                    ledgerAccount = new LedgerAccount
                    {
                        Id = SequentialGuid.Create(),
                        CompanyId = request.CompanyId,
                        CreatedBy = request.UserId,
                        AccountName = request.Tax + " Ledger",
                        AccountTypeId = taxAccountType.Id,
                        CreatedOn = DateTime.Today,
                        GLAccountNo = $"GL-{taxAccountType.Id:D1}{taxAccountType.AccountClassId:d2}{taxAccountType.AccountSubClassId:D2}{nextSerial:D2}",
                        SerialNo = nextSerial,
                    };
                    _dbContext.LedgerAccounts.Add(ledgerAccount);
                }
                
                var tax = new TaxType
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    Tax = request.Tax,
                    IsActive = true,
                    LedgerAccountId = ledgerAccount?.Id ?? Guid.Empty,
                    TaxRate = request.TaxRate,
                    IsCompound = request.IsCompound
                };

                _dbContext.TaxTypes.Add(tax);
                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.TaxType,
                    Action = (int)AuditLogTaxTypeAction.Create,
                    UserId = request.UserId,
                    Description = $"Added tax type {tax.Tax}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Tax type could not be created");
            }
        }

    }
}
