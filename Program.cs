using GhostFolio;
using HL;
using Trading212;
using System.Text.Json;
using System.Globalization;
using System.Resources;
using System.Collections.ObjectModel;

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
    string configFile = await File.ReadAllTextAsync(configPath).ConfigureAwait(false);
    Config? config = JsonSerializer.Deserialize<Config>(configFile);

    if (config == null)
    {
        Console.WriteLine("Config file is invalid!");
        return;
    }

    GhostFolioService ghostFolio = new(config);
    await ghostFolio.AuthenticateAsync().ConfigureAwait(false);

    if (ghostFolio.IsAuthenticated)
    {
        Account? account = await ghostFolio.GetAccoundByNameAsync(accountName).ConfigureAwait(false);

        if (account == null)
        {
            Console.WriteLine($"Account '{accountName}' not found!");
            return;
        }

        string[] csvContents = await File.ReadAllLinesAsync(filePath).ConfigureAwait(false);
        Console.WriteLine($"Read {csvContents.Length} entries from CSV for '{accountName}'");

        Collection<Activity> activities;

        if (IsTrading212Csv(csvContents))
        {
            activities = ParseTrading212Activities(csvContents, account.Id);
        }
        else
        {
            activities = ParseHLActivities(csvContents, account.Id, config);
        }

        Console.WriteLine($"Identifed {activities.Count} activities for '{accountName}'");

        await ghostFolio.ImportAsync(activities).ConfigureAwait(false);
    }
}

static bool IsTrading212Csv(string[] csv)
{
    foreach (string line in csv)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            return line.StartsWith("Action,", StringComparison.OrdinalIgnoreCase);
        }
    }

    return false;
}

static Collection<Activity> ParseTrading212Activities(string[] csvContents, Guid accountId)
{
    Collection<Trading212Transaction> transactions = Trading212Service.ParseCSV(csvContents);
    Collection<Activity> activities = [];

    foreach (Trading212Transaction item in transactions)
    {
        try
        {
            activities.Add(new Activity(item, accountId, Currency.GBP));
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"Skipping unsupported Trading212 action '{item.Action}': {ex.Message}");
        }
    }

    return activities;
}

static Collection<Activity> ParseHLActivities(string[] csvContents, Guid accountId, Config config)
{
    Collection<HLTransaction> transactions = HLService.ParseCSV(csvContents);
    List<string> skipFunds = [];
    Collection<Activity> activities = [];

    foreach (HLTransaction item in transactions)
    {
        if (!item.Reference.Equals("CARD WEB", StringComparison.OrdinalIgnoreCase)
            && !item.Reference.Equals("FPC", StringComparison.OrdinalIgnoreCase)
            && !item.Reference.Equals("TRANSFER", StringComparison.OrdinalIgnoreCase)
            && !item.Reference.Equals("LISA", StringComparison.OrdinalIgnoreCase)
            && !item.Description.Equals("SIPP CONTRIBUTION CLAIM", StringComparison.OrdinalIgnoreCase)
            && !item.Description.StartsWith("Unit Rebate Re-Investment", StringComparison.OrdinalIgnoreCase)
            && !item.Description.StartsWith("Income Re-Investment", StringComparison.OrdinalIgnoreCase)
            )
        {
            Activity activity = new(item, accountId, Currency.GBP, config);

            if (!skipFunds.Contains(activity.Symbol))
            {
                activities.Add(activity);
            }
        }
    }

    return activities;
}
