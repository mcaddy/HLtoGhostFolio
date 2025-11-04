using System.Collections.ObjectModel;
using System.Resources;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GhostFolio
{
    public class GhostFolioService(Config config)
    {
        private const string importUrl = "{0}api/v1/import?dryRun=false";
        private const string authUrl = "{0}api/v1/auth/anonymous";
        private const string accountUrl = "{0}api/v1/account/"; 

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
       
        private readonly Config config = config;

        private string authToken = string.Empty;

        public bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(authToken); }
        }

        public async Task<string> AuthenticateAsync()
        {
            this.authToken = await AuthenticateAsync(config.AccessToken, config.BaseUrl).ConfigureAwait(false);
            return authToken;
        }

        /// <summary>
        /// Authenticate against a GhostFolio instance
        /// </summary>
        /// <param name="accessToken">User Access Token</param>
        /// <returns>An Auth token</returns>
        public static async Task<string> AuthenticateAsync(string accessToken, Uri baseUrl)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var requestBody = new
                    {
                        accessToken
                    };

                    string jsonBody = JsonSerializer.Serialize(requestBody);
                    using StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(new Uri(string.Format(authUrl, baseUrl)), content).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    string authResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // Deserialize the JSON to extract the auth token
                    AuthResponse? auth = JsonSerializer.Deserialize<AuthResponse>(authResponse, jsonSerializerOptions);

                    if (auth != null)
                    {
                        return auth.AuthToken;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return string.Empty;

        }

        public async Task ImportAsync(Collection<Activity> activities)
        {
            if (string.IsNullOrEmpty(this.authToken))
            {
                throw new InvalidOperationException("Not authenticated. Please call AuthenticateAsync() first.");
            }

            await ImportAsync(this.authToken, activities, config.BondWinSymbol, config.ManagementFeeSymbol, config.InterestSymbol, config.BaseUrl).ConfigureAwait(false);
        }

        /// <summary>
        /// Import the Activities into Ghostfolio
        /// </summary>
        /// <param name="authToken">valid auth token</param>
        /// <param name="activities">List of activities</param>
        public static async Task ImportAsync(string authToken, Collection<Activity> activities, string bondWinSymbol, string managementFeeSymbol, string interestSymbol, Uri baseUrl)
        {
            var assetProfiles = new List<AssetProfile>(2)
            {
                // Create new Asset Profiles for the Mgt Fee and Interest entries, ensures they get created if they don't already exist
                new() {
                    Currency = Currency.GBP,
                    DataSource = DataSource.MANUAL,
                    Name = "Management Fee",
                    Symbol = managementFeeSymbol,
                },
                new() {
                    Currency = Currency.GBP,
                    DataSource = DataSource.MANUAL,
                    Name = "Interest",
                    Symbol = interestSymbol,
                },
                new() {
                    Currency = Currency.GBP,
                    DataSource = DataSource.MANUAL,
                    Name = "Premium Bond Win",
                    Symbol = bondWinSymbol,
                }
            };

            // Assemble the body of the GF import
            var importBody = new
            {
                assetProfiles,
                activities,
            };
            
            ResourceManager resourceManager = new("GhostFolio.Properties.Resources", typeof(GhostFolioService).Assembly);

            string json = JsonSerializer.Serialize(importBody, jsonSerializerOptions);
            using StringContent content = new(json, Encoding.UTF8, "application/json");

            // Invoke the import
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                var response = await client.PostAsync(new Uri(string.Format(importUrl, baseUrl)), content).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine(resourceManager.GetString("ImportBadResponseConsoleMessage", System.Globalization.CultureInfo.CurrentCulture));
                    Console.WriteLine(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                    return;
                }

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine(resourceManager.GetString("ImportResultConsoleMessage", System.Globalization.CultureInfo.CurrentCulture));
                Console.WriteLine(responseString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(resourceManager.GetString("ImportErrorConsoleMessage", System.Globalization.CultureInfo.CurrentCulture));
                Console.WriteLine(ex.Message);
            }
        }
        

        public async Task<Account?> GetAccoundByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(this.authToken))
            {
                throw new InvalidOperationException("Not authenticated. Please call AuthenticateAsync() first.");
            }

            return await GetAccoundByNameAsync(name, authToken, config.BaseUrl).ConfigureAwait(false);
        }

        public static async Task<Account?> GetAccoundByNameAsync(string name, string authToken, Uri baseUrl)
        { 
            Account foundAccount = new() { Name= name, Id = Guid.Empty };

            if (string.IsNullOrEmpty(authToken))
            {
                throw new InvalidOperationException("Not authenticated. Please call AuthenticateAsync() first.");
            }

            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                try
                {
                    var response = await client.GetAsync(new Uri(string.Format(accountUrl, baseUrl))).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // Parse the JSON document
                    using JsonDocument doc = JsonDocument.Parse(responseString);

                    // Navigate to the "accounts" array
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("accounts", out JsonElement accountsElement))
                    {
                        throw new Exception("JSON does not contain 'accounts' array.");
                    }

                    // Enumerate and find matching name
                    foreach (var account in accountsElement.EnumerateArray())
                    {
                        var accountName = account.GetProperty("name").GetString();
                        if (string.Equals(accountName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            foundAccount.Id = account.GetProperty("id").GetGuid();
                            return foundAccount;
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP error while fetching accounts: {httpEx.Message}");
                    return null;
                }

                return null; // Not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing accounts JSON: {ex.Message}");
                return null;
            }
        }
    }
}
