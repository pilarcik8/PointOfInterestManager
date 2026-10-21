// Vygenerovane / upravene pomocou AI (Modely: Gemini 3.8 Flash, Claude Sonnet 4.6) - Prompt 26, 27, 34, 35
using System.Collections.Generic;
using AUS2_S1;
using AUS2_S1.KD_Tree;
using AUS2_S1.Tester;

// 1. Test QTree s datami zo suborov v Data/
// TData = long (id uzla), TPosition = double (lat/lon)
var qMap = new Dictionary<long, (double x, double y)>();
var qTree = new QTree<long, double>(
    topLeftX: 47.0,
    topLeftY: 16.0,
    bottomRightX: 50.0,
    bottomRightY: 23.0,
    getPosition: id => qMap[id],
    maxDepth: 16
);

var pointsParser = new CSVParser();
pointsParser.LoadPoints("Data/slovakia_nodes.csv");

var rangesParser = new CSVParser();
rangesParser.LoadRanges("Data/testovacie_oblasti.csv");

var qTester = new TesterFromCSVData<long, double>(
    qTree: qTree,
    positionMap: qMap,
    points: pointsParser,
    ranges: rangesParser,
    checkInterval: 100000
);

// 2. Test KDTree s datami zo suborov v Data/
// TData = long (id/data uzla), TPosition = double (lat/lon suradnice)
var kdTree = new KDTree<long>(
    k: 2,
    maxDepth: 100
);

var kdTester = new TesterFromCSVData<long, double>(
    kdTree: kdTree,
    points: pointsParser,
    ranges: rangesParser,
    checkInterval: 100000
);
