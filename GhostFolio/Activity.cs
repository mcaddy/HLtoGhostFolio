using HL;
using System.Text.Json.Serialization;

namespace GhostFolio
{
    public class Activity
    {
        public Activity(Transaction transaction, Guid targetAccountId, Currency targetCurrency, Config config)
        {
            accountId = targetAccountId;
            currency = targetCurrency;

            date = transaction.TradeDate;

            if (transaction.Reference.Equals("MANAGE FEE"))
            {
                type = ActivityType.FEE;
                fee = -transaction.Value;
                dataSource = DataSource.MANUAL;
                symbol = config.ManagementFeeSymbol;
            }
            else if (transaction.Reference.Equals("INTEREST"))
            {
                type = ActivityType.INTEREST;
                quantity = 1;
                unitPrice = transaction.UnitCost / 100;
                dataSource = DataSource.MANUAL;
                symbol = config.InterestSymbol;
            }
            else
            {
                if (transaction.Value < 0)
                {
                    type = ActivityType.BUY;
                }
                else
                {
                    type = ActivityType.SELL;
                }

                unitPrice = transaction.UnitCost / 100;
                quantity = transaction.Quantity;
                dataSource = DataSource.YAHOO;
                symbol = Yahoo.LookupYahooCode(transaction.FundName(), config);
            }

            comment = transaction.Description;
            tags = [];
        }

        private Guid accountId;

        [JsonPropertyName("accountId")]
        public Guid AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }

        private string comment;

        [JsonPropertyName("comment")]
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        private float fee;

        [JsonPropertyName("fee")]
        public float Fee
        {
            get { return fee; }
            set { fee = value; }
        }

        private float quantity;

        [JsonPropertyName("quantity")]
        public float Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private ActivityType type;

        [JsonPropertyName("type")]
        public ActivityType Type
        {
            get { return type; }
            set { type = value; }
        }

        private float unitPrice;

        [JsonPropertyName("unitPrice")]
        public float UnitPrice
        {
            get { return unitPrice; }
            set { unitPrice = value; }
        }

        private Currency currency;

        [JsonPropertyName("currency")]
        public Currency Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        private DataSource dataSource;

        [JsonPropertyName("dataSource")]
        public DataSource DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }

        private DateTime date;

        [JsonPropertyName("date")]
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        private string symbol;

        [JsonPropertyName("symbol")]
        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        private List<string> tags;

        [JsonPropertyName("tags")]
        public List<string> Tags
        {
            get { return tags; }
            set { tags = value; }
        }

    }
}
