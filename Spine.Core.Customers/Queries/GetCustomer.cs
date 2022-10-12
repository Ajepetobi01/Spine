using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Customers.Queries
{
    public static class GetCustomer
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

        }

        public class Response
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string BusinessName { get; set; }
            public string BusinessType { get; set; }
            public string OperatingSector { get; set; }
            public List<AddressModel> BillingAddress { get; set; }
            public List<AddressModel> ShippingAddress { get; set; }
            public string Gender { get; set; }
            public string TIN { get; set; }
            public DateTime OnboardingDate { get; set; }
            public DateTime? LastTransactionDate { get; set; }
            public decimal TotalPurchases { get; set; }

            public List<NoteModel> Notes { get; set; }
        }

        public class NoteModel
        {
            public Guid Id { get; set; }
            public string Note { get; set; }
        }

        public class AddressModel
        {
            public Guid Id { get; set; }
            public bool IsPrimary { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
            public string State { get; set; }
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
                var query = from customer in _dbContext.Customers.Where(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted)
                            select new Response
                            {
                                Id = customer.Id,
                                Email = customer.Email,
                                Name = customer.Name,
                                Gender = customer.Gender,
                                TIN = customer.TIN,
                                OnboardingDate = customer.CreatedOn,
                                PhoneNumber = customer.PhoneNumber,
                                BusinessName = customer.BusinessName,
                                LastTransactionDate = customer.LastTransactionDate,
                                TotalPurchases = customer.TotalPurchases,
                                OperatingSector = customer.OperatingSector,
                                BusinessType = customer.BusinessType,
                            };

                var data = await query.SingleOrDefaultAsync();

                if (data != null)
                {
                    var addresses = await _dbContext.CustomerAddresses.Where(x =>
                            x.CompanyId == request.CompanyId && x.CustomerId == data.Id && !x.IsDeleted)
                        .ToListAsync();

                    data.BillingAddress = addresses.Where(x => x.IsBilling).Select(x => new AddressModel
                    {
                        AddressLine1 = x.AddressLine1,
                        AddressLine2 = x.AddressLine2,
                        IsPrimary = x.IsPrimary,
                        Id = x.Id,
                        State = x.State,
                        Country = x.Country,
                        PostalCode = x.PostalCode
                    }).ToList();

                    data.ShippingAddress = addresses.Where(x => !x.IsBilling).Select(x => new AddressModel
                    {
                        AddressLine1 = x.AddressLine1,
                        AddressLine2 = x.AddressLine2,
                        IsPrimary = x.IsPrimary,
                        Id = x.Id,
                        State = x.State,
                        Country = x.Country,
                        PostalCode = x.PostalCode
                    }).ToList();

                    data.Notes = await _dbContext.CustomerNotes.Where(x => x.CompanyId == request.CompanyId
                                                                           && !x.IsDeleted && x.CustomerId == data.Id)
                        .Select(x => new NoteModel {Note = x.Note, Id = x.Id})
                        .ToListAsync();
                }

                return data;
            }
        }

    }
}
