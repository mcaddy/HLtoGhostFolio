# HLtoGhostFolio

I couldn't find an easy way to get my Hargreaves Lansdown Trades into Ghostfolio so I decided to knock up an import program.

A couple of hurdles exist when conducting this automation, the largest of whch is that the HL transaction CSVs use a text based asset description that needs to be resolved into a Yahoo ticker, until I find a way to automate this search I have added a CSV based lookup feature.

In order to run the importer you need to first setup a config file in the following format.

```json
{
  "BaseUrl": "http://myGhostfolio.local:3333",
  "AccessToken": "3606fdef9f424e0092b21c31164b1a7a1d900d91539ec3a798963153a41528ab06406b48d081702f8620e0d508615c2f04fae912754715537dd895ba0fb9ea8b",
  "YahooLookupPath" : "C:\\Users\\username\\OneDrive\\Documents\\HLco.uk\\HLtoYahooLookup.csv"
}
```

then you can call the importer with the following syntax

```
HLtoGhostFolio "GhostFolio Account Name" "Path to HL CSV file" "Path to Config file"
```
