using System.Text.Json.Serialization;

namespace Spine.Services.Mono.Common
{
    public abstract class BaseResult
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
