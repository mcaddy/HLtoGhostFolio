namespace GhostFolio
{
    internal class Yahoo
    {
        static public string LookupYahooCode(string StockName, Config config)
        {
            string[] lines = File.ReadAllLines(config.YahooLookupPath);

            foreach (string line in lines)
            {
                string[] columns = line.Split(",");
                if (columns[1].Equals(StockName, StringComparison.OrdinalIgnoreCase))
                {
                    return columns[0];
                }
            }
            
            throw new KeyNotFoundException($"Yahoo code not found for '{StockName}'");
        }
    }
}
