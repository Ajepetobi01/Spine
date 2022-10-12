using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Common;

namespace Spine.Services.Paystack.Verification
{
    public static class VerifyBVN
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("data")]
            public Model Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("bvn")]
            public string BVN { get; set; }
            [JsonPropertyName("account_number")]
            public bool AccountNumber { get; set; }
            [JsonPropertyName("is_blacklisted")]
            public bool IsBlacklisted { get; set; }
            [JsonPropertyName("first_name")]
            public bool FirstName { get; set; }
            [JsonPropertyName("last_name")]
            public bool LastName { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Note: REQUIRED
            /// </summary>  
            /// 
            public string AccountNumber { get; set; }
            /// <summary>
            /// Note: REQUIRED
            /// </summary>  
            /// 
            public string BVN { get; set; }
            /// <summary>
            /// Note: REQUIRED
            /// </summary>  
            /// 
            public string BankCode { get; set; }

            /// <summary>
            /// Note: OPTIONAL
            /// </summary>  
            /// 
            public string FirstName { get; set; }
            /// <summary>
            /// Note: OPTIONAL
            /// </summary>  
            /// 
            public string MiddleName { get; set; }
            /// <summary>
            /// Note: OPTIONAL
            /// </summary>  
            /// 
            public string LastName { get; set; }

            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "bvn/match";
            }
        }

        public class Handler
        {
            public async Task<Response> Handle(Request request, PaystackClient paystackClient)
            {
                try
                {
                    var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("bvn", request.BVN),
                    new KeyValuePair<string, string>("account_number", request.AccountNumber),
                    new KeyValuePair<string, string>("bank_code", request.BankCode),
                    new KeyValuePair<string, string>("first_name", request.FirstName),
                    new KeyValuePair<string, string>("middle_name", request.MiddleName),
                    new KeyValuePair<string, string>("last_name", request.LastName)
                };
                    var content = new FormUrlEncodedContent(body.ToArray());
                    var response = await paystackClient.Post<Response>(content, request.GetResourceUrl());
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
