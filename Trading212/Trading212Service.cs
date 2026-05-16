using System.Collections.ObjectModel;
using System.Globalization;

namespace Trading212
{
    public static class Trading212Service
    {
        // Header: Action,Time,ISIN,Ticker,Name,Notes,ID,No. of shares,Price / share,Currency (Price / share),Exchange rate,Total,Currency (Total)
        private const int ColAction = 0;
        private const int ColTime = 1;
        private const int ColIsin = 2;
        private const int ColTicker = 3;
        private const int ColName = 4;
        private const int ColNotes = 5;
        private const int ColId = 6;
        private const int ColShares = 7;
        private const int ColPrice = 8;
        private const int ColPriceCurrency = 9;
        private const int ColExchangeRate = 10;
        private const int ColTotal = 11;
        private const int ColTotalCurrency = 12;
        private const int ExpectedColumns = 13;

        public static Collection<Trading212Transaction> ParseCSV(string[] csv)
        {
            if (csv == null || csv.Length == 0)
            {
                throw new ArgumentException("CSV input is null or empty", nameof(csv));
            }

            Collection<Trading212Transaction> transactions = [];

            foreach (string line in csv)
            {
                // Skip header row
                if (line.StartsWith("Action,", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] fields = SplitCsvLine(line);

                if (fields.Length != ExpectedColumns)
                {
                    Console.WriteLine($"Skipping invalid row (expected {ExpectedColumns} columns, got {fields.Length}): {line}");
                    continue;
                }

                string action = fields[ColAction].Trim();

                // Skip non-investment rows
                if (action.Equals("Deposit", StringComparison.OrdinalIgnoreCase)
                    || action.Equals("Withdrawal", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    Trading212Transaction transaction = new()
                    {
                        Action = action,
                        Time = DateTime.Parse(fields[ColTime].Trim(), CultureInfo.InvariantCulture),
                        Isin = fields[ColIsin].Trim(),
                        Ticker = fields[ColTicker].Trim(),
                        Name = fields[ColName].Trim(),
                        Notes = fields[ColNotes].Trim(),
                        Id = fields[ColId].Trim(),
                        PricePerShareCurrency = fields[ColPriceCurrency].Trim(),
                        TotalCurrency = fields[ColTotalCurrency].Trim(),
                    };

                    if (!string.IsNullOrEmpty(fields[ColShares]))
                    {
                        transaction.NumberOfShares = float.Parse(fields[ColShares].Trim(), CultureInfo.InvariantCulture);
                    }

                    if (!string.IsNullOrEmpty(fields[ColPrice]))
                    {
                        transaction.PricePerShare = float.Parse(fields[ColPrice].Trim(), CultureInfo.InvariantCulture);
                    }

                    if (!string.IsNullOrEmpty(fields[ColExchangeRate]))
                    {
                        transaction.ExchangeRate = float.Parse(fields[ColExchangeRate].Trim(), CultureInfo.InvariantCulture);
                    }

                    if (!string.IsNullOrEmpty(fields[ColTotal]))
                    {
                        transaction.Total = float.Parse(fields[ColTotal].Trim(), CultureInfo.InvariantCulture);
                    }

                    transactions.Add(transaction);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Failed to parse row: {line}");
                    Console.WriteLine(ex.Message);
                }
                catch (OverflowException ex)
                {
                    Console.WriteLine($"Failed to parse row: {line}");
                    Console.WriteLine(ex.Message);
                }
            }

            return transactions;
        }

        private static string[] SplitCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString());
            return [.. fields];
        }
    }
}
