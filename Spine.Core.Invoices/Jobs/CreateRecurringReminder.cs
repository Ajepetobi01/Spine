using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Models;
using Spine.Data;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Invoices.Commands
{
    public class CreateRecurringReminderCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }

    }

    public class CreateRecurringReminderHandler : IRequestHandler<CreateRecurringReminderCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<CreateRecurringReminderHandler> _logger;
        private readonly IEmailSender _emailSender;

        public CreateRecurringReminderHandler(SpineContext context, IEmailSender emailSender, ILogger<CreateRecurringReminderHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<Unit> Handle(CreateRecurringReminderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var invoice = await _dbContext.Invoices.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (invoice == null)
                {
                    _logger.LogInformation($"Invoice with Invoice Id {request.Id} for company Id {request.CompanyId} not found");
                    return Unit.Value;
                }

                if (invoice.InvoiceStatus == InvoiceStatus.Cancelled)
                {
                    _logger.LogInformation(
                        $"Invoice with Invoice Id {request.Id} for company Id {request.CompanyId} has been cancelled");
                    return Unit.Value;
                }
                
                if (invoice.InvoiceBalance == 0)
                {
                    _logger.LogInformation($"Invoice with Invoice Id {request.Id} for company Id {request.CompanyId} has no balance due. Reminder not sent");

                    RecurringJob.RemoveIfExists($"Invoice Reminder { invoice.Id}");
                    return Unit.Value;
                }

                var currency = await (from cur in _dbContext.Currencies.Where(x => x.Id == invoice.CurrencyId)
                                      select new CurrencyModel
                                      {
                                          Code = cur.Code,
                                          Id = cur.Id,
                                          Name = cur.Name,
                                          Symbol = cur.Symbol
                                      }).SingleAsync();

                var emailModel = new InvoiceReminder
                {
                    Currency = currency,
                    Subject = invoice.Subject,
                    InvoiceNo = invoice.InvoiceNoString,
                    DueDate = invoice.DueDate,
                    InvoiceDate = invoice.InvoiceDate,
                    Name = invoice.CustomerName,
                    Amount = invoice.InvoiceTotalAmount / invoice.RateToBaseCurrency,
                    BalanceDue = invoice.InvoiceBalance / invoice.RateToBaseCurrency
                };

                var emailSent = await _emailSender.SendTemplateEmail(invoice.CustomerEmail, $"{emailModel.AppName} - Invoice Reminder for Invoice {invoice.InvoiceNoString} ", EmailTemplateEnum.InvoiceReminder, emailModel);

                if (emailSent) _logger.LogInformation($"sent reminder for invoice no {invoice.InvoiceNoString} to {invoice.CustomerEmail}");
                else _logger.LogWarning("email sending failed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while sending reminder for invoice {request.Id} {ex.Message}");
            }
            return Unit.Value;
        }
    }
}
