namespace GhostFolio
{
    public class Config
    {
        public string BaseUrl { get; set; } = "https://localhost:3333";
        public string AccessToken { get; set; } = string.Empty;
        public string InterestSymbol { get; set; } = "14a69cb9-1e31-43fa-b320-83703d8ed75c";
        public string ManagementFeeSymbol { get; set; } = "14a69cb9-1e31-43fa-b320-83703d8ed74b";
        public string YahooLookupPath { get; set; } = string.Empty;
    }
}
