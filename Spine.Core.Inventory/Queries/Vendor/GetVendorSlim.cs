using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Attributes;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Helpers;

namespace Spine.Core.Inventories.Queries.Vendor
{
    public static class GetVendorSlim
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public Guid Id { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            
            public string PhoneNo { get; set; }
            public string BusinessName { get; set; }
            public string DisplayName { get; set; }
            public decimal Receivables { get; set; }
            public decimal Payables { get; set; }

            public decimal TotalPurchases { get; set; }
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
                var item = await (from inv in _dbContext.Vendors.Where(x =>
                        x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id)
                    select new Response
                    {
                        Id = inv.Id,
                        Name = inv.Name,
                        Payables = inv.AmountOwed,
                        Receivables = inv.AmountReceived,
                        TotalPurchases = inv.AmountOwed + inv.AmountReceived,
                        Email = inv.Email,
                        BusinessName = inv.BusinessName,
                        PhoneNo = inv.PhoneNumber,
                        DisplayName = inv.DisplayName,
                    }).SingleOrDefaultAsync();

                return item;
            }
        }
    }
}
