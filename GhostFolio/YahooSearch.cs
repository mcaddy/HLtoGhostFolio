using System.Text.Json;

namespace GhostFolio
{
    public static class YahooSearch
    {
        private const string SearchUrl = "https://query1.finance.yahoo.com/v1/finance/search";

        public static async Task<IReadOnlyList<YahooQuote>> SearchByIsinAsync(string isin)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible)");

            var uri = new Uri($"{SearchUrl}?q={Uri.EscapeDataString(isin)}&quotesCount=10&newsCount=0&enableFuzzyQuery=false");

            try
            {
                using var response = await client.GetAsync(uri).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("quotes", out JsonElement quotesEl))
                {
                    return [];
                }

                var results = new List<YahooQuote>();
                foreach (var q in quotesEl.EnumerateArray())
                {
                    if (!q.TryGetProperty("symbol", out var symbolEl))
                    {
                        continue;
                    }

                    string symbol = symbolEl.GetString() ?? string.Empty;
                    if (string.IsNullOrEmpty(symbol))
                    {
                        continue;
                    }

                    results.Add(new YahooQuote(
                        Symbol: symbol,
                        ShortName: q.TryGetProperty("shortname", out var sn) ? sn.GetString() : null,
                        LongName: q.TryGetProperty("longname", out var ln) ? ln.GetString() : null,
                        QuoteType: q.TryGetProperty("quoteType", out var qt) ? qt.GetString() : null
                    ));
                }

                return results;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Yahoo Finance search failed: {ex.Message}");
                return [];
            }
        }

        public static void AppendToLookupCsv(string symbol, string fundName, string isin, Config config)
        {
            ArgumentNullException.ThrowIfNull(config);
            File.AppendAllText(config.YahooLookupPath, $"{Environment.NewLine}{symbol},{fundName},{isin}");
        }
    }

    public record YahooQuote(string Symbol, string? ShortName, string? LongName, string? QuoteType)
    {
        public string DisplayName => LongName ?? ShortName ?? Symbol;
    }
}
