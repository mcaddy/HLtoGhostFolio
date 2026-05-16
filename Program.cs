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
            activities = await ParseHLActivitiesAsync(csvContents, account.Id, config).ConfigureAwait(false);
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

static async Task<Collection<Activity>> ParseHLActivitiesAsync(string[] csvContents, Guid accountId, Config config)
{
    Collection<HLTransaction> transactions = HLService.ParseCSV(csvContents);
    List<string> skipSymbols = [];
    HashSet<string> skipFundNames = [];
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
            && !item.Reference.StartsWith("HLS", StringComparison.OrdinalIgnoreCase))
        {
            if (skipFundNames.Contains(item.FundName()))
            {
                continue;
            }

            try
            {
                Activity activity = new(item, accountId, Currency.GBP, config);

                if (!skipSymbols.Contains(activity.Symbol))
                {
                    activities.Add(activity);
                }
            }
            catch (KeyNotFoundException)
            {
                string fundName = item.FundName();
                string? resolved = await ResolveSymbolWithPromptAsync(fundName, config).ConfigureAwait(false);

                if (resolved != null)
                {
                    // Symbol saved to CSV by the prompt helper; re-read picks it up
                    activities.Add(new Activity(item, accountId, Currency.GBP, config));
                }
                else
                {
                    Console.WriteLine($"Skipping all transactions for \"{fundName}\".");
                    skipFundNames.Add(fundName);
                }
            }
        }
    }

    return activities;
}

static async Task<string?> ResolveSymbolWithPromptAsync(string fundName, Config config)
{
    Console.WriteLine();
    Console.WriteLine($"Fund not found in lookup CSV: \"{fundName}\"");
    Console.WriteLine("Look up the ISIN on https://www.hl.co.uk and enter it below.");
    Console.Write("ISIN (or press Enter to skip this fund): ");

    string isin = Console.ReadLine()?.Trim() ?? string.Empty;

    if (string.IsNullOrEmpty(isin))
    {
        return null;
    }

    Console.WriteLine($"Searching Yahoo Finance for ISIN {isin}...");
    IReadOnlyList<YahooQuote> results = await YahooSearch.SearchByIsinAsync(isin).ConfigureAwait(false);

    if (results.Count == 0)
    {
        Console.WriteLine("No results found. Check the ISIN and try again next run.");
        return null;
    }

    Console.WriteLine("Results:");
    for (int i = 0; i < results.Count; i++)
    {
        Console.WriteLine($"  {i + 1}. [{results[i].QuoteType ?? "?"}] {results[i].Symbol} — {results[i].DisplayName}");
    }

    Console.Write($"Select the correct result (1-{results.Count}) or 0 to skip: ");
    if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > results.Count)
    {
        Console.WriteLine("Skipping.");
        return null;
    }

    YahooQuote selected = results[choice - 1];
    YahooSearch.AppendToLookupCsv(selected.Symbol, fundName, isin, config);
    Console.WriteLine($"Saved: \"{fundName}\" → {selected.Symbol}");

    return selected.Symbol;
}
