using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class UpdateInvoicePreference
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public DiscountSettings? Discount { get; set; }
            [Required]
            public TaxSettings? Tax { get; set; }

            // this is normally part of payment preference, but added here because of mobile
            public ApplyTaxSettings ApplyTax { get; set; }
            
            public bool RoundAmountToNearestWhole { get; set; }

            [Required]
            public string InvoiceNoPrefix { get; set; }

            [Required]
            public string InvoiceNoSeparator { get; set; }

            public bool EnableDueDate { get; set; }
            [RequiredIf(nameof(EnableDueDate), true, ErrorMessage = "Due date is required")]
            public int DueDate { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.NoContent;
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var pref = await _dbContext.InvoicePreferences.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId);
                if (pref == null) return new Response("Settings not found");

                pref.LastModifiedBy = request.UserId;
                pref.Discount = request.Discount.Value;
                pref.Tax = request.Tax.Value;
                pref.RoundAmountToNearestWhole = request.RoundAmountToNearestWhole;
                pref.EnableDueDate = request.EnableDueDate;
                pref.DueDate = request.DueDate;
                
                if (request.ApplyTax != ApplyTaxSettings.None)
                { 
                    pref.ApplyTax = request.ApplyTax;
                }
                
                var invoiceNoSetting = await _dbContext.InvoiceNoSettings.SingleAsync(x => x.CompanyId == request.CompanyId);

                invoiceNoSetting.Prefix = request.InvoiceNoPrefix;
                invoiceNoSetting.Separator = request.InvoiceNoSeparator;
                invoiceNoSetting.LastModifiedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.UpdateInvoiceSettings,
                    UserId = request.UserId,
                    Description = $"Updated invoice preferences"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Settings could not be updated");
            }
        }

    }
}
