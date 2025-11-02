using System.Text.Json.Serialization;

namespace GhostFolio
{
    // Asset profile class
    public class AssetProfile
    {
        [JsonPropertyName("assetClass")]
        public string? AssetClass { get; set; }

        [JsonPropertyName("assetSubClass")]
        public string? AssetSubClass { get; set; } 

        [JsonPropertyName("comment")]
        public string? Comment { get; set; } 

        [JsonPropertyName("countries")]
        public List<CountryAllocation> Countries { get; set; } = [];

        [JsonPropertyName("currency")]
        public Currency Currency { get; set; }

        [JsonPropertyName("cusip")]
        public string? Cusip { get; set; }

        [JsonPropertyName("dataSource")]
        public DataSource DataSource { get; set; }

        [JsonPropertyName("figi")]
        public string? Figi { get; set; }

        [JsonPropertyName("figiComposite")]
        public string? FigiComposite { get; set; }

        [JsonPropertyName("figiShareClass")]
        public string? FigiShareClass { get; set; }

        [JsonPropertyName("holdings")]
        public List<object> Holdings { get; set; } = [];

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("isin")]
        public string? Isin { get; set; }

        [JsonPropertyName("marketData")]
        public List<string> MarketData { get; set; } = [];

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("scraperConfiguration")]
        public object? ScraperConfiguration { get; set; }

        [JsonPropertyName("sectors")]
        public List<SectorAllocation> Sectors { get; set; } = [];

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

       // [JsonPropertyName("symbolMapping")]
       // public Dictionary<string, object> SymbolMapping { get; set; } = [];

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
