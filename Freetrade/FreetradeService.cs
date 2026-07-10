using System.Collections.ObjectModel;
using System.Globalization;

namespace Freetrade
{
    public static class FreetradeService
    {
        // Header:
        // Title,Type,Timestamp,Account Currency,Total Amount in Account Currency,Buy / Sell,Ticker,ISIN,
        // Price per Share in Account Currency,Stamp Duty,Quantity,Venue,Order ID,Order Type,
        // Instrument Currency,Total Amount in Instrument Currency,Price per Share,FX Rate,Base FX Rate,
        // FX Fee (BPS),FX Fee Amount,
        // Dividend Ex Date,Dividend Pay Date,Dividend Eligible Quantity,Dividend Amount Per Share,
        // Dividend Gross Distribution Amount,Dividend Net Distribution Amount,
        // Dividend Withheld Tax Percentage,Dividend Withheld Tax Amount,
        // Stock Split Ex Date,Stock Split Pay Date,Stock Split New ISIN,Stock Split Rate of Share Outturn From,
        // Stock Split Rate of Share Outturn To,Stock Split Maintain Holding of Initial ISIN,
        // Stock Split New Share Quantity,Stock Split Rate of Cash Outturn Amount,Stock Split Rate of Cash Outturn Currency,
        // Stock Split Cash Outturn Received Amount,Stock Split Has Fractional Payout,
        // Stock Split Rate of Fractional Payout Amount,Stock Split Rate of Fractional Payout Currency,
        // Stock Split Fractional Payout Cash Received Amount,Stock Split Fractional Payout Cash Received Currency
        private const int ColTitle = 0;
        private const int ColType = 1;
        private const int ColTimestamp = 2;
        private const int ColAccountCurrency = 3;
        private const int ColTotalAmountInAccountCurrency = 4;
        private const int ColBuySell = 5;
        private const int ColTicker = 6;
        private const int ColIsin = 7;
        private const int ColPricePerShareInAccountCurrency = 8;
        private const int ColStampDuty = 9;
        private const int ColQuantity = 10;
        private const int ColOrderId = 12;
        private const int ColOrderType = 13;
        private const int ColInstrumentCurrency = 14;
        private const int ColFxFeeAmount = 20;
        private const int ColDividendEligibleQuantity = 23;
        private const int ColDividendAmountPerShare = 24;
        private const int ColDividendNetDistributionAmount = 26;
        private const int ColDividendWithheldTaxAmount = 28;
        private const int MinExpectedColumns = 21;

        public const string HeaderPrefix = "Title,Type,Timestamp,";

        public static Collection<FreetradeTransaction> ParseCSV(string[] csv)
        {
            if (csv == null || csv.Length == 0)
            {
                throw new ArgumentException("CSV input is null or empty", nameof(csv));
            }

            Collection<FreetradeTransaction> transactions = [];

            foreach (string line in csv)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith(HeaderPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] fields = SplitCsvLine(line);

                if (fields.Length < MinExpectedColumns)
                {
                    Console.WriteLine($"Skipping invalid row (expected at least {MinExpectedColumns} columns, got {fields.Length}): {line}");
                    continue;
                }

                try
                {
                    FreetradeTransaction transaction = new()
                    {
                        Title = fields[ColTitle].Trim(),
                        Type = fields[ColType].Trim(),
                        Timestamp = DateTime.Parse(fields[ColTimestamp].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                        AccountCurrency = fields[ColAccountCurrency].Trim(),
                        BuySell = fields[ColBuySell].Trim(),
                        Ticker = fields[ColTicker].Trim(),
                        Isin = fields[ColIsin].Trim(),
                        OrderId = fields[ColOrderId].Trim(),
                        OrderType = fields[ColOrderType].Trim(),
                        InstrumentCurrency = fields[ColInstrumentCurrency].Trim(),
                        TotalAmountInAccountCurrency = ParseDecimal(fields[ColTotalAmountInAccountCurrency]),
                        PricePerShareInAccountCurrency = ParseDecimal(fields[ColPricePerShareInAccountCurrency]),
                        StampDuty = ParseDecimal(fields[ColStampDuty]),
                        Quantity = ParseDecimal(fields[ColQuantity]),
                        FxFeeAmount = ParseDecimal(GetField(fields, ColFxFeeAmount)),
                        DividendEligibleQuantity = ParseDecimal(GetField(fields, ColDividendEligibleQuantity)),
                        DividendAmountPerShare = ParseDecimal(GetField(fields, ColDividendAmountPerShare)),
                        DividendNetDistributionAmount = ParseDecimal(GetField(fields, ColDividendNetDistributionAmount)),
                        DividendWithheldTaxAmount = ParseDecimal(GetField(fields, ColDividendWithheldTaxAmount)),
                    };

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

        private static string GetField(string[] fields, int index)
        {
            return index < fields.Length ? fields[index] : string.Empty;
        }

        private static decimal ParseDecimal(string value)
        {
            string trimmed = value.Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                return 0m;
            }

            return decimal.Parse(trimmed, CultureInfo.InvariantCulture);
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
