// Vygenerovane / upravene pomocou AI (Modely: Gemini 3.8 Flash, Claude Sonnet 4.6) - Prompt 26, 27, 34, 35
using AUS2_S1.Tester;

var pointsParser = new CSVParser();
pointsParser.LoadPoints("C:\\Users\\Admin\\source\\repos\\AUS2 S1\\AUS2 S1\\Data\\slovakia_nodes.csv");

var rangesParser = new CSVParser();
rangesParser.LoadRanges("C:\\Users\\Admin\\source\\repos\\AUS2 S1\\AUS2 S1\\Data\\testovacie_oblasti.csv");

// Rychlostny tester vlastnych aj externych stromov.
ExternalDataTreeTester.Run(pointsParser, rangesParser, maxRanges : 100);
