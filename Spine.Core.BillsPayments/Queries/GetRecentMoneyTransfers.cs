using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.BillsPayments.Queries
{
    public static class GetRecentMoneyTransfers
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid UserId { get; set; }
        }

        public class Model
        {
            public decimal Amount { get; set; }
            public string RecipientAccountNo { get; set; }
            public string RecipientAccountName { get; set; }
            public string RecipientBank { get; set; }
            public string Remark { get; set; }
            public string RefNo { get; set; }

            public string FromBankName { get; set; }
            public string FromAccountName { get; set; }
            public string FromAccountNo { get; set; }

        }

        public class Response : List<Model>
        {
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IMapper _mapper;

            public Handler(SpineContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var items = await (from trans in _dbContext.MoneyTransfers.Where(x => x.CompanyId == request.CompanyId)
                                       //  && x.CreatedBy  == request.UserId )
                                   join bank in _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId) on trans.AccountFrom equals bank.Id
                                   orderby trans.DateCreated descending
                                   select new Model
                                   {
                                       Amount = trans.Amount,
                                       FromAccountName = bank.AccountName,
                                       FromAccountNo = bank.AccountNumber,
                                       FromBankName = bank.BankName,
                                       RecipientAccountName = trans.RecipientAccountName,
                                       RecipientAccountNo = trans.RecipientAccountNo,
                                       RecipientBank = trans.RecipientBank,
                                       RefNo = trans.RefNo,
                                       Remark = trans.Remark
                                   }).Take(5).ToListAsync();

                return _mapper.Map<Response>(items);

            }
        }

    }
}
