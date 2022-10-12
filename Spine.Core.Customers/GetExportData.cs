//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;
//using ExcelCsvExport.Interfaces;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Spine.Common.Helpers;
//using Spine.Data;

//namespace Spine.Core.Customers
//{
//    //interface to mark the models that can be exported
//    public interface IExportFilter : IFilter
//    {
//        [JsonIgnore]
//        SpineContext SpineContext { get; set; }
//    }


//    public static class GetExportData<T>
//    {
//        public class Query : IRequest<Result>
//        {
//            [JsonIgnore]
//            protected internal SpineContext SpineContext { get; set; }

//            [JsonIgnore]
//            public DateTime Requested { get; } = Constants.GetCurrentDateTime();

//            [JsonIgnore]
//            public virtual Guid CompanyId { get; set; }

//        }

//        public class Result
//        {
//            public string CompanyName { get; set; }
//            public ICollection<T> Items { get; set; }
//        }

//        public class Handler
//        {
//            private readonly SpineContext _dbContext;
//            public Handler(SpineContext dbContext)
//            {
//                _dbContext = dbContext;
//            }

//            public async Task<Result> Handle(Query message)
//            {
//                var result = await (from company in _dbContext.Companies.Where(x => x.Id == message.CompanyId)
//                                    select new Result
//                                    {
//                                        CompanyName = company.Name
//                                    }).FirstAsync();

//                message.SpineContext = _dbContext;
//                return result;
//            }
//        }
//    }
//}
