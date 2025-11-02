using System.Text.Json.Serialization;

namespace GhostFolio
{
    public class SectorAllocation
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("weight")]
        public required double Weight { get; set; }
    }

}