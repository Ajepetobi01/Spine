using System.Text.Json.Serialization;

namespace Spine.Services.Paystack.Common
{
    public abstract class BaseResult
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
