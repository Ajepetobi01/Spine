using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Spine.Services.HttpClients
{
    //services.AddHttpClient("ApiCaller", config => //using a named instance
    //{
    //config.Timeout = new TimeSpan(0, 0, 30);
    //config.DefaultRequestHeaders.Clear();
    //      config.DefaultRequestHeaders.Accept.Add(
    //            new MediaTypeWithQualityHeaderValue("application/json"));
    //});

    public interface IHttpClientFactoryService
    {
        Task<IApiResponse> Get<T>(string url);
        Task<IApiResponse> Post<T>(string url, IRequestModel requestModel);
        Task<IApiResponse> Post<T>(FormUrlEncodedContent urlEncodedContent, string url);
        Task<IApiResponse> Put(string url, IRequestModel requestModel);
        Task<IApiResponse> Delete(string url);

    }

    public class HttpClientFactoryService : IHttpClientFactoryService
    {
        protected HttpClient Client;  // will be set from the individual classes inheriting from this class
        private readonly CancellationToken _cancellationToken;
        private readonly JsonSerializerOptions _serializerOptions;

        public HttpClientFactoryService()
        {
            _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _cancellationToken = new CancellationTokenSource().Token;
        }

        //public HttpClientFactoryService(IHttpClientFactory httpClientFactory)
        //{
        //    _httpClient = httpClientFactory.CreateClient("ApiCaller");
        //    serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //    cancellationToken = new CancellationTokenSource().Token;
        //}

        public async Task<IApiResponse> Get<T>(string url)
        {
            using (var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cancellationToken))
            {
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var data = await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions);
                    return new ApiSuccessModel<T>
                    {
                        StatusCode = response.StatusCode,
                        Model = data
                    };
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new ApiErrorModel
                    {
                        ErrorMessage = content,
                        StatusCode = response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase
                    };
                }
            }
        }

        public async Task<IApiResponse> Post<T>(string url, IRequestModel requestModel)
        {
            HttpResponseMessage response;
            //var objectToAdd = JsonSerializer.Serialize(requestModel);
            //var stringContent = new StringContent(objectToAdd, Encoding.UTF8, "application/json");
            // response = await _httpClient.PostAsync(url, stringContent);
            //response.EnsureSuccessStatusCode();
            //var content = await response.Content.ReadAsStringAsync();
            //var createResponse = JsonSerializer.Deserialize<T>(content, _options);


            var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, requestModel);
            ms.Seek(0, SeekOrigin.Begin);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var requestContent = new StreamContent(ms))
            {
                request.Content = requestContent;
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        // response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStreamAsync();
                        var createResponse = await JsonSerializer.DeserializeAsync<T>(content, _serializerOptions);
                        return new ApiSuccessModel<T>
                        {
                            StatusCode = response.StatusCode,
                            Model = createResponse
                        };
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return new ApiErrorModel
                        {
                            ErrorMessage = content,
                            StatusCode = response.StatusCode,
                            ReasonPhrase = response.ReasonPhrase
                        };
                    }
                }
            }
        }

        public async Task<IApiResponse> Post<T>(FormUrlEncodedContent urlEncodedContent, string url)
        {
            HttpResponseMessage response;
            response = await Client.PostAsync(url, urlEncodedContent);

            if (response.IsSuccessStatusCode)
            {
                // response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStreamAsync();
                var createResponse = await JsonSerializer.DeserializeAsync<T>(content, _serializerOptions);
                return new ApiSuccessModel<T>
                {
                    StatusCode = response.StatusCode,
                    Model = createResponse
                };
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ApiErrorModel
                {
                    ErrorMessage = content,
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase
                };
            }
        }

        public async Task<IApiResponse> Put(string url, IRequestModel requestModel)
        {
            HttpResponseMessage response;
            //var objectToUpdate = JsonSerializer.Serialize(requestModel);
            //var stringContent = new StringContent(objectToUpdate, Encoding.UTF8, "application/json");
            //response = await _httpClient.PutAsync(url, stringContent);
            //response.EnsureSuccessStatusCode();

            var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, requestModel);
            ms.Seek(0, SeekOrigin.Begin);

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var requestContent = new StreamContent(ms))
            {
                request.Content = requestContent;
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        //response.EnsureSuccessStatusCode();
                        return new ApiSuccessModel { StatusCode = response.StatusCode };
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return new ApiErrorModel
                        {
                            ErrorMessage = content,
                            StatusCode = response.StatusCode,
                            ReasonPhrase = response.ReasonPhrase
                        };
                    }
                }
            }
        }

        public async Task<IApiResponse> Delete(string url)
        {
            HttpResponseMessage response;
            response = await Client.DeleteAsync(url);

            //var request = new HttpRequestMessage(HttpMethod.Delete, url);
            //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                //response.EnsureSuccessStatusCode();
                return new ApiSuccessModel { StatusCode = response.StatusCode };
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ApiErrorModel
                {
                    ErrorMessage = content,
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase
                };
            }
        }

    }
}
