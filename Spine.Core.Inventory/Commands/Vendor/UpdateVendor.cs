using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Data.Entities.Vendor;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Vendor
{
    public static class UpdateVendor
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

            [Required]
            public string Name { get; set; }
            [Required] public string Email { get; set; }
            
            public string PhoneNumber { get; set; }
            public string BusinessName { get; set; }

            public string DisplayName { get; set; }
            public string OperatingSector { get; set; }
            public string RcNumber { get; set; }
            public string Website { get; set; }
            public string TIN { get; set; }
            
            [RequiredNonDefault] public BankAccountModel BankAccount { get; set; }
            [RequiredNonDefault] public ContactPersonModel ContactPerson { get; set; }
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
        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.NoContent;
            }

            public Response(HttpStatusCode statusCode)
            {
                Status = statusCode;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }

        }


        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IAuditLogHelper _auditHelper;
            private readonly IMapper _mapper;

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {

                var vendors = await _dbContext.Vendors.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted &&
                                                                  (x.Id == request.Id || x.Email.ToLower() ==
                                                                      request.Email.ToLower())).ToListAsync();

                if (vendors.Count == 0) return new Response("Vendor not found");
                if (vendors.Any(x => x.Id != request.Id))
                {
                    return new Response("Email is already in use by another vendor");
                }

                var vendor = vendors.First();
                _mapper.Map(request, vendor);

                var contactPerson = await _dbContext.VendorContactPersons.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && x.VendorId == vendor.Id && x.Id == request.ContactPerson.Id &&
                    !x.IsDeleted);
                if (contactPerson != null)
                {
                    contactPerson.Role = request.ContactPerson.Role;
                    contactPerson.FullName = request.ContactPerson.FullName;
                    contactPerson.EmailAddress = request.ContactPerson.EmailAddress;
                    contactPerson.PhoneNumber = request.ContactPerson.PhoneNumber;
                    contactPerson.LastModifiedBy = request.UserId;
                }

                var bankAccount = await _dbContext.VendorBankAccounts.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && x.VendorId == vendor.Id && x.Id == request.BankAccount.Id &&
                    !x.IsDeleted);
                if (bankAccount != null)
                {
                    bankAccount.AccountName = request.BankAccount.AccountName;
                    bankAccount.AccountNumber = request.BankAccount.AccountNumber;
                    bankAccount.BankCode = request.BankAccount.BankCode;
                    bankAccount.BankName = request.BankAccount.BankName;
                    bankAccount.LastModifiedBy = request.UserId;
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int) AuditLogEntityType.Vendor,
                        Action = (int) AuditLogVendorAction.Update,
                        Description = $"Updated information of vendor with id {request.Id}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
