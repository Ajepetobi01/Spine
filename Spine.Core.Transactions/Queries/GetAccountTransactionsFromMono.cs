using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Models;
using Spine.Data;
using Spine.Services.HttpClients;
using Spine.Services.Mono;

namespace Spine.Core.Transactions.Queries
{
    public static class GetAccountTransactionsFromMono
    {
        public class Query : IRequest<List<PreviewTransactionImport>>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid AccountId { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

        }

        public class Response : List<PreviewTransactionImport>
        {
        }


        public class Handler : IRequestHandler<Query, List<PreviewTransactionImport>>
        {
            private readonly SpineContext _dbContext;
            private readonly MonoClient _monoClient;
            private readonly IMapper _mapper;

            public Handler(SpineContext dbContext, MonoClient monoClient, IMapper mapper)
            {
                _dbContext = dbContext;
                _monoClient = monoClient;
                _mapper = mapper;
            }

            public async Task<List<PreviewTransactionImport>> Handle(Query request, CancellationToken token)
            {
                var monoAccountId = await _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && x.Id == request.AccountId
                                                    && !x.IsDeleted && !x.IsCash && x.IntegrationProvider == BankAccountIntegrationProvider.Mono)
                    .Select(x => x.AccountId).SingleOrDefaultAsync();

                if (monoAccountId.IsNullOrWhiteSpace()) return null;

                List<PreviewTransactionImport> data = null;
                var handler = new GetAccountTransactions.Handler();
                var response = await handler.Handle(new GetAccountTransactions.Request { AccountId = monoAccountId, StartDate = request.StartDate, EndDate = request.EndDate }, _monoClient);
                if (response.Message.IsNullOrEmpty())
                {
                    data = _mapper.Map<List<PreviewTransactionImport>>(response.Data);
                }

                return data;
            }
        }
    }
}
