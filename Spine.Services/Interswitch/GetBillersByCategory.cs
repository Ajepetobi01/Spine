using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using Spine.Services.Interswitch.Common;

namespace Spine.Services.Interswitch
{
    public static class GetBillersByCategory
    {
        public class Request : BaseRequest
        {
            /// <summary>
            /// REQUIRED. Category Id
            /// </summary>
            public string Id { get; set; }

            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "categorys";
            }
        }

        public class Response : BaseResult
        {
            [JsonPropertyName("billers")]
            public List<Model> Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("categoryid")]
            public string CategoryId { get; set; }
            [JsonPropertyName("categoryname")]
            public string CategoryName { get; set; }
            [JsonPropertyName("categorydescription")]
            public string CategoryDescription { get; set; }
            [JsonPropertyName("billerid")]
            public string BillerId { get; set; }
            [JsonPropertyName("billername")]
            public string BillerName { get; set; }

            [JsonPropertyName("customerfield1")]
            public string CustomerField1 { get; set; }
            [JsonPropertyName("customerfield2")]
            public string CustomerField2 { get; set; }
            [JsonPropertyName("currencySymbol")]
            public string CurrencySymbol { get; set; }
            [JsonPropertyName("currencyCode")]
            public string CurrencyCode { get; set; }
            [JsonPropertyName("logoUrl")]
            public string LogoUrl { get; set; }

            public string FullLogoUrl => "https://quickteller.sandbox.interswitchng.com/Content/Images/Downloaded/" + LogoUrl;

        }


        public class Handler
        {
            public async Task<Response> Handle(Request request, InterswitchClient interswitchClient)
            {
                try
                {
                    var response = await interswitchClient.Get<Response>($"{request.GetResourceUrl()}/{request.Id}/billers?terminalId={request.TerminalId}");

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
