using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Inventories.Queries
{
    public static class GetInventoryLedgerAccounts
    {
        public class Query : IRequest<Response>
        {
            public Guid CompanyId { get; set; }
            public int AccountTypeId { get; set; }

        }

        public class Model
        {
            public Guid Id { get; set; }
            public string AccountName { get; set; }
            public string GLAccountNo { get; set; }
           
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
                var accounts = await (from acct in _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.CompanyId 
                         && x.AccountTypeId == request.AccountTypeId && !x.IsDeleted)
                                      select new Model
                                      {
                                          Id = acct.Id,
                                          AccountName = acct.AccountName,
                                          GLAccountNo = acct.GLAccountNo
                                      }).ToListAsync();

                return _mapper.Map<Response>(accounts);

            }
        }

    }
}
