namespace Freetrade
{
    public class FreetradeTransaction
    {
        public FreetradeTransaction()
        {
            title = string.Empty;
            type = string.Empty;
            accountCurrency = string.Empty;
            buySell = string.Empty;
            ticker = string.Empty;
            isin = string.Empty;
            orderId = string.Empty;
            orderType = string.Empty;
            instrumentCurrency = string.Empty;
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        private DateTime timestamp;

        public DateTime Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        private string accountCurrency;

        public string AccountCurrency
        {
            get { return accountCurrency; }
            set { accountCurrency = value; }
        }

        private decimal totalAmountInAccountCurrency;

        public decimal TotalAmountInAccountCurrency
        {
            get { return totalAmountInAccountCurrency; }
            set { totalAmountInAccountCurrency = value; }
        }

        private string buySell;

        public string BuySell
        {
            get { return buySell; }
            set { buySell = value; }
        }

        private string ticker;

        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; }
        }

        private string isin;

        public string Isin
        {
            get { return isin; }
            set { isin = value; }
        }

        private decimal pricePerShareInAccountCurrency;

        public decimal PricePerShareInAccountCurrency
        {
            get { return pricePerShareInAccountCurrency; }
            set { pricePerShareInAccountCurrency = value; }
        }

        private decimal stampDuty;

        public decimal StampDuty
        {
            get { return stampDuty; }
            set { stampDuty = value; }
        }

        private decimal quantity;

        public decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private string orderId;

        public string OrderId
        {
            get { return orderId; }
            set { orderId = value; }
        }

        private string orderType;

        public string OrderType
        {
            get { return orderType; }
            set { orderType = value; }
        }

        private string instrumentCurrency;

        public string InstrumentCurrency
        {
            get { return instrumentCurrency; }
            set { instrumentCurrency = value; }
        }

        private decimal fxFeeAmount;

        public decimal FxFeeAmount
        {
            get { return fxFeeAmount; }
            set { fxFeeAmount = value; }
        }

        private decimal dividendEligibleQuantity;

        public decimal DividendEligibleQuantity
        {
            get { return dividendEligibleQuantity; }
            set { dividendEligibleQuantity = value; }
        }

        private decimal dividendAmountPerShare;

        public decimal DividendAmountPerShare
        {
            get { return dividendAmountPerShare; }
            set { dividendAmountPerShare = value; }
        }

        private decimal dividendNetDistributionAmount;

        public decimal DividendNetDistributionAmount
        {
            get { return dividendNetDistributionAmount; }
            set { dividendNetDistributionAmount = value; }
        }

        private decimal dividendWithheldTaxAmount;

        public decimal DividendWithheldTaxAmount
        {
            get { return dividendWithheldTaxAmount; }
            set { dividendWithheldTaxAmount = value; }
        }
    }
}
