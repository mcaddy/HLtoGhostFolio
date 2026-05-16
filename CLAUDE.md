# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HLtoGhostFolio is a C# console application that imports financial transactions from Hargreaves Lansdown (HL) brokerage CSV exports into Ghostfolio, an open-source portfolio management system. The tool automates the reconciliation of investment holdings across platforms by parsing HL transaction data and converting it into Ghostfolio-compatible activity records.

### Key Problem Solved

- **Asset Mapping Challenge**: HL transaction CSVs contain only text-based fund descriptions (e.g., "abrdn Global Smaller Companies Class S - Accumulation (GBP)"), not ticker symbols. The tool resolves these descriptions to Yahoo Finance tickers using a CSV-based lookup file.
- **Multi-Account Support**: Supports importing into different Ghostfolio accounts (HL SIPP, HL S&S ISA, HL LISA, NS&I Premium Bonds, etc.).
- **Transaction Type Handling**: Intelligently categorizes transactions (BUY/SELL/INTEREST/FEE) based on HL reference codes and descriptions.

## Build and Run

### Prerequisites
- .NET 10.0 SDK

### Build
```bash
dotnet build HLtoGhostFolio.sln
```

### Release Build
```bash
dotnet build -c Release HLtoGhostFolio.sln
```

### Run
```bash
dotnet run --project HLtoGhostFolio.csproj -- "<AccountName>" "<PathToHLCSV>" "<PathToConfigFile>"
```

**Example:**
```bash
dotnet run --project HLtoGhostFolio.csproj -- "HL SIPP" "C:\Transactions.csv" "C:\config.json"
```

### Debug Profiles
LaunchSettings.json contains predefined profiles for different account imports:
- `HL SIPP > GhostFolio`
- `HL S&S ISA > GhostFolio`
- `HL LISA > GhostFolio`
- `NS&I Premium Bonds > GhostFolio`

Run a specific profile with:
```bash
dotnet run --project HLtoGhostFolio.csproj --launch-profile "HL SIPP > GhostFolio"
```

### Code Analysis
```bash
dotnet build /p:EnforceCodeStyleInBuild=True
```

## Architecture

### Three-Layer Structure

The solution is organized as a layered architecture with three projects:

1. **HLtoGhostFolio (Console Application)** - `Program.cs`
   - Entry point orchestrating the import pipeline
   - Handles command-line arguments validation
   - Manages the overall workflow: authenticate → load account → parse CSV → filter transactions → convert to activities → import

2. **HL Library** - Hargreaves Lansdown transaction parsing
   - **HLService**: Static CSV parser that handles:
     - Header detection ("Trade date" marker)
     - Quote-aware CSV field splitting (handles commas within quoted fund names)
     - Field parsing with null/n/a handling
     - Date parsing respecting current culture
   - **HLTransaction**: Data model for parsed HL CSV rows with computed FundName() method
     - Uses regex to extract fund name from HL's description format: `"FundName Quantity @ UnitPrice"`
     - Strips prefixes like "Unit Rebate Re-Investment", "Income Re-Investment", "Fee Sale -"

3. **GhostFolio Library** - Ghostfolio API integration and activity conversion
   - **GhostFolioService**: HTTP client for Ghostfolio REST API
     - Handles bearer token authentication via `/api/v1/auth/anonymous`
     - Imports activities via `/api/v1/import?dryRun=false`
     - Fetches accounts from `/api/v1/account/`
   - **Activity**: Converts HLTransaction into Ghostfolio import format
     - Maps HL reference types to ActivityType (BUY/SELL/INTEREST/FEE)
     - Special handling for:
       - MANAGE FEE → FEE activity using config.ManagementFeeSymbol
       - INTEREST → INTEREST activity using config.InterestSymbol
       - BOND WIN → INTEREST activity using config.BondWinSymbol
       - BOND BUY → BUY activity with manual GF_Premium Bond symbol
       - Standard transactions → BUY/SELL with Yahoo datasource lookup
   - **Config**: Deserializes JSON configuration containing:
     - BaseUrl: Ghostfolio instance URL
     - AccessToken: User authentication token
     - YahooLookupPath: Path to CSV ticker mapping file
     - Special asset symbols (bond/interest/fee)
   - **Yahoo**: Static lookup that queries the CSV file to resolve fund names to tickers
   - Supporting enums: ActivityType, Currency, DataSource
   - Supporting models: Account, AssetProfile, AuthResponse, CountryAllocation, SectorAllocation

### Data Flow

```
HL CSV File
    ↓
[HLService.ParseCSV] → Collection<HLTransaction>
    ↓
[Program.cs filters] → Remove cash entries (CARD WEB, FPC, TRANSFER, LISA)
                     → Remove reinvestment entries (Unit Rebate, Income Re-Investment, SIPP CONTRIBUTION)
    ↓
[Activity constructor] → Transforms each HLTransaction:
                        - Looks up Yahoo ticker via Yahoo.LookupYahooCode()
                        - Determines activity type (BUY/SELL/INTEREST/FEE)
                        - Calculates unitPrice, quantity, fees
                        → Collection<Activity>
    ↓
[GhostFolioService.ImportAsync]
    - Creates AssetProfile records for special symbols
    - POSTs import payload (assetProfiles + activities) to Ghostfolio API
    - Returns import result/errors
```

## Configuration

### config.json Format
```json
{
  "BaseUrl": "http://ghostfolio.local:3333",
  "AccessToken": "<your-ghostfolio-user-token>",
  "YahooLookupPath": "C:\Users\username\HLtoYahooLookup.csv",
  "InterestSymbol": "14a69cb9-1e31-43fa-b320-83703d8ed75c",
  "BondWinSymbol": "eeed4980-bb96-4aad-bb1e-659739e5f8ee",
  "ManagementFeeSymbol": "14a69cb9-1e31-43fa-b320-83703d8ed74b"
}
```

### Yahoo Lookup CSV Format
First two columns are mandatory (ticker and fund name); additional columns are ignored pending API automation:
```csv
0P0000Z8O1.L,abrdn Global Smaller Companies Class S - Accumulation (GBP),GB00BBX46522,[...]
0P0001AE23.L,Allianz Global Artificial Intelligence Accumulation - GBP - Class PT,LU1597246385,[...]
```

## Key Patterns and Conventions

### Code Style
- **Nullable Reference Types**: Enabled project-wide (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled for cleaner code
- **Code Analysis**: `AnalysisLevel=latest-all` with enforcement in build
- **Culture**: Locale set to en-GB (`<NeutralLanguage>en-GB</NeutralLanguage>`)
- **Property Naming**: Full property syntax with backing fields (not auto-properties) for Activity and HLTransaction models

### Exception Handling
- CSV parsing catches and logs individual row parse failures without aborting the entire import
- HTTP errors are caught and logged; bad requests return response body for debugging
- Missing lookups throw KeyNotFoundException with descriptive messages

### String Comparisons
- Uses `StringComparison.OrdinalIgnoreCase` for culture-invariant matching (HL reference codes, fund names)
- Uses `CultureInfo.CurrentCulture` for parsing dates and floats from CSV

### Resource Strings
- Localized strings stored in Properties/Resources.resx
- Loaded via ResourceManager for console messages and API error responses

## Development Notes

- **No External NuGet Dependencies**: The project uses only .NET Framework libraries (System.Net.Http, System.Text.Json, System.Collections.ObjectModel).
- **Async-First**: All I/O operations (file read, HTTP) are async with `.ConfigureAwait(false)`.
- **Future Enhancement**: Comments mention planned automation of ISIN/country data lookups via a third-party API to eliminate manual CSV maintenance.
- **Known Limitation**: Some Yahoo funds are marked as "broken" (hardcoded skip list in Program.cs, currently empty).
- **Manual Asset Symbols**: Ticker UUIDs for interest, fees, and bond wins are manually configured in config.json; these create manual-datasource assets in Ghostfolio.

