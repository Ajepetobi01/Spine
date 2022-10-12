using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Common;

namespace Spine.Services.Paystack.Verification
{
    public static class VerifyAccountNumber
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("data")]
            public Model Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("account_number")]
            public string AccountNumber { get; set; }
            [JsonPropertyName("account_name")]
            public string AccountName { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Note: REQUIRED
            /// </summary>  
            /// 
            public string AccountNo { get; set; }
            /// <summary>
            /// Note: REQUIRED
            /// </summary>  
            /// 
            public string BankCode { get; set; }

            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "bank/resolve";
            }
        }

        public class Handler
        {
            public async Task<Response> Handle(Request request, PaystackClient paystackClient)
            {
                try
                {
                    var response = await paystackClient.Get<Response>($"{request.GetResourceUrl()}?account_number={request.AccountNo}&bank_code={request.BankCode}");

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
