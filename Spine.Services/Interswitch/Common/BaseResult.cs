using System.Text.Json.Serialization;

namespace Spine.Services.Interswitch.Common
{
    public abstract class BaseResult
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
