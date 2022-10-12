using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using Spine.Services.Interswitch.Common;

namespace Spine.Services.Interswitch
{
    public static class GetBillerCategories
    {
        public class Request : BaseRequest
        {

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
            [JsonPropertyName("categorys")]
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

        }

        public class Handler
        {
            public async Task<Response> Handle(Request request, InterswitchClient interswitchClient)
            {
                try
                {
                    var response = await interswitchClient.Get<Response>($"{request.GetResourceUrl()}?terminalId={request.TerminalId}");
                    
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
