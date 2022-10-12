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
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Common;
using Spine.Services.Paystack.SubAccounts;

namespace Spine.Core.Invoices.Commands
{
    public static class AddPaymentIntegration
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public PaymentIntegrationProvider? IntegrationProvider { get; set; }

            //  [Required]
            //public PaymentIntegrationType? IntegrationType { get; set; }

            [Required]
            public string SettlementBankCode { get; set; }
            [Required]
            public string SettlementBankName { get; set; }
            [Required]
            public string SettlementAccountNumber { get; set; }
            [Required]
            public string SettlementBankCurrency { get; set; }

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
            private readonly PaystackClient _paystackClient;
            public Handler(SpineContext context, IAuditLogHelper auditHelper, PaystackClient paystackClient)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _paystackClient = paystackClient;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var details = await (from comp in _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                                     join user in _dbContext.Users on comp.Email equals user.Email
                                     join pref in _dbContext.InvoicePreferences on comp.Id equals pref.CompanyId
                                     select new { comp.Email, comp.PhoneNumber, BusinessName = comp.Name, user.FullName, pref }).SingleOrDefaultAsync();

                var existingCheck = await _dbContext.PaymentIntegrations.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId
                                                            && x.SettlementAccountNumber == request.SettlementAccountNumber && x.IntegrationProvider == request.IntegrationProvider);
                string action = "";
                if (existingCheck == null)
                {
                    var integration = new PaymentIntegration
                    {
                        CompanyId = request.CompanyId,
                        CreatedBy = request.UserId,
                        Id = SequentialGuid.Create(),
                        IntegrationProvider = request.IntegrationProvider.Value,
                        IntegrationType = PaymentIntegrationType.Customer, // request.IntegrationType.Value,
                        SettlementAccountNumber = request.SettlementAccountNumber,
                        SettlementBankCode = request.SettlementBankCode,
                        SettlementBankName = request.SettlementBankName,
                        SettlementBankCurrency = request.SettlementBankCurrency,
                        SubaccountCode = "",
                        RecipientCode = "",
                        BusinessName = details.BusinessName,
                        PrimaryContactEmail = details.Email,
                        PrimaryContactName = details.FullName,
                        PrimaryContactPhone = details.PhoneNumber,
                        Description = "",
                    };

                    if (integration.IntegrationProvider == PaymentIntegrationProvider.Paystack)
                    {
                        if (integration.IntegrationType == PaymentIntegrationType.Customer)
                        {
                            var subAccountRequest = new CreatePaystackSubAccount.Request
                            {
                                BusinessName = integration.BusinessName,
                                SettlementBank = integration.SettlementBankCode,
                                AccountNumber = integration.SettlementAccountNumber,
                                PrimaryContactName = integration.PrimaryContactName,
                                PrimaryContactEmail = integration.PrimaryContactEmail,
                                PrimaryContactPhone = integration.PrimaryContactPhone,
                                PercentageCharge = 1, //this will be overriden by the percentage charge in the initialize trans
                                Description = "Set up payment integration for invoice payments",
                                Metadata = new MetaDataObject[]
                               {
                                new MetaDataObject  { custom_fields = new []
                                    {
                                        new CustomFields { display_name  = "Recipient Type", variable_name = "recipient_type", value="Spine Customer"}
                                    }
                                }
                                }
                            };

                            var handler = new CreatePaystackSubAccount.Handler();
                            var response = await handler.Handle(subAccountRequest, _paystackClient);
                            if (response != null && response.Status)
                            {
                                integration.SubaccountCode = response.Data.SubaccountCode;
                            }
                            else
                                return new Response(response?.Message ?? "An error occured. Please try again");
                        }

                        if (integration.IntegrationType == PaymentIntegrationType.Spine)
                        {
                            integration.RecipientCode = "";
                        }
                    }

                    if (integration.IntegrationProvider == PaymentIntegrationProvider.Flutterwave)
                    {
                        if (integration.IntegrationType == PaymentIntegrationType.Customer)
                        {
                            integration.SubaccountCode = "";
                        }
                        if (integration.IntegrationType == PaymentIntegrationType.Spine)
                        {
                            integration.RecipientCode = "";
                        }
                    }

                    _dbContext.PaymentIntegrations.Add(integration);
                    details.pref.PaymentLinkEnabled = true;
                    details.pref.PaymentIntegrationId = integration.Id;
                    action = $"Added payment integration details for invoice. Account Number - {request.SettlementAccountNumber}";
                }

                else
                {
                    details.pref.PaymentLinkEnabled = true;
                    details.pref.PaymentIntegrationId = existingCheck.Id;
                    action = $"Updated payment integration details for invoice to Account Number - {existingCheck.SettlementAccountNumber}";
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.UpdateInvoiceSettings,
                    UserId = request.UserId,
                    Description = action
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Settings could not be saved");
            }
        }

    }
}
