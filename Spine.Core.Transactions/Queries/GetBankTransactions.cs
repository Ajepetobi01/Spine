using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Models;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Transactions.Queries
{
    public static class GetBankTransactions
    {
        public class Query : IRequest<Response>, IPagedRequest
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            public Guid? AccountId { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public bool All { get; set; }
            public int Page { get; set; } = 1;
            public int PageLength { get; set; } = 25;
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string BankName { get; set; }
            public string AccountNumber { get; set; }
            public string AccountName { get; set; }
            public DateTime TransactionDate { get; set; }
            public string Description { get; set; }
            public string ReferenceNo { get; set; }
            public string UserReferenceNo { get; set; }
            public decimal Amount { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }

            public string ChequeNo { get; set; }
            public string Payee { get; set; }
            public string UploadedBy { get; set; }
            public string Status { get; set; }

            [JsonIgnore]
            public TransactionStatus StatusEnum { get; set; }
            [JsonIgnore]
            public Guid BankAccountId { get; set; }
        }

        public class Response : PagedResult<Model>
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
                var query = from trans in _dbContext.BankTransactions.Where(x => x.CompanyId == request.CompanyId)
                            join bank in _dbContext.BankAccounts on trans.BankAccountId equals bank.Id
                            join user in _dbContext.Users on trans.CreatedBy equals user.Id
                            select new Model
                            {
                                Id = trans.Id,
                                BankAccountId = bank.Id,
                                Payee = trans.Payee,
                                ChequeNo = trans.ChequeNo,
                                BankName = bank.BankName,
                                AccountName = bank.AccountName,
                                AccountNumber = bank.AccountNumber,
                                TransactionDate = trans.TransactionDate,
                                ReferenceNo = trans.ReferenceNo,
                                UserReferenceNo = trans.UserReferenceNo,
                                Description = trans.Description,
                                Amount = trans.Amount,
                                Debit = trans.Debit,
                                Credit = trans.Credit,
                                UploadedBy = user.FullName,
                                Status = trans.Status.GetDescription(),
                                StatusEnum = trans.Status
                            };

                if (!request.All) query = query.Where(x => x.StatusEnum == TransactionStatus.Pending);
                if (request.AccountId.HasValue) query = query.Where(x => x.BankAccountId == request.AccountId);

                if (request.StartDate.HasValue) query = query.Where(x => x.TransactionDate >= request.StartDate);
                if (request.EndDate.HasValue) query = query.Where(x => x.TransactionDate <= request.EndDate);

                query = query.OrderByDescending(x => x.TransactionDate);
                if (request.Page == 0)
                {
                    return _mapper.Map<Response>(await query.ToListAsync());
                }
                
                return await query.ToPageResultsAsync<Model, Response>(request);
            }
        }

    }
}
