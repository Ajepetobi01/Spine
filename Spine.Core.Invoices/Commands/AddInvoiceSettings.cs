using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Data;
using Spine.Data.Entities.Invoices;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class AddInvoiceSettings
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            // this is normally part of payment preference, but added here because of mobile
            public ApplyTaxSettings ApplyTax { get; set; }

            [Required]
            public DiscountSettings? Discount { get; set; }

            [Required]
            public TaxSettings? Tax { get; set; }
            
            public bool RoundAmountToNearestWhole { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
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
                if (await _dbContext.InvoicePreferences.AnyAsync(x => x.CompanyId == request.CompanyId))
                    return new Response("You have invoice settings already");

                var baseCurencyId = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted).Select(x => x.BaseCurrencyId).SingleAsync();
                var themeId = await _dbContext.InvoiceColorThemes.Select(x => x.Id).FirstOrDefaultAsync();

                var cust = new InvoiceCustomization
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    Id = SequentialGuid.Create(),
                    ColorThemeId = themeId
                };

                _dbContext.InvoiceCustomizations.Add(cust);

                var pref = new InvoicePreference
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    CurrencyId = baseCurencyId,
                    RateToCompanyBaseCurrency = 1,
                    Discount = request.Discount.Value,
                    Tax = request.Tax.Value,
                    ApplyTax = ApplyTaxSettings.OnBoth,
                    RoundAmountToNearestWhole = request.RoundAmountToNearestWhole,
                    CustomizationId = cust.Id,
                };

                if (request.ApplyTax != ApplyTaxSettings.None)
                { 
                    pref.ApplyTax = request.ApplyTax;
                }
                
                var companyName = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted).Select(x => x.Name).SingleAsync();
                var nameSplitted = companyName.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                var code = "";
                foreach (var item in nameSplitted)
                {
                    code += item[0];
                }

                _dbContext.InvoiceNoSettings.Add(new InvoiceNoSetting
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    Separator = "-",
                    Prefix = code
                });

                _dbContext.InvoicePreferences.Add(pref);

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.AddInvoiceSettings,
                    UserId = request.UserId,
                    Description = $"Added invoice settings"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Settings could not be saved");
            }
        }
    }
}
