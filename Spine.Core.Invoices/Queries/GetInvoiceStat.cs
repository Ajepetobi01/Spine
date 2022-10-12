using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoiceStat
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
        }

        public class Response
        {
            public int InvoiceCount { get; set; }
            public decimal Generated { get; set; }
            public decimal Received { get; set; }
            public decimal Due { get; set; }
            public decimal Overdue { get; set; }

            //public double GeneratedPercentageGain { get; set; }
            //public double ReceivedPercentageGain { get; set; }
            //public double DuePercentageGain { get; set; }
            //public double OverduePercentageGain { get; set; }

        }


        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var invoices = await _dbContext.Invoices.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    .Select(x => new { x.InvoiceTotalAmount, x.InvoiceBalance, x.InvoiceDate, x.DueDate }).ToListAsync();

                var payments = await _dbContext.InvoicePayments.Where(x => x.CompanyId == request.CompanyId)
                    .Select(x => new { x.AmountPaid, x.PaymentDate }).ToListAsync();

                var amountGenerated = invoices.Sum(x => x.InvoiceTotalAmount);
                var amountDue = invoices.Where(x => x.DueDate.HasValue && DateTime.Today <= x.DueDate).Sum(x => x.InvoiceBalance);
                var amountOverdue = invoices.Where(x => x.DueDate.HasValue && DateTime.Today > x.DueDate).Sum(x => x.InvoiceBalance);
                var amountReceived = payments.Sum(x => x.AmountPaid);

                //var invoicesWithoutCurrentMonth = invoices.Where(x => x.InvoiceDate.Month != DateTime.Today.Month && x.InvoiceDate.Year != DateTime.Today.Year).ToList();
                //var paymentsWithoutCurrentMonth = payments.Where(x => x.PaymentDate.Month != DateTime.Today.Month && x.PaymentDate.Year != DateTime.Today.Year).ToList();

                //var prevAmountGenerated = invoicesWithoutCurrentMonth.Sum(x => x.InvoiceTotalAmount);
                //var prevAmountRecieved = paymentsWithoutCurrentMonth.Sum(x => x.AmountPaid);
                //var prevAmountDue = invoicesWithoutCurrentMonth.Where(x => x.DueDate.HasValue && DateTime.Today <= x.DueDate).Sum(x => x.InvoiceBalance);
                //var prevAmountOverdue = invoicesWithoutCurrentMonth.Where(x => x.DueDate.HasValue && DateTime.Today > x.DueDate).Sum(x => x.InvoiceBalance);

                return new Response
                {
                    InvoiceCount = invoices.Count,
                    Generated = amountGenerated,
                    Due = amountDue,
                    Overdue = amountOverdue,
                    Received = amountReceived,

                };
            }
        }

    }
}
