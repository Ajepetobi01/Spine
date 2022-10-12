using System.Text.Json.Serialization;

namespace Spine.Services.Paystack.Common
{
    public class MetaDataObject
    {
        [JsonPropertyName("custom_fields")]
        public CustomFields[] custom_fields { get; set; }

    }
    public class CustomFields
    {
        [JsonPropertyName("display_name")]
        public string display_name { get; set; }
        [JsonPropertyName("variable_name")]
        public string variable_name { get; set; }
        [JsonPropertyName("value")]
        public string value { get; set; }
    }
}
