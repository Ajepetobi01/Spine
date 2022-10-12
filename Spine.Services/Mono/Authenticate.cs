using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Spine.Services.Mono.Common;
using System.ComponentModel.DataAnnotations;

namespace Spine.Services.Mono
{
    public static class Authenticate
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Note: Code returned from the Connect widget
            /// </summary>  
            /// 
            [Required]
            public string Code { get; set; }


            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "account/auth";
            }
        }

        public class Handler
        {
            public async Task<Response> Handle(Request request, MonoClient monoClient)
            {
                try
                {
                    var body = new List<KeyValuePair<string, string>> {
                          new KeyValuePair<string, string>("code", request.Code)
                    };

                    var content = new FormUrlEncodedContent(body.ToArray());
                    var response = await monoClient.Post<Response>(content, request.GetResourceUrl());
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
