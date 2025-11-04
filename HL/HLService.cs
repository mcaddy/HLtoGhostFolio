using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace HL
{
    public static class HLService
    {
        /// <summary>
        /// Parse a CSV file creating a List of Transactions
        /// </summary>
        /// <param name="csv">a string array derived from a HL CSV transation export</param>
        /// <returns>List of Transactions</returns>
        public static Collection<Transaction> ParseCSV(string[] csv)
        {
            // Title Row - Trade date,Settle date,Reference,Description,Unit cost (p),Quantity,Value (£)

            if (csv == null || csv.Length == 0)
            {
                throw new ArgumentException("CSV input is null or empty", nameof(csv));
            }

            string TitleRowSearchTerm = "Trade date";

            bool pastHeader = false;

            Collection<Transaction> transactions = [];

            foreach (string line in csv)
            {
                if (line.StartsWith(TitleRowSearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    pastHeader = true;
                }
                else
                {
                    if (pastHeader)
                    {
                        var result = new List<string>();
                        var sb = new StringBuilder();
                        bool inQuotes = false;

                        for (int i = 0; i < line.Length; i++)
                        {
                            char c = line[i];

                            if (c == '\"')
                            {
                                inQuotes = !inQuotes;
                                continue;
                            }

                            if (c == ',' && !inQuotes)
                            {
                                result.Add(sb.ToString().Trim());
                                sb.Clear();
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }

                        result.Add(sb.ToString().Trim()); // last field

                        if (result.Count != 7)
                        {
                            Console.WriteLine($"Skipping invalid row: {line}");
                            continue;
                        }

                        try
                        {

                            DateTime tradeDate = DateTime.Parse(result[0], CultureInfo.CurrentCulture);

                                DateTime settleDate = DateTime.MinValue;

                            if (!string.IsNullOrEmpty(result[1])){
                                settleDate = DateTime.Parse(result[1], CultureInfo.CurrentCulture);
                            }
                            else
                            {
                                settleDate = tradeDate;
                            }

                            Transaction transaction = new()
                            {
                                TradeDate = tradeDate,
                                SettleDate = settleDate,
                                Reference = result[2],
                                Description = result[3]
                            };

                            if (!result[4].Equals("n/a", StringComparison.OrdinalIgnoreCase))
                            {
                                transaction.UnitCost = float.Parse(result[4].Replace("\"", "", StringComparison.OrdinalIgnoreCase), CultureInfo.InvariantCulture);
                            }
                            if (!result[5].Equals("n/a", StringComparison.OrdinalIgnoreCase))
                            {
                                transaction.Quantity = float.Parse(result[5].Replace("\"", "", StringComparison.OrdinalIgnoreCase), CultureInfo.InvariantCulture);
                            }
                            if (!string.IsNullOrEmpty(result[6]) && !result[6].Equals("n/a", StringComparison.OrdinalIgnoreCase))
                            {
                                transaction.Value = float.Parse(result[6].Replace("\"", "", StringComparison.OrdinalIgnoreCase), CultureInfo.InvariantCulture);
                            }


                            transactions.Add(transaction);
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to parse row: {line}");
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }

            return transactions;

        }
    }
}
