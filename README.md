# HLtoGhostFolio

I couldn't find an easy way to get my Hargreaves Lansdown Trades into Ghostfolio so I decided to knock up an import program.

A couple of hurdles exist when conducting this automation, the largest of whch is that the HL transaction CSVs use a text based asset description that needs to be resolved into a Yahoo ticker, until I find a way to automate this search I have added a CSV based lookup feature.

In order to run the importer you need to first setup a config file in the following format.

```json
{
  "BaseUrl": "[Path to the Ghostfolio Install with port] e.g. http:\\Ghostfolio.local:3333",
  "AccessToken": "[Access token for your user account] e.g. 3606fdef9f424e0092b21c31164b1a7a1d900d91539ec3a798963153a41528ab06406b48d081702f8620e0d508615c2f04fae912754715537dd895ba0fb9ea8b",
  "YahooLookupPath" : "[Path to the Yahoo lookup file] e.g. C:\\Users\\username\\OneDrive\\Documents\\HLco.uk\\HLtoYahooLookup.csv"
}
```

then you need a mapping file between the HL transaction descriptions and Yahoo ticker in the following format.
Only the first two colums are used at this time, however the ISIN and countries data will be automated if I find a suitable API

```csv
0P0000Z8O1.L	abrdn Global Smaller Companies Class S - Accumulation (GBP)	GB00BBX46522	[ { "code": "US", "weight": 0.4886 }, { "code": "GB", "weight": 0.0754 }, { "code": "DE", "weight": 0.0586 }, { "code": "AU", "weight": 0.0522 }, { "code": "FR", "weight": 0.0459 }, { "code": "MX", "weight": 0.0310 }, { "code": "SE", "weight": 0.0216 }, { "code": "IN", "weight": 0.0180 },  { "code": "TW", "weight": 0.0162 }, { "code": "ES", "weight": 0.0132 }, { "code": "IT", "weight": 0.0114 }, { "code": "PL", "weight": 0.0098 }]
0P0001AE23.L	Allianz Global Artificial Intelligence Accumulation - GBP - Class PT	LU1597246385	[ { "code": "US", "weight": 0.7911 }, { "code": "TW", "weight": 0.0332 }, { "code": "CA", "weight": 0.0308 }, { "code": "SG", "weight": 0.0225 }, { "code": "NL", "weight": 0.0215 }, { "code": "LU", "weight": 0.0145 }, { "code": "HK", "weight": 0.0123 }, { "code": "IE", "weight": 0.0121 }, { "code": "GB", "weight": 0.0106 }, { "code": "CN", "weight": 0.0056 } ]
```

then you can call the importer with the following syntax

```cmd
HLtoGhostFolio "GhostFolio Account Name" "Path to HL CSV file" "Path to Config file"
```
