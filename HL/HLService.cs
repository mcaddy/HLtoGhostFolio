using System.Globalization;
using System.Text;

namespace HL
{
    public class HLService
    {
        /// <summary>
        /// Parse a CSV file creating a List of Transactions
        /// </summary>
        /// <param name="csv">a string array derived from a HL CSV transation export</param>
        /// <returns>List of Transactions</returns>
        public static List<Transaction> ParseCSV(string[] csv)
        {
            // Title Row - Trade date,Settle date,Reference,Description,Unit cost (p),Quantity,Value (£)

            string TitleRowSearchTerm = "Trade date";

            bool pastHeader = false;

            List<Transaction> transactions = [];

            foreach (string line in csv)
            {
                if (line.StartsWith(TitleRowSearchTerm))
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
                            Transaction transaction = new()
                            {
                                TradeDate = DateTime.Parse(result[0]),
                                SettleDate = DateTime.Parse(result[1]),
                                Reference = result[2],
                                Description = result[3]
                            };

                            if (!result[4].Equals("n/a"))
                            {
                                transaction.UnitCost = float.Parse(result[4].Replace("\"", ""), CultureInfo.InvariantCulture);
                            }
                            if (!result[5].Equals("n/a"))
                            {
                                transaction.Quantity = float.Parse(result[5].Replace("\"", ""), CultureInfo.InvariantCulture);
                            }
                            if (!result[6].Equals("n/a"))
                            {
                                transaction.Value = float.Parse(result[6].Replace("\"", ""), CultureInfo.InvariantCulture);
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
