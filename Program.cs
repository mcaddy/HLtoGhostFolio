using GhostFolio;
using HL;
using System.Text.Json;
using System.Globalization;
using System.Resources;

ResourceManager resourceManager = new("Resources", typeof(Program).Assembly);

if (args.Length == 3)
{
    if (args[0].Equals("--help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("-h", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine(resourceManager.GetString("UsageText", CultureInfo.CurrentCulture));
        return;
    }

    await importAsync(args[0], args[1], args[2]).ConfigureAwait(false);
}

static async Task importAsync(string accountName, string filePath, string configPath)
{
    // Load the Config
    string configFile = await File.ReadAllTextAsync(configPath).ConfigureAwait(false);
    Config? config = JsonSerializer.Deserialize<Config>(configFile);

    if (config == null)
    {
        Console.WriteLine("Config file is invalid!");
        return;
    }   

    // Get authenticated
    GhostFolioService ghostFolio = new(config);
    await ghostFolio.AuthenticateAsync().ConfigureAwait(false);

    // if Sucesseful attempt import
    if (ghostFolio.IsAuthenticated)
    {
        Account? account = await ghostFolio.GetAccoundByNameAsync(accountName).ConfigureAwait(false);

        if (account == null)
        {
            Console.WriteLine($"Account '{accountName}' not found!");
            return;
        }

        string[] CsvContents = await File.ReadAllLinesAsync(filePath).ConfigureAwait(false);
        Console.WriteLine($"Read {CsvContents.Length} entries from CSV for '{accountName}'");

        List<Transaction> transactions = HLService.ParseCSV(CsvContents);

        // Convert the List of all transactions into GhostFolio Activities
        List<string> skipFunds = [];

        List<Activity> activities = [];

        foreach (Transaction item in transactions)
        {
            // Need to skip cash input ammounts for now, might we do something with them later?
            // Also skipping TRANSFERs
            if (!item.Reference.Equals("CARD WEB",StringComparison.OrdinalIgnoreCase) 
                && !item.Reference.Equals("FPC", StringComparison.OrdinalIgnoreCase) 
                && !item.Reference.Equals("TRANSFER", StringComparison.OrdinalIgnoreCase) 
                && !item.Reference.Equals("LISA", StringComparison.OrdinalIgnoreCase) 
                && !item.Description.Equals("SIPP CONTRIBUTION CLAIM", StringComparison.OrdinalIgnoreCase))
            {
                Activity activity = new(item, account.Id, Currency.GBP, config);

                //Some funds are broken on Yahoo
                if (!skipFunds.Contains(activity.Symbol))
                {
                    activities.Add(activity);
                }
            }
        }
        
        Console.WriteLine($"Identifed {activities.Count} activities for '{accountName}'");

        await ghostFolio.ImportAsync(activities).ConfigureAwait(false);
    }
}