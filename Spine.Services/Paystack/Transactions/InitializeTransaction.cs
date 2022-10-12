using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using Spine.Common.Helper;
using Spine.Services.Paystack.Common;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace Spine.Services.Paystack.Transactions
{
    public static class InitializePaystackTransaction
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("data")]
            public Model Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("authorization_url")]
            public string AuthorizationUrl { get; set; }
            [JsonPropertyName("access_code")]
            public string AccessCode { get; set; }
            [JsonPropertyName("reference")]
            public string Reference { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Note: A Unique reference is required for every transaction.
            /// Only -,., = and alphanumeric characters allowed.
            /// </summary>  
            /// 
            public string Reference { get; set; }
            /// <summary>
            /// Note: Fully qualified url, e.g. https://nut-ng.org/. 
            /// Use this to override the callback url provided on the dashboard for this transaction
            /// </summary>  
            public string CallbackURL { get; set; }
            /// <summary>
            /// Note: All Amounts must be in kobo.
            /// Cannot be null or empty
            /// </summary>  
            public int AmountInKobo { get; set; }
            /// <summary>
            /// Note: Cannot be null or empty
            /// </summary>  
            public string Email { get; set; }
            /// <summary>
            /// Note: If transaction is to create a subscription to a predefined plan, provide plan code here.
            /// </summary>  
            /// 
            public string Plan { get; set; }
            /// <summary>
            /// Note: Used to apply a multiple to the amount returned by the plan code above.
            /// </summary>  
            /// 
            public float Quantity { get; set; }
            /// <summary>
            /// Note: Number of invoices to raise during the subscription. Overrides invoice_limit set on plan
            /// </summary>  
            /// 
            public int InvoiceLimit { get; set; }
            /// <summary>
            /// Note: Stringified JSON object. Add a custom_fields attribute which has an array of objects 
            /// if you would like the fields to be added to your transaction when displayed on the dashboard.
            /// Sample: { "custom_fields":[{"display_name":"Cart ID","variable_name":"cart_id","value":"8393"}]}
            /// </summary>  
            /// 
            public MetaDataObject[] Metadata { get; set; }
            /// <summary>
            /// Note: URL customer is redirected to if the cancel button is clicked. NB: This should be a key in the metadata object.
            /// </summary>  
            /// 
            public string MetadataCancelAction { get; set; }

            /// <summary>
            /// Note: The code for the subaccount that owns the payment. e.g. ACCT_8f4s1eq7ml6rlzj
            /// </summary>  
            /// 
            public string Subaccount { get; set; }
            /// <summary>
            /// Note: A flat fee to charge the subaccount for this transaction, in kobo. 
            /// This overrides the split percentage set when the subaccount was created. 
            /// Ideally, you will need to use this if you are splitting in flat rates (since subaccount creation only allows for percentage split). 
            /// e.g. 7000 for a 70 naira flat fee.
            /// </summary>  
            /// 
            public int TransactionCharge { get; set; }
            /// <summary>
            /// Note: Who bears Paystack charges? account or subaccount(defaults to account).
            /// </summary>  
            /// 
            public string Bearer { get; set; }
            /// <summary>
            /// Note: Option: Send 'card' or 'bank' or 'card','bank' as an array to specify what options to show the user paying (defaults to "card","bank")
            /// </summary>  
            public string[] Channels { get; set; }

            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "transaction/initialize";
                CallbackURL = "";
                Reference = SequentialGuid.Create().ToString().ToUpper();
                Subaccount = "";
                Bearer = "account";
                TransactionCharge = 0;
                Plan = "";
                Metadata = new MetaDataObject[] { };
                MetadataCancelAction = "";
                Channels = new[] { "card", "bank" };
            }
        }

        public class Handler
        {
            public async Task<Response> Handle(Request request, PaystackClient paystackClient)
            {
                try
                {
                    var body = new List<KeyValuePair<string, string>>();
                    if (request.TransactionCharge != 0)
                    {
                        body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("reference", request.Reference),
                        new KeyValuePair<string, string>("callback_url", request.CallbackURL),
                        new KeyValuePair<string, string>("amount", request.AmountInKobo.ToString()),
                        new KeyValuePair<string, string>("email", request.Email),
                        new KeyValuePair<string, string>("plan", request.Plan),
                        new KeyValuePair<string, string>("quantity", request.Quantity.ToString()),
                        new KeyValuePair<string, string>("invoice_limit", request.InvoiceLimit.ToString()),
                        new KeyValuePair<string, string>("metadata.cancel_action", request.MetadataCancelAction),
                        new KeyValuePair<string, string>("subaccount", request.Subaccount),
                        new KeyValuePair<string, string>("transaction_charge", request.TransactionCharge.ToString()),
                        new KeyValuePair<string, string>("bearer", request.Bearer)
                    };
                    }
                    else
                    {
                        body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("reference", request.Reference),
                        new KeyValuePair<string, string>("callback_url", request.CallbackURL),
                        new KeyValuePair<string, string>("amount", request.AmountInKobo.ToString()),
                        new KeyValuePair<string, string>("email", request.Email),
                        new KeyValuePair<string, string>("plan", request.Plan),
                        new KeyValuePair<string, string>("quantity", request.Quantity.ToString()),
                        new KeyValuePair<string, string>("invoice_limit", request.InvoiceLimit.ToString()),
                        new KeyValuePair<string, string>("metadata.cancel_action", request.MetadataCancelAction),
                        new KeyValuePair<string, string>("subaccount", request.Subaccount),
                        new KeyValuePair<string, string>("bearer", request.Bearer)
                    };
                    }

                    foreach (var meta in request.Metadata)
                    {
                        body.Add(new KeyValuePair<string, string>("metadata[]", JsonSerializer.Serialize(meta)));
                    }
                    foreach (var channel in request.Channels)
                    {
                        body.Add(new KeyValuePair<string, string>("channels[]", channel));
                    }
                    var content = new FormUrlEncodedContent(body.ToArray());
                    var response = await paystackClient.Post<Response>(content, request.GetResourceUrl());  //.ConfigureAwait(false);
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
