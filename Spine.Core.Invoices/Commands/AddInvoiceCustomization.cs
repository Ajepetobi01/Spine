using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Data.Entities.Invoices;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class AddInvoiceCustomization
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

            //[Required]
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
            public Guid Id { get; set; }
            public Response(Guid id)
            {
                Id = id;
                Status = HttpStatusCode.Created;
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
                var cust = new InvoiceCustomization
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    LogoEnabled = request.LogoEnabled,
                    SignatureEnabled = request.SignatureEnabled,
                    SignatureName = request.SignatureName,
                    BannerImageId = request.BannerImageId.Value,
                    ColorThemeId = request.ColorThemeId.Value,
                    LogoImageId = request.LogoImageId.Value,
                    SignatureImageId = request.SignatureImageId.Value
                };

                _dbContext.InvoiceCustomizations.Add(cust);
                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.UpdateInvoiceSettings,
                    UserId = request.UserId,
                    Description = $"Add invoice customization"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response(cust.Id) : new Response("Customization could not be updated");
            }
        }

    }
}
