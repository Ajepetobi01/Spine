using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Common;

namespace Spine.Services.Paystack.Misc
{
    public static class ListBanks
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("data")]
            public List<Model> Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("slug")]
            public string Slug { get; set; }
            [JsonPropertyName("code")]
            public string Code { get; set; }
            [JsonPropertyName("pay_with_bank")]
            public bool PayWithBank { get; set; }
            [JsonPropertyName("country")]
            public string Country { get; set; }
            [JsonPropertyName("currency")]
            public string Currency { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Note: REQUIRED
            /// </summary>  
            /// 
            public string Country { get; set; }


            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "bank";
                Country = "nigeria";
            }
        }

        public class Handler
        {
            public async Task<Response> Handle(Request request, PaystackClient paystackClient)
            {
                try
                {
                    var response = await paystackClient.Get<Response>($"{request.GetResourceUrl()}?country={request.Country}");

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var successModel = ((ApiSuccessModel<Response>)response);
                        return successModel.Model;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

    }
}
