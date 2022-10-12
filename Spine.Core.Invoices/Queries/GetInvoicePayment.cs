using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Invoices.Queries
{
    public static class GetInvoicePayment
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public Guid InvoiceId { get; set; }
            public string Recipient { get; set; }
            public string InvoiceNo { get; set; }
            public decimal Amount { get; set; }
            public decimal BalanceDue { get; set; }
            public string Notes { get; set; }
            public string ReferenceNo { get; set; }
            public string UserReferenceNo { get; set; }

            public DateTime PaymentDate { get; set; }
            public string PaymentSource { get; set; }
            public string DepositTo { get; set; }

            public List<Guid> Documents { get; set; }
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
                var item = await (from payment in _dbContext.InvoicePayments.Where(x => x.CompanyId == request.CompanyId
                                                                               && request.Id == x.Id)
                                  join invoice in _dbContext.Invoices on payment.InvoiceId equals invoice.Id
                                  where invoice.CompanyId == request.CompanyId && !invoice.IsDeleted

                                  join bank in _dbContext.BankAccounts on payment.BankAccountId equals bank.Id into paymentBank
                                  from bank in paymentBank.DefaultIfEmpty()
                                  select new Response
                                  {
                                      PaymentSource = payment.PaymentSource.GetDescription(),
                                      PaymentDate = payment.PaymentDate,
                                      Amount = payment.AmountPaid,
                                      UserReferenceNo = payment.UserReferenceNo,
                                      ReferenceNo = payment.ReferenceNo,
                                      Notes = payment.Notes,
                                      DepositTo = bank.AccountName + " " + bank.AccountNumber,
                                      Id = payment.Id,

                                      BalanceDue = invoice.InvoiceBalance,
                                      Recipient = invoice.CustomerName,
                                      InvoiceId = invoice.Id,
                                      InvoiceNo = invoice.InvoiceNoString,
                                  }).SingleOrDefaultAsync();

                if (item != null)
                {
                    item.Documents = await _dbContext.Documents.Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == item.Id)
                    .Select(x => x.DocumentId).ToListAsync();
                }

                return item;
            }
        }

    }
}
