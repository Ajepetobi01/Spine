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
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Journal
{
    public static class AddJournal
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore] public Guid CompanyId { get; set; }
            [JsonIgnore] public Guid UserId { get; set; }

            [Required] public string ProductName { get; set; }
            public string Description { get; set; }
            public bool IsCashBased { get; set; }
            [RequiredNonDefault] public DateTime? JournalDate { get; set; }

            [RequiredNonDefault] public int? CurrencyId { get; set; }
            public decimal RateToBaseCurrency { get; set; }

            [RequiredNotEmpty] public List<LineItemModel> LineItems { get; set; }
        }

        public class LineItemModel
        {
            [Required]
            public Guid? LedgerAccountId { get; set; }
            public string Description { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            
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
            private readonly CommandsScheduler _scheduler;
            private readonly ISerialNumberHelper _serialHelper;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, ISerialNumberHelper serialHelper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
                _serialHelper = serialHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                if (request.LineItems.Sum(x => x.Credit) != request.LineItems.Sum(x => x.Debit))
                    return new Response("Total Credit must be equal to the total Debit amount");
                
                // var preference = await _dbContext.InvoicePreferences.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId);
                // if (preference == null) return new Response("You must set Base currency in invoice settings before posting a journal");

                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.JournalDate.Value.Date >= x.StartDate && request.JournalDate.Value.Date <= x.EndDate);
               
                if (accountingPeriod == null || accountingPeriod.IsClosed) return new Response("Journal date does not have an open accounting period");

                var lastUsed =
                    await _serialHelper.GetLastUsedJournalNo(_dbContext, request.CompanyId, 1);

                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();

                if (request.CurrencyId != baseCurrency)
                {
                    if (request.RateToBaseCurrency == 0.0m)
                        return new Response("Enter the rate to local currency if journal has a different currency");
                    
                    // multiply by the rate to base currency to save the amounts in the company base currency
                    foreach (var item in request.LineItems)
                    {
                        item.Debit *= request.RateToBaseCurrency;
                        item.Credit *= request.RateToBaseCurrency;
                    }
                }
                else
                {
                    request.RateToBaseCurrency = 1;
                }

                var journalNo = Constants.GenerateSerialNo(Constants.SerialNoType.Journal, lastUsed +1);
                var journalId = SequentialGuid.Create();

                var model = new List<JournalModel>();
                foreach (var item in request.LineItems)
                {
                    _dbContext.JournalPostings.Add(new JournalPosting
                    {
                        CompanyId = request.CompanyId,
                        JournalId = journalId,
                        PostingDate = request.JournalDate.Value,
                        ProductName = request.ProductName,
                        Description = request.Description,
                        IsCashBased = request.IsCashBased,
                        CurrencyId = request.CurrencyId.Value,
                        BaseCurrencyId = baseCurrency,
                        RateToBaseCurrency = request.RateToBaseCurrency,
                        CreatedBy = request.UserId,
                        JournalNo = journalNo,
                        
                        ItemDescription = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        LedgerAccountId = item.LedgerAccountId.Value
                    });
                    
                    model.Add(new JournalModel
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        JournalId = journalId,
                        JournalDate = request.JournalDate.Value,
                        JournalNo = journalNo,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        CurrencyId = request.CurrencyId.Value,
                        ExchangeRate = request.RateToBaseCurrency,
                        LedgerAccountId = item.LedgerAccountId.Value
                    });
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Transactions,
                        Action = (int)AuditLogTransactionAction.PostJournal,
                        Description = $"Post new journal {journalNo}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForJournalPosting
                    {
                        CompanyId = request.CompanyId,
                        UserId = request.UserId,
                        Journals = model
                    }, $"Post Journal {journalNo}");

                    return new Response();
                }

                return new Response("Unable to post journal");

            }
        }

    }
}
