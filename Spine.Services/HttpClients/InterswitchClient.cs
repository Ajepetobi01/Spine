using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spine.Common.Helper;

namespace Spine.Services.HttpClients
{

    public class InterswitchClient
    {
        private readonly HttpClient _httpClient;
        private readonly CancellationToken _cancellationToken;
        private readonly JsonSerializerOptions _serializerOptions;

        private readonly string ClientId;
        private readonly string TerminalId;
        private readonly string Secret;
        private readonly string TimeStamp;
        private readonly string Nonce;


        public InterswitchClient(HttpClient client, IConfiguration configuration)
        {
            _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _cancellationToken = new CancellationTokenSource().Token;

            var baseUrl = configuration["Interswitch:BaseUrl"];

            ClientId = configuration["Interswitch:ClientId"];
            TerminalId = configuration["Interswitch:TerminalId"];
            Secret = configuration["Interswitch:Secret"];
            Nonce = SequentialGuid.Create().ToString(); // GetUniqueKey(20);
            TimeStamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();

            _httpClient = client;
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("TerminalID", TerminalId);
            _httpClient.DefaultRequestHeaders.Add("Timestamp", TimeStamp);
            _httpClient.DefaultRequestHeaders.Add("Nonce", Nonce);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "InterswitchAuth " + Base64Encode(ClientId));
            // _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

        }

        public string GetSignature(string httpVerb, string url)
        {
            StringBuilder signature = new StringBuilder(httpVerb);
            signature.Append('&').Append(Uri.EscapeDataString(url))
                .Append('&').Append(TimeStamp)
                .Append('&').Append(Nonce)
                .Append('&').Append(ClientId)
                .Append('&').Append(Secret);

            //if (SignedParameters != null && !SignedParameters.Equals(""))
            //{
            //    signature.Append("&").Append(SignedParameters);
            //}

            return ComputeHash(signature.ToString());
        }

        public static string ComputeHash(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            byte[] result;

            SHA512 shaM = new SHA512Managed();
            result = shaM.ComputeHash(data);
            return Convert.ToBase64String(result);
        }

        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars = "1234567890".ToCharArray();
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string SHA1(string plainText)
        {
            SHA1 HashTool = new SHA1Managed();
            Byte[] PhraseAsByte = System.Text.Encoding.UTF8.GetBytes(string.Concat(plainText));
            Byte[] EncryptedBytes = HashTool.ComputeHash(PhraseAsByte);
            HashTool.Clear();
            return Convert.ToBase64String(EncryptedBytes);
        }



        public async Task<IApiResponse> Get<T>(string url)
        {
            var signature = GetSignature("GET", _httpClient.BaseAddress + url);

            _httpClient.DefaultRequestHeaders.Add("Signature", signature);
            _httpClient.DefaultRequestHeaders.Add("SignatureMethod", "SHA512");

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cancellationToken))
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
            var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, requestModel);
            ms.Seek(0, SeekOrigin.Begin);

            var signature = GetSignature("POST", _httpClient.BaseAddress + url);

            _httpClient.DefaultRequestHeaders.Add("Signature", signature);
            _httpClient.DefaultRequestHeaders.Add("SignatureMethod", "SHA512");

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var requestContent = new StreamContent(ms);
            request.Content = requestContent;
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using (response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationToken))
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
}
