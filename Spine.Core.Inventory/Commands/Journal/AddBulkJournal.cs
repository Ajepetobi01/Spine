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
using Spine.Common.Converters;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Journal
{
    public static class AddBulkJournal
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [RequiredNotEmpty]
            public List<BulkJournalModel> Journals { get; set; }
        }

        public class BulkJournalModel
        {
            [RequiredNonDefault] 
            [JsonConverter(typeof(DateTimeConverterFactory))]
            public DateTime JournalDate { get; set; }
            
            [Required] public string ProductName { get; set; }
            public string Description { get; set; }
            public string CashBased { get; set; }
            
            
            [Required]
            public string LedgerAccount { get; set; }
            public string ItemDescription { get; set; }
            
            [JsonConverter(typeof(StringToDecimalConverter))]
            public decimal Debit { get; set; }
            [JsonConverter(typeof(StringToDecimalConverter))]
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
            private readonly ISerialNumberHelper _serialHelper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, CommandsScheduler scheduler, IAuditLogHelper auditHelper, ISerialNumberHelper serialHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
                _serialHelper = serialHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var items = request.Journals.GroupBy(x => x.JournalDate).Select(x => new
                {
                    x.Key,
                    Items = x.Select(y => new BulkJournalModel
                    {
                        JournalDate = x.Key,
                        ProductName = y.ProductName,
                        Description = y.Description,
                        CashBased = y.CashBased,
                        LedgerAccount = y.LedgerAccount,
                        Credit = y.Credit,
                        Debit = y.Debit,
                        ItemDescription = y.ItemDescription
                    }).ToList()
                }).ToList();

                var lastUsed =
                    await _serialHelper.GetLastUsedJournalNo(_dbContext, request.CompanyId,
                        items.Sum(x => x.Items.Count));
                
                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();

                var ledgerAccounts = await _dbContext.LedgerAccounts
                    .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    .Select(x => new {x.Id, x.AccountName}).ToDictionaryAsync(x => x.AccountName, y => y.Id);

                var accountingPeriods = await _dbContext.AccountingPeriods.Where(x => x.CompanyId == request.CompanyId)
                    .ToListAsync();
                
                var model = new List<JournalModel>();
                foreach (var item in items)
                {
                    if (item.Items.Sum(x => x.Credit) != item.Items.Sum(x => x.Debit))
                        return new Response("Total credit must be equal to the total Debit amount for postings on same date");
                    
                    var accountingPeriod = accountingPeriods.FirstOrDefault(x => item.Key.Date >= x.StartDate && item.Key.Date <= x.EndDate);
               
                    if (accountingPeriod == null || accountingPeriod.IsClosed) return new Response($"Journal date {item.Key} does not have an open accounting period");

                    lastUsed++;
                    var journalNo = Constants.GenerateSerialNo(Constants.SerialNoType.Journal, lastUsed);
                    var journalId = SequentialGuid.Create();
                    
                    foreach (var lineItem in item.Items)
                    {
                        var ledgerAccountId = ledgerAccounts[lineItem.LedgerAccount];
                        _dbContext.JournalPostings.Add(new JournalPosting
                        {
                            CompanyId = request.CompanyId,
                            JournalId = journalId,
                            PostingDate = lineItem.JournalDate,
                            ProductName = lineItem.ProductName,
                            Description = lineItem.Description,
                            IsCashBased = lineItem.CashBased.ToLower() == "true",
                            CurrencyId = baseCurrency,
                            BaseCurrencyId = baseCurrency,
                            RateToBaseCurrency = 1,
                            CreatedBy = request.UserId,
                            JournalNo = journalNo,
                        
                            ItemDescription = lineItem.ItemDescription,
                            Debit = lineItem.Debit,
                            Credit = lineItem.Credit,
                            LedgerAccountId = ledgerAccountId
                        });

                        model.Add(new JournalModel
                        {
                            AccountingPeriodId = accountingPeriod.Id,
                            JournalId = journalId,
                            JournalDate = lineItem.JournalDate,
                            JournalNo = journalNo,
                            Debit = lineItem.Debit,
                            Credit = lineItem.Credit,
                            CurrencyId = baseCurrency,
                            ExchangeRate = 1,
                            LedgerAccountId = ledgerAccountId
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
                }

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForJournalPosting
                    {
                        CompanyId = request.CompanyId,
                        UserId = request.UserId,
                        Journals = model
                    }, $"Post Journals");

                    return new Response();
                }

                return new Response("Unable to post journal");

            }
        }
    }
}
