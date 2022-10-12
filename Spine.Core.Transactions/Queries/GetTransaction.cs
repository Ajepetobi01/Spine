using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Transactions.Queries
{
    public static class GetTransaction
    {
        public class Query : IRequest<Model>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid TransactionGroupId { get; set; }
            public Guid? CategoryId { get; set; }
            public Guid? BankAccountId { get; set; }
            public string Category { get; set; }
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

            public string Payee { get; set; }
            public string ChequeNo { get; set; }
            //  public string Source { get; set; }
            //    public string Type { get; set; }

            public List<Guid> Documents { get; set; }

        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Model> Handle(Query request, CancellationToken token)
            {
                var item = await (from trans in _dbContext.Transactions.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id)
                                      //  && x.CreatedBy  == request.UserId )
                                  join cat in _dbContext.TransactionCategories on trans.CategoryId equals cat.Id into transCat
                                  from cat in transCat.DefaultIfEmpty()
                                  where cat == null || !cat.IsDeleted
                                  join bank in _dbContext.BankAccounts on trans.BankAccountId equals bank.Id into transBank
                                  from bank in transBank.DefaultIfEmpty()
                                  select new Model
                                  {
                                      Id = trans.Id,
                                      CategoryId = cat.Id,
                                      BankAccountId = bank.Id,
                                      Category = cat.Name ?? "Uncategorized",
                                      BankName = bank.BankName,
                                      AccountName = bank.AccountName,
                                      AccountNumber = bank.AccountNumber,
                                      TransactionGroupId = trans.TransactionGroupId,
                                      TransactionDate = trans.TransactionDate,
                                      ReferenceNo = trans.ReferenceNo,
                                      UserReferenceNo = trans.UserReferenceNo,
                                      Description = trans.Description,
                                      Amount = trans.Amount,
                                      Debit = trans.Debit,
                                      Credit = trans.Credit,
                                      Payee = trans.Payee,
                                      ChequeNo = trans.ChequeNo,
                                      //Source = trans.Source.GetDescription(),
                                      //Type = trans.Type.GetDescription()
                                  }).SingleOrDefaultAsync();

                var docs = await _dbContext.Documents.Where(x => x.CompanyId == request.CompanyId && x.ParentItemId == item.Id)
                  .Select(x => new { x.ParentItemId, x.DocumentId }).ToListAsync();
                var docsLookup = docs.ToLookup(d => d.ParentItemId, f => f.DocumentId);
                item.Documents = docsLookup[item.Id].ToList();

                return item;

            }
        }

    }
}
