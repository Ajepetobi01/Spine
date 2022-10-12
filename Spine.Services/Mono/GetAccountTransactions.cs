using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Spine.Services.HttpClients;
using System.Threading.Tasks;
using System.Net;
using System.ComponentModel.DataAnnotations;
using Spine.Services.Mono.Common;
using Spine.Common.Attributes;
using Spine.Common.Extensions;

namespace Spine.Services.Mono
{
    public static class GetAccountTransactions
    {
        public class Response : BaseResult
        {
            [JsonPropertyName("data")]
            public List<Model> Data { get; set; }

            [JsonPropertyName("paging")]
            public Paging Paging { get; set; }
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

        public class Paging
        {
            [JsonPropertyName("total")]
            public int Total { get; set; }
            [JsonPropertyName("page")]
            public int Page { get; set; }
            [JsonPropertyName("previous")]
            public string Previous { get; set; }
            [JsonPropertyName("next")]
            public string Next { get; set; }
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
            ///  start period of the transactions eg. 01-10-2020
            /// </summary>
            public DateTime? StartDate { get; set; }

            /// <summary>
            /// end period of the transactions eg. 01-10-2020
            /// </summary>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// filters all transactions by narration e.g Uber transactions
            /// </summary>
            public string Narration { get; set; }

            /// <summary>
            /// filters transactions by debit or credit
            /// </summary>
            [StringRange(new string[] { "debit", "credit" })]
            public string Type { get; set; }

            /// <summary>
            /// true or false (If you want to receive the data all at once or you want it paginated)
            /// </summary>
            public bool Paginate { get; set; } = false;

            /// <summary>
            /// limit the number of transactions returned per API call
            /// </summary>
            public int Limit { get; set; }

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
                    var filters = "";

                    if (!request.Narration.IsNullOrWhiteSpace()) filters += $"&narration={request.Narration}";
                    if (!request.Type.IsNullOrWhiteSpace()) filters += $"&type={request.Type}";
                    if (request.StartDate.HasValue) filters += $"start{request.StartDate.Value:dd-MM-yyyy}";
                    if (request.EndDate.HasValue) filters += $"end{request.EndDate.Value:dd-MM-yyyy}";

                    filters += $"&paginate={request.Paginate}";
                    if (request.Limit > 0) filters += $"&limit={request.Limit}";

                    var response = await monoClient.Get<Response>($"{request.GetResourceUrl()}{request.AccountId}/transactions?{filters}");

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
