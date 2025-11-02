using System.Text.Json.Serialization;

namespace GhostFolio
{
    public class CountryAllocation
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }

        [JsonPropertyName("weight")]
        public required double Weight { get; set; }
    }
}
