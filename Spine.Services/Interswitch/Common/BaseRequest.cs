using System.Text.Json.Serialization;

namespace Spine.Services.Interswitch.Common
{
    public abstract class BaseRequest
    {
        /// <summary>
        /// REQUIRED. Terminal ID (This is usually provided by Interswitch but you can use the Terminal ID provided in this documentation to test)
        /// </summary>
        [JsonPropertyName("terminalId")]
        public string TerminalId { get; set; }
    }
}
