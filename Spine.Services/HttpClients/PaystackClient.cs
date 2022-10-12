using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace Spine.Services.HttpClients
{

    public class PaystackClient : HttpClientFactoryService
    {
        private readonly HttpClient _httpClient;

        public PaystackClient(HttpClient client, IConfiguration configuration) //: base(client)
        {
            var secretKey = configuration["Paystack:SecretKey"];

            _httpClient = client;
            _httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretKey);

            Client = _httpClient;

        }

    }
}
