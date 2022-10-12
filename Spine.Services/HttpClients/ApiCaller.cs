using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Spine.Services.HttpClients
{
    public class ApiCaller : HttpClientFactoryService
    {
        private readonly HttpClient _httpClient;

        public ApiCaller(HttpClient client)
        {
            _httpClient = client;
            //  _httpClient.BaseAddress = new Uri("");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //_client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("text/xml"));

            Client = _httpClient;

        }
    }
}
