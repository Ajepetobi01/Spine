//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text.Json.Serialization;
//using System.Threading;
//using System.Threading.Tasks;
//using ExcelCsvExport.Attributes;
//using ExcelCsvExport.Enums;
//using ExcelCsvExport.Interfaces;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Spine.Common.Enums;
//using Spine.Common.Extensions;
//using Spine.Common.Helpers;
//using Spine.Core.Invoices.Helpers;
//using Spine.Data;

//namespace Spine.Core.Invoices.Queries
//{
//    public static class ExportInvoices
//    {
//        public class Query : GetExportData<Model>.Query, IExportFilter
//        {
//            [JsonIgnore]
//            public Guid UserId { get; set; }

//            public bool OnlyMine { get; set; }

//            public DateTime? StartDate { get; set; }
//            public DateTime? EndDate { get; set; }

//            public Guid? CustomerId { get; set; }

//            [Column(TypeName = "decimal(18,2")]
//            public decimal? MinAmount { get; set; }
//            [Column(TypeName = "decimal(18,2")]
//            public decimal? MaxAmount { get; set; }

//            public string CustomerName { get; set; }
//            public string InvoiceNo { get; set; }
//            public string Subject { get; set; }

//            //public string ItemName { get; set; }
//            //public string ItemDescription { get; set; }
//            public PaymentStatusFilter? Status { get; set; }

//            [JsonIgnore]
//            SpineContext IExportFilter.SpineContext { get; set; }

//            public async Task<IEnumerable<string>> FormatAsync()
//            {
//                // var d = await  SpineContext.Invoices.CountAsync();

//                return new List<string>
//                {
//                    $"Payment Status: {(Status == null ? "All" : Status.Value.GetDescription())}",
//                    $"Amount Range: {(MinAmount == null ? "N/A" : MinAmount.ToString())} - {(MaxAmount == null ? "N/A" : MaxAmount.ToString())}",
//                    $"Date Range: {StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}"
//                };
//            }
//        }

//        [FileName("Invoices")]
//        [ExportName("Invoice List")]
//        public class Model : IExportModel
//        {
//            [Width(20)]
//            [ExportHeader("Email Address", 2)]
//            public string Email { get; set; }
//            [Width(30)]
//            [ExportHeader("Recipient", 1)]
//            public string Recipient { get; set; }
//            [Width(20)]
//            [ExportHeader("Subject", 3)]
//            public string Subject { get; set; }
//            [Width(20)]
//            [ExportHeader("Invoice Number", 4)]
//            public string InvoiceNo { get; set; }
//            [Width(20)]
//            [ExportHeader("Amount ", ExportDataFormat.Currency, 5)]
//            public decimal Amount { get; set; }
//            [Width(20)]
//            [ExportHeader("Balance Due", ExportDataFormat.Currency, 6)]
//            public decimal BalanceDue { get; set; }

//            [Width(20)]
//            [ExportHeader("Invoice Date", ExportDataFormat.DateOrNA, 7)]
//            public DateTime InvoiceDate { get; set; }
//            [Width(20)]
//            [ExportHeader("Due Date", ExportDataFormat.DateOrNA, 8)]
//            public DateTime? InvoiceDueDate { get; set; }

//            //   public string PaymentSource { get; set; }

//            [Width(20)]
//            [ExportHeader("Status", 9)]
//            public string Status { get; set; }


//            public Guid CustomerId { get; set; }
//            public Guid CreatedBy { get; set; }
//        }

//        public class Handler : GetExportData<Model>.Handler, IRequestHandler<Query, GetExportData<Model>.Result>
//        {
//            private readonly SpineContext _context;
//            public Handler(SpineContext context) : base(context)
//            {
//                _context = context;
//            }

//            public async Task<GetExportData<Model>.Result> Handle(Query request, CancellationToken cancellationToken)
//            {
//                var result = await base.Handle(request);

//                if (request.StartDate == null) request.StartDate = DateTime.MinValue;
//                if (request.EndDate == null) request.EndDate = DateTime.MaxValue;

//                var query = from invoice in _context.Invoices.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
//                                                                             && request.StartDate <= x.InvoiceDate && x.InvoiceDate <= request.EndDate.GetValueOrDefault().ToEndOfDay())
//                            select new Model
//                            {
//                                CustomerId = invoice.CustomerId,
//                                CreatedBy = invoice.CreatedBy,
//                                Email = invoice.CustomerEmail,
//                                Recipient = invoice.CustomerName,
//                                Subject = invoice.Subject,
//                                InvoiceDate = invoice.InvoiceDate,
//                                InvoiceDueDate = invoice.DueDate != DateTime.MinValue ? invoice.DueDate : null,

//                                Amount = invoice.InvoiceTotalAmount,
//                                BalanceDue = invoice.InvoiceBalance,
//                                InvoiceNo = invoice.InvoiceNoString
//                            };

//                var today = Constants.GetCurrentDateTime();

//                if (request.OnlyMine) query = query.Where(x => x.CreatedBy == request.UserId);
//                if (request.MinAmount != null) query = query.Where(x => x.Amount >= request.MinAmount);
//                if (request.MaxAmount != null) query = query.Where(x => x.Amount <= request.MaxAmount);
//                if (request.CustomerId.HasValue) query = query.Where(x => x.CustomerId == request.CustomerId);
//                if (!request.InvoiceNo.IsNullOrEmpty()) query = query.Where(x => x.InvoiceNo.Contains(request.CustomerName));
//                if (!request.CustomerName.IsNullOrEmpty()) query = query.Where(x => x.Recipient.Contains(request.CustomerName));
//                if (!request.Subject.IsNullOrEmpty()) query = query.Where(x => x.Subject.Contains(request.Subject));
//                if (request.Status != null)
//                {
//                    switch (request.Status)
//                    {
//                        case PaymentStatusFilter.Paid:
//                            query = query.Where(x => x.BalanceDue == 0);
//                            break;
//                        case PaymentStatusFilter.Due:
//                            query = query.Where(x => x.BalanceDue > 0 && x.InvoiceDueDate < today);
//                            break;
//                        case PaymentStatusFilter.NotDue:
//                            query = query.Where(x => x.BalanceDue > 0 && x.InvoiceDueDate > today);
//                            break;
//                        default:
//                            break;
//                    }
//                }

//                query = query.OrderByDescending(x => x.InvoiceDate);
//                result.Items = await query.ToListAsync();
//                foreach (var item in result.Items)
//                {
//                    if (item.BalanceDue == 0)
//                        item.Status = "Completed";

//                    else
//                    {
//                        if (item.InvoiceDueDate.HasValue && item.InvoiceDueDate.Value.Date < today)
//                        {
//                            var dateDiff = (today - item.InvoiceDueDate.Value.Date).Duration().Days;
//                            item.Status = $"Overdue by {dateDiff} day(s)";
//                        }
//                        else
//                        {
//                            item.Status = "Not due";
//                        }
//                    }

//                }
//                return result;
//            }
//        }

//    }
//}
