using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spine.Services.HttpClients;
using Spine.Services.Paystack.Common;

namespace Spine.Services.Paystack.SubAccounts
{
    public static class CreatePaystackSubAccount
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("data")]
            public Model Data { get; set; }
        }

        public class Model
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("integration")]
            public int Integration { get; set; }
            [JsonPropertyName("domain")]
            public string Domain { get; set; }
            [JsonPropertyName("subaccount_code")]
            public string SubaccountCode { get; set; }
            [JsonPropertyName("business_name")]
            public string BusinessName { get; set; }
            [JsonPropertyName("description")]
            public string Description { get; set; }
            [JsonPropertyName("primary_contact_name")]
            public string PrimaryContactName { get; set; }
            [JsonPropertyName("primary_contact_email")]
            public string PrimaryContactEmail { get; set; }
            [JsonPropertyName("primary_contact_phone")]
            public object PrimaryContactPhone { get; set; }
            [JsonPropertyName("metadata")]
            public object Metadata { get; set; }
            [JsonPropertyName("percentage_charge")]
            public float PercentageCharge { get; set; }
            [JsonPropertyName("is_verified")]
            public bool Isverified { get; set; }
            [JsonPropertyName("settlement_bank")]
            public string SettlementBank { get; set; }
            [JsonPropertyName("account_number")]
            public string AccountNumber { get; set; }
            [JsonPropertyName("settlement_schedule")]
            public string SettlementSchedule { get; set; }
            [JsonPropertyName("active")]
            public bool Active { get; set; }
            [JsonPropertyName("migrate")]
            public bool Migrate { get; set; }
            [JsonPropertyName("createdAt")]
            public DateTime CreatedAt { get; set; }
            [JsonPropertyName("updatedAt")]
            public DateTime UpdatedAt { get; set; }
        }

        public class Request
        {
            /// <summary>
            /// Note: REQUIRED Name of business for subaccount
            /// </summary>  
            /// 
            public string BusinessName { get; set; }
            /// <summary>
            /// Note: REQUIRED  Name of Bank (see list of accepted names by calling https://developers.paystack.co/docs/list-banks)
            /// </summary>  
            public string SettlementBank { get; set; }
            /// <summary>
            /// Note: REQUIRED NUBAN Bank Account Number
            /// </summary>  
            public string AccountNumber { get; set; }
            /// <summary>
            /// Note: REQUIRED What is the default percentage charged when receiving on behalf of this subaccount?
            /// </summary>  
            public float PercentageCharge { get; set; }
            /// <summary>
            /// Note: A description for this subaccount
            /// </summary>  
            public string Description { get; set; }

            /// <summary>
            /// Note: A contact email for the subaccount
            /// </summary>  
            /// 
            public string PrimaryContactEmail { get; set; }
            /// <summary>
            /// Note: A name for the contact person for this subaccount
            /// </summary>  
            /// 
            public string PrimaryContactName { get; set; }
            /// <summary>
            /// Note: A phone number to call for this subaccount
            /// </summary>  
            /// 
            public string PrimaryContactPhone { get; set; }
            /// <summary>
            /// Note: Stringified JSON object. Add a custom_fields attribute which has an array of objects 
            /// if you would like the fields to be added to your transaction when displayed on the dashboard.
            /// Sample: { "custom_fields":[{"display_name":"Cart ID","variable_name":"cart_id","value":"8393"}]}
            /// </summary>  
            /// 
            public MetaDataObject[] Metadata { get; set; }
            /// <summary>
            /// Note: Any of auto, weekly, monthly, manual. 
            /// Auto means payout is T+1 and manual means payout to the subaccount should only be made when requested.
            /// </summary>  
            /// 
            public string SettlementSchedule { get; set; }


            public string GetResourceUrl()
            {
                return _resourceUrl;
            }

            private readonly string _resourceUrl;

            public Request()
            {
                _resourceUrl = "subaccount";
                SettlementSchedule = "auto";
                Metadata = new MetaDataObject[] { };

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
                    new KeyValuePair<string, string>("business_name", request.BusinessName),
                    new KeyValuePair<string, string>("settlement_bank", request.SettlementBank),
                    new KeyValuePair<string, string>("account_number", request.AccountNumber),
                    new KeyValuePair<string, string>("percentage_charge", request.PercentageCharge.ToString()),
                    new KeyValuePair<string, string>("description", request.Description),
                    new KeyValuePair<string, string>("primary_contact_email", request.PrimaryContactEmail),
                    new KeyValuePair<string, string>("primary_contact_name", request.PrimaryContactName),
                    new KeyValuePair<string, string>("primary_contact_phone", request.PrimaryContactPhone),
                    new KeyValuePair<string, string>("settlement_schedule", request.SettlementSchedule)
                };
                    foreach (var meta in request.Metadata)
                    {
                        body.Add(new KeyValuePair<string, string>("metadata[]", JsonSerializer.Serialize(meta)));
                    }
                    var content = new FormUrlEncodedContent(body.ToArray());
                    var response = await paystackClient.Post<Response>(content, request.GetResourceUrl());  //.ConfigureAwait(false);
                    if (response.StatusCode == HttpStatusCode.Created)
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
