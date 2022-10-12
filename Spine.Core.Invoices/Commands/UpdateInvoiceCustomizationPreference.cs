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
using Spine.Common.Helper;
using Spine.Data;
using Spine.Data.Entities.Invoices;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class UpdateInvoiceCustomizationPreference
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            public bool LogoEnabled { get; set; }
            public bool SignatureEnabled { get; set; }
            public string SignatureName { get; set; }

            public Guid? BannerImageId { get; set; }
            [RequiredIf(nameof(LogoEnabled), true, ErrorMessage = "Company logo is required")]
            public Guid? LogoImageId { get; set; }
            [RequiredIf(nameof(SignatureEnabled), true, ErrorMessage = "Signature is required")]
            public Guid? SignatureImageId { get; set; }

            [Required]
            public Guid? ColorThemeId { get; set; }
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

                var cust = new InvoiceCustomization
                {
                    Id = SequentialGuid.Create(),
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    LogoEnabled = request.LogoEnabled,
                    SignatureEnabled = request.SignatureEnabled,
                    SignatureName = request.SignatureName,
                    BannerImageId = request.BannerImageId,
                    ColorThemeId = request.ColorThemeId,
                    LogoImageId = request.LogoImageId,
                    SignatureImageId = request.SignatureImageId
                };
                _dbContext.InvoiceCustomizations.Add(cust);

                pref.CustomizationId = cust.Id;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.UpdateInvoiceSettings,
                    UserId = request.UserId,
                    Description = $"Updated invoice customization preferences"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Customization could not be updated");
            }
        }

    }
}
