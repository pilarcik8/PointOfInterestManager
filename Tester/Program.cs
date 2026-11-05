//QAUD:
// QuadTree: https://github.com/futurechris/QuadTree - 41 stars - 1 watching - 12 forks
// QuadTrees: https://github.com/splitice/QuadTrees.git -166 stars - 8 watching - 35 forks
// SpatialTrees: https://github.com/hroger1030/SpatialTrees - 1 star - 1 watching - 0 forks

//KD:
// NetTopologySuite: https://github.com/NetTopologySuite/NetTopologySuite -1.7k stars - 94 watching - 338 forks
// omni-collections: https://github.com/Codeturion/omni-collections - 9 stars - 0 watchers - 0 forks
// BKDTree: https://github.com/DevAM-Tools/BKDTree - 2 stars - 1 watchers - 0 forks

using System.Collections;
using DataStructures;
using DataStructTester;

using DataStructures.Hybrid_Quad_Tree;
using DataStructures.QTree;
using DataStructures.KD_Tree;

using DataStructTester.Testers;

var pointsParser = new CSVParser();
var projectPath = Directory.GetCurrentDirectory();
var pointsFilePath = Path.Combine(projectPath, "..\\..\\..\\Data\\slovakia_nodes.csv");
var rangesFilePath = Path.Combine(projectPath, "..\\..\\..\\Data\\testovacie_oblasti.csv");

pointsParser.LoadPoints(pointsFilePath);

Console.WriteLine($"Body nahrate\nPocet bodov: {pointsParser.GetRowCount()}\n");

var rangesParser = new CSVParser();
rangesParser.LoadRanges(rangesFilePath);

Console.WriteLine($"Obdlzniky nahrate\nPocet obdlznikov: {rangesParser.GetRowCount()}\n");

// Random data
var randomDataTreeTester = new RandomDataTreeTester();
randomDataTreeTester.TestQTreeTester(operationsCount: 1000, insertProbability: 70, seed: 42, maxDepht: 10);
randomDataTreeTester.TestKDTreeTester(operationsCount: 1000, insertProbability: 70, seed: 42);
randomDataTreeTester.TestHybridTreeTester(operationsCount: 1000, insertProbability: 70, seed: 42, maxDepht: 10);

// External data
ExternalDataTreeTester.RunBenchmark(
    points: pointsParser,
    ranges: rangesParser,
    maxPoints: 1000,
    maxRanges: 100
);

ExternalDataTreeTester.RunCorrectnessTest(
    points: pointsParser,
    ranges: rangesParser,
    maxPoints: 1000,
    maxRanges: 100,
    insertCheckInterval: 10,
    findCheckInterval: 10
);


