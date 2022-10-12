using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class UpdateInvoicePaymentPreference
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

           // [Required]
            public string PaymentTerms { get; set; }
            //[Required]
            public string ShareMessage { get; set; }

            public bool EnablePaymentLink { get; set; }

            public ApplyTaxSettings ApplyTax { get; set; }

            [Required]
            public int CurrencyId { get; set; }

            [Required]
            public decimal? NewCurrencyRate { get; set; }

        }

        public class PaymentIntegrationModel
        {
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

                pref.ApplyTax = request.ApplyTax;
                pref.LastModifiedBy = request.UserId;
                pref.PaymentTerms = request.PaymentTerms;
                pref.ShareMessage = request.ShareMessage;
                pref.PaymentLinkEnabled = request.EnablePaymentLink;
                pref.CurrencyId = request.CurrencyId;
                pref.RateToCompanyBaseCurrency = request.NewCurrencyRate.Value;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.UpdateInvoiceSettings,
                    UserId = request.UserId,
                    Description = $"Updated invoice payment preferences"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Settings could not be updated");
            }
        }

    }
}
