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
    public static class UpdateInvoiceCustomization
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

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
                //  if (request.Theme.Split(',').Length != 3) return new Response("Theme must contain 3 comma separated hex value of colors");

                var cust = await _dbContext.InvoiceCustomizations.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id);
                if (cust == null) return new Response("Settings not found");

                cust.LastModifiedBy = request.UserId;
                cust.LogoEnabled = request.LogoEnabled;
                cust.SignatureEnabled = request.SignatureEnabled;
                cust.SignatureName = request.SignatureName;
                cust.ColorThemeId = request.ColorThemeId;
                cust.BannerImageId = request.BannerImageId;
                cust.SignatureImageId = request.SignatureImageId;
                cust.LogoImageId = request.LogoImageId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.UpdateInvoiceSettings,
                    UserId = request.UserId,
                    Description = $"Updated invoice customization"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Customization could not be updated");
            }
        }

    }
}
