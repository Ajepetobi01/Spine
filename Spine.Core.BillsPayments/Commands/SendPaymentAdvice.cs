using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities.BillsPayments;
using Spine.Services;
using Spine.Services.HttpClients;
using Spine.Services.Interswitch;

namespace Spine.Core.BillsPayments.Commands
{
    public static class SendPaymentAdvice
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public GetBillerPaymentItems.Model PaymentItem { get; set; }

            [RequiredNonDefault]
            public int? AmountToPay { get; set; }

            public string FullName { get; set; }
            public string CustomerId { get; set; }
            public string CustomerMobile { get; set; }
            public string CustomerEmail { get; set; }
            public InterswitchAmountType AmountType { get; set; }
            public string AmountTypeDescription { get; set; }

        }

        public class Response : BasicActionResult
        {
            public SendBillsPaymentAdvice.Model Data { get; set; }

            public Response(string message) : base(message)
            {
                ErrorMessage = message;
            }

            public Response(SendBillsPaymentAdvice.Model data)
            {
                Status = HttpStatusCode.OK;
                Data = data;
            }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly IAuditLogHelper _auditHelper;
            private readonly IMapper _mapper;
            private readonly InterswitchClient _client;
            private readonly string RequestReferencePrefix;

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper, InterswitchClient client, IConfiguration configuration)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _client = client;

                RequestReferencePrefix = configuration["Interswitch:TransferCodePrefix"];
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {

                if (!int.TryParse(request.PaymentItem.Amount, out var amount))
                    return new Response("Invalid amount");
                
                //validate amount to pay with payment amount
                switch (request.AmountType)
                {
                    case InterswitchAmountType.None:
                        break;
                    case InterswitchAmountType.Minimum:
                        if (request.AmountToPay.Value < amount)
                            return new Response($"Amount to pay must be greater than or equal to {request.PaymentItem.Amount}");
                        break;
                    case InterswitchAmountType.Maximum:
                        if (request.AmountToPay.Value > amount)
                            return new Response($"Amount to pay must be less than or equal to {request.PaymentItem.Amount}");
                        break;
                    case InterswitchAmountType.GreaterThanMinimum:
                        if (request.AmountToPay.Value <= amount)
                            return new Response($"Amount to pay must be greater than {request.PaymentItem.Amount}");
                        break;
                    case InterswitchAmountType.LessThanMaximum:
                        if (request.AmountToPay.Value >= amount)
                            return new Response($"Amount to pay must be less than  {request.PaymentItem.Amount}");
                        break;
                    case InterswitchAmountType.Exact:
                        if (request.AmountToPay.Value != amount)
                            return new Response($"Amount to pay must be equal to {request.PaymentItem.Amount}");
                        break;
                    default:
                        break;
                }

                var newItem = _mapper.Map<BillPayment>(request.PaymentItem);
                newItem.CompanyId = request.CompanyId;
                newItem.CreatedBy = request.UserId;
                newItem.AmountToPay = request.AmountToPay.Value;
                newItem.FullName = request.FullName;
                newItem.CustomerId = request.CustomerId;
                newItem.CustomerMobile = request.CustomerMobile;
                newItem.CustomerEmail = request.CustomerEmail;
                //newItem.AmountType = request.AmountType;
                //newItem.AmountTypeDescription = request.AmountTypeDescription;


                var requestRef = RequestReferencePrefix + Constants.GenerateAlphaNumericId(8);
                var handler = new SendBillsPaymentAdvice.Handler();
                var response = await handler.Handle(new SendBillsPaymentAdvice.Request
                {
                    PaymentCode = request.PaymentItem.PaymentCode,
                    Amount = request.AmountToPay.Value,
                    CustomerId = request.CustomerId,
                    CustomerEmail = request.CustomerEmail,
                    CustomerMobile = request.CustomerMobile,
                    RequestReference = requestRef
                }, _client);

                if (response.Message.IsNullOrEmpty())
                {
                    newItem.TransactionReference = response.Data.TransactionReference;
                    newItem.MiscData = response.Data.MiscData;
                    newItem.PIN = response.Data.PIN;
                    newItem.ResponseCodeGrouping = response.Data.ResponseCodeGrouping;

                    _dbContext.BillPayments.Add(newItem);
                    _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.BillsPayment,
                        Action = (int)AuditLogBillsPaymentAction.PayUtilityService,
                        UserId = request.UserId,
                        Description = $" Submitted payment request - {request.AmountToPay} for {request.PaymentItem.PaymentItemName} with refNo {response.Data.TransactionReference}"
                    });

                    await _dbContext.SaveChangesAsync();

                    response.Data.RequestReference = requestRef;
                    return new Response(response.Data);
                }

                else
                    return new Response(response.Message);

            }
        }

    }
}
