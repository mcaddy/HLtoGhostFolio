using System.Text.RegularExpressions;

namespace HL
{
    public class Transaction
    {
        public Transaction()
        {
            tradeDate = DateTime.MinValue;
            settleDate = DateTime.MinValue;
            description = string.Empty;
            reference = string.Empty;
        }

        private DateTime tradeDate;

        public DateTime TradeDate
        {
            get { return tradeDate; }
            set { tradeDate = value; }
        }

        private DateTime settleDate;

        public DateTime SettleDate
        {
            get { return settleDate; }
            set { settleDate = value; }
        }

        private string reference;

        public string Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private float unitCost;

        public float UnitCost
        {
            get { return unitCost; }
            set { unitCost = value; }
        }

        private float quantity;

        public float Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private float transactionValue;

        public float Value
        {
            get { return transactionValue; }
            set { transactionValue = value; }
        }

        private static string StripString(string input, string remove)
        {
            return input.StartsWith(remove, StringComparison.OrdinalIgnoreCase) ? input[remove.Length..].Trim() : input;
        }

        public string FundName()
        {
            // Regex pattern to match the trailing 'number @ number'
            // Allows numbers starting with a dot (like .121) or normal numbers
            string pattern = @"^(.*)\s+([0-9]*\.?[0-9]+)\s*@\s*([0-9]*\.?[0-9]+)$";

            string result = StripString(description, "Unit Rebate Re-Investment");
            result = StripString(result, "Income Re-Investment");

            var match = Regex.Match(result, pattern);

            if (match.Success)
            {
                result = match.Groups[1].Value.Trim();
            }

            result = result.EndsWith("Fee Sale -", StringComparison.OrdinalIgnoreCase) ? result[..^"Fee Sale -".Length].Trim() : result;

            return result;
        }
    }
}
