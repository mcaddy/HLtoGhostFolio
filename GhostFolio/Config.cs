namespace GhostFolio
{
    public class Config
    {
        public Uri BaseUrl { get; set; } = new Uri("https://localhost:3333");
        public string AccessToken { get; set; } = string.Empty;
        public string InterestSymbol { get; set; } = "14a69cb9-1e31-43fa-b320-83703d8ed75c";
        public string BondWinSymbol { get; set; } = "eeed4980-bb96-4aad-bb1e-659739e5f8ee";
        public string ManagementFeeSymbol { get; set; } = "14a69cb9-1e31-43fa-b320-83703d8ed74b";
        public string YahooLookupPath { get; set; } = string.Empty;
    }
}
