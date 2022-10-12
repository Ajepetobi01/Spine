using System;
using System.Text.Json.Serialization;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using Spine.Services.Mono.Common;
using System.ComponentModel.DataAnnotations;

namespace Spine.Services.Mono
{
    public static class GetAccount
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("account")]
            public Account Account { get; set; }

            [JsonPropertyName("meta")]
            public Meta Meta { get; set; }
        }

        public class Account
        {
            [JsonPropertyName("_id")]
            public string Id { get; set; }
            [JsonPropertyName("bvn")]
            public string Bvn { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("balance")]
            public decimal Balance { get; set; }
            [JsonPropertyName("currency")]
            public string Currency { get; set; }
            [JsonPropertyName("accountNumber")]
            public string AccountNumber { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("institution")]
            public Institution Institution { get; set; }
        }

        public class Institution
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("bankCode")]
            public string BankCode { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        public class Meta
        {
            [JsonPropertyName("data_status")]
            public string DataStatus { get; set; }
            [JsonPropertyName("auth_method")]
            public string AuthMethod { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Account ID returned from token exchange
            /// </summary>  
            /// 
            [Required]
            public string AccountId { get; set; }


            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "accounts/";
            }
        }

        public class Handler
        {
            public async Task<Response> Handle(Request request, MonoClient monoClient)
            {
                try
                {
                    var response = await monoClient.Get<Response>($"{request.GetResourceUrl()}{request.AccountId}");

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var successModel = ((ApiSuccessModel<Response>)response);
                        return successModel.Model;
                    }
                    else
                    {
                        var errorModel = (ApiErrorModel)response;
                        return new Response
                        {
                            Message = errorModel.ErrorMessage
                        };
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
