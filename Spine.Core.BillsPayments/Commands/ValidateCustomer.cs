using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Spine.Common.ActionResults;
using Spine.Common.Extensions;
using Spine.Services.HttpClients;
using Spine.Services.Interswitch;

namespace Spine.Core.BillsPayments.Commands
{
    public static class ValidateCustomer
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required]
            public string PaymentCode { get; set; }

            [Required]
            public string CustomerId { get; set; }

        }

        public class Response : BasicActionResult
        {
            public List<CustomerValidation.Model> Data { get; set; }

            public Response(string message) : base(message)
            {
                ErrorMessage = message;
            }

            public Response(List<CustomerValidation.Model> data)
            {
                Status = HttpStatusCode.OK;
                Data = data;
            }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly InterswitchClient _client;

            public Handler(InterswitchClient client)
            {
                _client = client;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {

                var handler = new CustomerValidation.Handler();
                var response = await handler.Handle(new CustomerValidation.Request
                {
                    Customers = new List<CustomerValidation.RequestModel> {
                    new CustomerValidation.RequestModel { CustomerId = request.CustomerId, PaymentCode = request.PaymentCode}
                }
                }, _client);
                if (response.Message.IsNullOrEmpty())
                {
                    return new Response(response.Data);
                }

                else
                    return new Response(response.Message);
            }
        }

    }
}
