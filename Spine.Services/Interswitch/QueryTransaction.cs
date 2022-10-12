using System;
using System.Text.Json.Serialization;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using Spine.Services.Interswitch.Common;

namespace Spine.Services.Interswitch
{
    public static class QueryTransaction
    {
        public class Request : BaseRequest
        {
            /// <summary>
            /// The request reference passed in the ‘SendBillPaymentAdvice’ method or the transfer code passed in the ‘DoTransfer’ method
            /// </summary>
            public string RequestReference { get; set; }

            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "transactions?requestreference=";
            }
        }

        public class Response : BaseResult
        {
            public Model Data { get; set; }
        }

        public class Model
        {
            /*
             *   “recharge”: {
    “biller”: “ZainMTU”,
    “customerId1”: “08090673520”,
    “customerId2”: null,
    “paymentTypeName”: “Zain MTU 50”,
    “paymentTypeCode”: “01”,
    “billerId”: “901”},
             * */
            [JsonPropertyName("responseCode")]
            public string ResponseCode { get; set; }
            [JsonPropertyName("transactionSet")]
            public string TransactionSet { get; set; }
            [JsonPropertyName("transactionResponseCode")]
            public string TransactionResponseCode { get; set; }
            [JsonPropertyName("transactionRef")]
            public string TransactionRef { get; set; }
            [JsonPropertyName("surcharge")]
            public string Surcharge { get; set; }
            [JsonPropertyName("status")]
            public string Status { get; set; }
            [JsonPropertyName("serviceProviderId")]
            public string ServiceProviderId { get; set; }

            [JsonPropertyName("amount")]
            public decimal Amount { get; set; }
            [JsonPropertyName("currencyCode")]
            public string CurrencyCode { get; set; }
            [JsonPropertyName("customer")]
            public string Customer { get; set; }
            [JsonPropertyName("customerEmail")]
            public string CustomerEmail { get; set; }
            [JsonPropertyName("customerMobile")]
            public string CustomerMobile { get; set; }
            [JsonPropertyName("serviceCode")]
            public string ServiceCode { get; set; }
            [JsonPropertyName("serviceName")]
            public string ServiceName { get; set; }
            [JsonPropertyName("requestReference")]
            public string RequestReference { get; set; }
            [JsonPropertyName("paymentDate")]
            public DateTime PaymentDate { get; set; }

        }


        public class Handler
        {
            public async Task<Response> Handle(Request request, InterswitchClient interswitchClient)
            {
                try
                {
                    var response = await interswitchClient.Get<Response>($"{request.GetResourceUrl()}{request.RequestReference}&terminalId={request.TerminalId}");

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
