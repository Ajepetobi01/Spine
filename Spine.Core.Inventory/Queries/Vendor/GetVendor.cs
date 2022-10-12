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
    public static class GetVendor
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
            
            public TypeOfVendor VendorTypeEnum { get; set; }
            public string VendorType { get; set; }

            public DateTime? LastTransactionDate { get; set; }
            public string DisplayName { get; set; }
            public string OperatingSector { get; set; }
            public string RcNumber { get; set; }
            public string Website { get; set; }
            public string TIN { get; set; }
            public decimal Receivables { get; set; }
            public decimal Payables { get; set; }

            public decimal TotalPurchases { get; set; }

            [JsonIgnore] public Status StatusEnum { get; set; }
            public string Status { get; set; }

            public DateTime CreatedOn { get; set; }

            public List<AddressModel> BillingAddress { get; set; }
            public List<AddressModel> ShippingAddress { get; set; }
            
            public BankAccountModel BankAccount { get; set; }
            public ContactPersonModel ContactPerson { get; set; }
            
        }

        public class BankAccountModel
        {
            public Guid Id { get; set; }
            public string BankName { get; set; }
            public string BankCode { get; set; }
            public string AccountName { get; set; }
            public string AccountNumber { get; set; }
        }
        
        public class ContactPersonModel
        {
            public Guid Id { get; set; }
            public string Role { get; set; }
            public string FullName { get; set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
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
                var item = await (from inv in _dbContext.Vendors.Where(x =>
                        x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.Id)
                    select new Response
                    {
                        Id = inv.Id,
                        Name = inv.Name,
                        LastTransactionDate = inv.LastTransactionDate,
                        CreatedOn = inv.CreatedOn,
                        Payables = inv.AmountOwed,
                        Receivables = inv.AmountReceived,
                        TotalPurchases = inv.AmountOwed + inv.AmountReceived,
                        Status = inv.Status.GetDescription(),
                        StatusEnum = inv.Status,
                        VendorTypeEnum = inv.VendorType,
                        VendorType = inv.VendorType.GetDescription(),
                        Email = inv.Email,
                        BusinessName = inv.BusinessName,
                        PhoneNo = inv.PhoneNumber,
                        DisplayName = inv.DisplayName,
                        OperatingSector = inv.OperatingSector,
                        Website = inv.Website,
                        TIN = inv.TIN,
                        RcNumber = inv.RcNumber,
                    }).SingleOrDefaultAsync();

                if (item == null) return null;

                item.BankAccount = await _dbContext.VendorBankAccounts.Where(x =>
                        x.CompanyId == request.CompanyId && x.VendorId == item.Id && !x.IsDeleted)
                    .Select(x => new BankAccountModel
                    {
                        Id = x.Id,
                        BankCode = x.BankCode,
                        BankName = x.BankName,
                        AccountName = x.AccountName,
                        AccountNumber = x.AccountNumber
                    }).SingleOrDefaultAsync();

                item.ContactPerson = await (from person in _dbContext.VendorContactPersons.Where(x =>
                        x.CompanyId == request.CompanyId && x.VendorId == item.Id && !x.IsDeleted)
                    select new ContactPersonModel
                    {
                        Id = person.Id,
                        Role = person.Role,
                        FullName = person.FullName,
                        EmailAddress = person.EmailAddress,
                        PhoneNumber = person.PhoneNumber
                    }).SingleOrDefaultAsync();

                if (item.ContactPerson != null) item.ContactPerson.Role = item.ContactPerson.Role.GetFirstPart();

                var addresses = await _dbContext.VendorAddresses.Where(x =>
                        x.CompanyId == request.CompanyId && x.VendorId == item.Id && !x.IsDeleted)
                    .ToListAsync();

                item.BillingAddress = addresses.Where(x => x.IsBilling).Select(x => new AddressModel
                {
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    IsPrimary = x.IsPrimary,
                    Id = x.Id,
                    State = x.State,
                    Country = x.Country,
                    PostalCode = x.PostalCode
                }).ToList();

                item.ShippingAddress = addresses.Where(x => !x.IsBilling).Select(x => new AddressModel
                {
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    IsPrimary = x.IsPrimary,
                    Id = x.Id,
                    State = x.State,
                    Country = x.Country,
                    PostalCode = x.PostalCode
                }).ToList();

                return item;
            }
        }
    }
}
