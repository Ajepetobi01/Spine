using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using System.ComponentModel.DataAnnotations;
using Spine.Services.Mono.Common;
using Spine.Common.Attributes;

namespace Spine.Services.Mono
{
    public static class GetAccountStatement
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("data")]
            public List<Model> Data { get; set; }

            [JsonPropertyName("meta")]
            public Meta Meta { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("_id")]
            public string Id { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("category")]
            public string Category { get; set; }
            [JsonPropertyName("date")]
            public DateTime Date { get; set; }
            [JsonPropertyName("narration")]
            public string Narration { get; set; }

            [JsonPropertyName("amount")]
            public decimal Amount { get; set; }
        }

        public class Meta
        {
            [JsonPropertyName("count")]
            public int Count { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Account ID returned from token exchange
            /// </summary>  
            /// 
            [Required]
            public string AccountId { get; set; }

            /// <summary>
            /// Note: Number of months from current date (1-12)
            /// </summary>  
            ///
            [Range(1, 12)]
            public int Period { get; set; } = 6;

            /// <summary>
            /// You can set the output as pdf if you want to receive pdf instead of Json
            /// </summary>  
            ///
            [StringRange(new string[] { "pdf" })]
            public string Output { get; set; }

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
                    var outputString = "";
                    if (request.Output == "pdf")
                    {
                        outputString = "&output=pdf";
                    }

                    var response = await monoClient.Get<Response>($"{request.GetResourceUrl()}{request.AccountId}/statement?period=last{request.Period}months{outputString}");

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
