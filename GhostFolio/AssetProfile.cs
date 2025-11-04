using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
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
        public Collection<CountryAllocation> Countries { get; } = [];

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
        public Collection <object> Holdings { get; } = [];

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("isin")]
        public string? Isin { get; set; }

        [JsonPropertyName("marketData")]
        public Collection<string> MarketData { get; } = [];

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("scraperConfiguration")]
        public object? ScraperConfiguration { get; set; }

        [JsonPropertyName("sectors")]
        public Collection<SectorAllocation> Sectors { get; } = [];

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

       // [JsonPropertyName("symbolMapping")]
       // public Dictionary<string, object> SymbolMapping { get; set; } = [];

        [JsonPropertyName("url")]
        public Uri? Url { get; set; }
    }
}
