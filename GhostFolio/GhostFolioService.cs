using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GhostFolio
{
    public class GhostFolioService(Config config)
    {
        private const string importUrl = "{0}/api/v1/import?dryRun=false";
        private const string authUrl = "{0}/api/v1/auth/anonymous";
        private const string accountUrl = "{0}/api/v1/account/"; 

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
       
        private readonly Config config = config;

        private string authToken = string.Empty;

        public bool IsAuthenticated
        {
            get { return authToken != string.Empty; }
        }

        public async Task<string> AuthenticateAsync()
        {
            this.authToken = await AuthenticateAsync(config.AccessToken, config.BaseUrl);
            return authToken;
        }

        /// <summary>
        /// Authenticate against a GhostFolio instance
        /// </summary>
        /// <param name="accessToken">User Access Token</param>
        /// <returns>An Auth token</returns>
        public static async Task<string> AuthenticateAsync(string accessToken, string baseUrl)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var requestBody = new
                    {
                        accessToken
                    };

                    var jsonBody = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(string.Format(authUrl, baseUrl), content);
                    response.EnsureSuccessStatusCode();

                    string authResponse = await response.Content.ReadAsStringAsync();

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

        public async Task ImportAsync(List<Activity> activities)
        {
            if (string.IsNullOrEmpty(this.authToken))
            {
                throw new InvalidOperationException("Not authenticated. Please call AuthenticateAsync() first.");
            }

            await ImportAsync(this.authToken, activities, config.ManagementFeeSymbol, config.InterestSymbol, config.BaseUrl);
        }

        /// <summary>
        /// Import the Activities into Ghostfolio
        /// </summary>
        /// <param name="authToken">valid auth token</param>
        /// <param name="activities">List of activities</param>
        public static async Task ImportAsync(string authToken, List<Activity> activities, string managementFeeSymbol, string interestSymbol, string baseUrl)
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
                }
            };

            // Assemble the body of the GF import
            var importBody = new
            {
                assetProfiles,
                activities,
            };

            string json = JsonSerializer.Serialize(importBody, jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Invoke the import
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                var response = await client.PostAsync(string.Format(importUrl,baseUrl), content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Import response:");
                Console.WriteLine(responseString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during import:");
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<Account?> GetAccoundByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(this.authToken))
            {
                throw new InvalidOperationException("Not authenticated. Please call AuthenticateAsync() first.");
            }

            return await GetAccoundByNameAsync(name, authToken, config.BaseUrl);
        }

        public static async Task<Account?> GetAccoundByNameAsync(string name, string authToken, string baseUrl)
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
                    var response = await client.GetAsync(string.Format(accountUrl, baseUrl));
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();

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
