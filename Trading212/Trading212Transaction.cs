namespace Trading212
{
    public class Trading212Transaction
    {
        public Trading212Transaction()
        {
            action = string.Empty;
            time = DateTime.MinValue;
            isin = string.Empty;
            ticker = string.Empty;
            name = string.Empty;
            notes = string.Empty;
            id = string.Empty;
            pricePerShareCurrency = string.Empty;
            totalCurrency = string.Empty;
        }

        private string action;

        public string Action
        {
            get { return action; }
            set { action = value; }
        }

        private DateTime time;

        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        private string isin;

        public string Isin
        {
            get { return isin; }
            set { isin = value; }
        }

        private string ticker;

        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string notes;

        public string Notes
        {
            get { return notes; }
            set { notes = value; }
        }

        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private float numberOfShares;

        public float NumberOfShares
        {
            get { return numberOfShares; }
            set { numberOfShares = value; }
        }

        private float pricePerShare;

        public float PricePerShare
        {
            get { return pricePerShare; }
            set { pricePerShare = value; }
        }

        private string pricePerShareCurrency;

        public string PricePerShareCurrency
        {
            get { return pricePerShareCurrency; }
            set { pricePerShareCurrency = value; }
        }

        private float exchangeRate;

        public float ExchangeRate
        {
            get { return exchangeRate; }
            set { exchangeRate = value; }
        }

        private float total;

        public float Total
        {
            get { return total; }
            set { total = value; }
        }

        private string totalCurrency;

        public string TotalCurrency
        {
            get { return totalCurrency; }
            set { totalCurrency = value; }
        }
    }
}
