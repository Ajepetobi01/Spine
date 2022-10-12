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
//using Spine.Common.Extensions;
//using Spine.Data;

//namespace Spine.Core.Customers.Queries
//{
//    public static class ExportCustomers
//    {
//        public class Query : GetExportData<Model>.Query, IExportFilter
//        {
//            [JsonIgnore]
//            public Guid UserId { get; set; }
//            public bool OnlyMine { get; set; }

//            public DateTime? StartDate { get; set; }
//            public DateTime? EndDate { get; set; }

//            public string SearchBy { get; set; }

//            //public string Name { get; set; }
//            //public string Email { get; set; }
//            //public string Phone { get; set; }

//            //[Column(TypeName = "decimal(18,2")]
//            //public decimal? MinAmountOwed { get; set; }
//            //[Column(TypeName = "decimal(18,2")]
//            //public decimal? MaxAmountOwed { get; set; }
//            //[Column(TypeName = "decimal(18,2")]
//            //public decimal? MinAmountReceived { get; set; }
//            //[Column(TypeName = "decimal(18,2")]
//            //public decimal? MaxAmountReceived { get; set; }

//            [JsonIgnore]
//            SpineContext IExportFilter.SpineContext { get; set; }

//            public async Task<IEnumerable<string>> FormatAsync()
//            {
//                // var d = await  SpineContext.Customers.CountAsync();

//                return new List<string>
//                {
//                  //  $"Amount Received Range: {(MinAmountReceived == null ? "N/A" : MinAmountReceived.ToString())} - {(MaxAmountReceived == null ? "N/A" : MaxAmountOwed.ToString())}",
//                 //   $"Amount Owed Range: {(MinAmountOwed == null ? "N/A" : MinAmountOwed.ToString())} - {(MaxAmountOwed == null ? "N/A" : MaxAmountOwed.ToString())}",
//                    $"Date Range: {StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}"
//                };
//            }
//        }

//        [FileName("Customers")]
//        [ExportName("Customers List")]
//        public class Model : IExportModel
//        {
//            [Width(20)]
//            [ExportHeader("Email Address", 2)]
//            public string Email { get; set; }
//            [Width(30)]
//            [ExportHeader("Full Name", 1)]
//            public string Name { get; set; }

//            [Width(20)]
//            [ExportHeader("Phone Number", 3)]
//            public string PhoneNumber { get; set; }
//            [Width(20)]
//            [ExportHeader("Date Onboarded", ExportDataFormat.DateOrNA, 4)]
//            public string DateCreated { get; set; }

//            //   [Width(20)]
//            //    [ExportHeader("Total Amount Received", ExportDataFormat.Currency, 5)]
//            public decimal TotalReceived { get; set; }

//            //[Width(20)]
//            // [ExportHeader("Total Amount Owed", ExportDataFormat.Currency, 6)]
//            public decimal AmountOwed { get; set; }

//            [Width(20)]
//            [ExportHeader("Total Purchases", ExportDataFormat.Currency, 5)]
//            public decimal TotalPurchases { get; set; }
//            [Width(20)]
//            [ExportHeader("Last Transaction", ExportDataFormat.DateOrNA, 6)]
//            public DateTime? LastTransactionDate { get; set; }


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

//                var query = (from customer in _context.Customers.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
//                                  && request.StartDate <= x.CreatedOn && x.CreatedOn <= request.EndDate.GetValueOrDefault().ToEndOfDay())
//                             select new Model
//                             {
//                                 Email = customer.Email,
//                                 Name = customer.Name,
//                                 DateCreated = customer.CreatedOn.ToLongDateString(),
//                                 PhoneNumber = customer.PhoneNumber,
//                                 AmountOwed = customer.AmountOwed,
//                                 TotalReceived = customer.AmountReceived,
//                                 TotalPurchases = customer.TotalPurchases,
//                                 LastTransactionDate = customer.LastTransactionDate,
//                                 CreatedBy = customer.CreatedBy
//                             });

//                if (request.OnlyMine) query = query.Where(x => x.CreatedBy == request.UserId);
//                //if (!request.Name.IsNullOrEmpty()) query = query.Where(x => x.Name.Contains(request.Name));
//                //if (!request.Email.IsNullOrEmpty()) query = query.Where(x => x.Email.Contains(request.Email));
//                //if (!request.Phone.IsNullOrEmpty()) query = query.Where(x => x.PhoneNumber.Contains(request.Phone));
//                //if (request.MinAmountOwed != null) query = query.Where(x => x.AmountOwed >= request.MinAmountOwed);
//                //if (request.MaxAmountOwed != null) query = query.Where(x => x.AmountOwed <= request.MaxAmountOwed);
//                //if (request.MinAmountReceived != null) query = query.Where(x => x.TotalReceived >= request.MinAmountReceived);
//                //if (request.MaxAmountOwed != null) query = query.Where(x => x.TotalReceived <= request.MaxAmountReceived);

//                if (!request.SearchBy.IsNullOrEmpty())
//                    query = query.Where(x => x.Name.Contains(request.SearchBy) || x.Email.Contains(request.SearchBy) || x.PhoneNumber.Contains(request.SearchBy));

//                result.Items = await query.OrderBy(x => x.Name).ToListAsync();

//                return result;
//            }
//        }

//    }
//}
