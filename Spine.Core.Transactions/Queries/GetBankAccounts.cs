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
using Spine.Common.Enums;

namespace Spine.Core.Transactions.Queries
{
    public static class GetBankAccounts
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public bool ExcludeCash { get; set; }
            public bool IncludeInactive { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public bool IsCash { get; set; }
            public bool IsActive { get; set; }
            public bool IsAutoCreated { get; set; }
            public string AccountName { get; set; }
            public string AccountNumber { get; set; }
            public string BankName { get; set; }
            public decimal Balance { get; set; }
            public string AccountType { get; set; }
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
                var accounts = await (from bank in _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                      && (!request.ExcludeCash || !x.IsCash)
                                      && (request.IncludeInactive || x.IsActive))
                                      select new Model
                                      {
                                          Id = bank.Id,
                                          IsCash = bank.IsCash,
                                          IsActive = bank.IsActive,
                                          IsAutoCreated = bank.IntegrationProvider != BankAccountIntegrationProvider.None,
                                          BankName = bank.BankName,
                                          AccountName = bank.AccountName,
                                          AccountNumber = bank.AccountNumber,
                                          AccountType = bank.AccountType,
                                          Balance = bank.CurrentBalance
                                      }).ToListAsync();

                return _mapper.Map<Response>(accounts);
            }
        }

    }
}
