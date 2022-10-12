using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using Spine.Services.Interswitch.Common;
using Spine.Common.Attributes;
using Spine.Common.Enums;

namespace Spine.Services.Interswitch
{
    public static class CustomerValidation
    {
        public class RequestModel
        {
            /// <summary>
            /// REQUIRED
            /// Length <= 20
            /// Unique payment code retrieved from GetBillerPaymentItems call
            /// </summary>
            public string PaymentCode { get; set; }

            /// <summary>
            /// REQUIRED
            /// Length <= 50
            /// Customer’s Unique Identifier
            /// </summary>
            public string CustomerId { get; set; }

        }

        public class Request : IRequestModel
        {
            [RequiredNotEmpty]
            public List<RequestModel> Customers { get; set; }

            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "customers/validations";
            }
        }

        public class Response : BaseResult
        {
            [JsonPropertyName("customers")]
            public List<Model> Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("paymentCode")]
            public string PaymentCode { get; set; }

            [JsonPropertyName("customerId")]
            public string CustomerId { get; set; }

            [JsonPropertyName("responseCode")]
            public string ResponseCode { get; set; }

            [JsonPropertyName("fullName")]
            public string FullName { get; set; }

            [JsonPropertyName("amount")]
            public string Amount { get; set; }

            [JsonPropertyName("amountType")]
            public InterswitchAmountType AmountType { get; set; }

            [JsonPropertyName("amountTypeDescription")]
            public string AmountTypeDescription { get; set; }

        }


        public class Handler
        {
            public async Task<Response> Handle(Request request, InterswitchClient interswitchClient)
            {
                try
                {
                    var response = await interswitchClient.Post<Response>($"{request.GetResourceUrl()}", request);

                    if (response?.StatusCode == HttpStatusCode.OK)
                    {
                        var successModel = ((ApiSuccessModel<Response>)response);
                        return successModel.Model;
                    }
                    else
                    {
                        var errorModel = (ApiErrorModel)response;
                        return new Response
                        {
                            Message = errorModel?.ErrorMessage
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new Response
                    {
                        Message = ex.Message
                    };
                }
            }
        }
    }
}
