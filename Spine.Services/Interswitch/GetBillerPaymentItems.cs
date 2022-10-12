using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using Spine.Services.Interswitch.Common;

namespace Spine.Services.Interswitch
{
    public static class GetBillerPaymentItems
    {
        public class Request : BaseRequest
        {
            public int BillerId { get; set; }

            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "billers";
            }
        }

        public class Response : BaseResult
        {
            [JsonPropertyName("paymentitems")]
            public List<Model> Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("categoryid")]
            public string CategoryId { get; set; }
            [JsonPropertyName("billerid")]
            public string BillerId { get; set; }
            [JsonPropertyName("isAmountFixed")]
            public bool IsAmountFixed { get; set; }
            [JsonPropertyName("paymentitemid")]
            public string PaymentItemId { get; set; }
            [JsonPropertyName("paymentitemname")]
            public string PaymentItemName { get; set; }

            [JsonPropertyName("amount")]
            public string Amount { get; set; }
            [JsonPropertyName("code")]
            public string Code { get; set; }
            [JsonPropertyName("currencySymbol")]
            public string CurrencySymbol { get; set; }
            [JsonPropertyName("currencyCode")]
            public string CurrencyCode { get; set; }
            [JsonPropertyName("itemCurrencySymbol")]
            public string ItemCurrencySymbol { get; set; }
            [JsonPropertyName("paymentcode")]
            public string PaymentCode { get; set; }

        }


        public class Handler
        {
            public async Task<Response> Handle(Request request, InterswitchClient interswitchClient)
            {
                try
                {
                    var response = await interswitchClient.Get<Response>($"{request.GetResourceUrl()}/{request.BillerId}/paymentitems?terminalId={request.TerminalId}");

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
