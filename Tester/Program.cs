//QAUD:
// QuadTree: https://github.com/futurechris/QuadTree - 41 stars - 1 watching - 12 forks
// QuadTrees: https://github.com/splitice/QuadTrees.git -166 stars - 8 watching - 35 forks
// SpatialTrees: https://github.com/hroger1030/SpatialTrees - 1 star - 1 watching - 0 forks

//KD:
// NetTopologySuite: https://github.com/NetTopologySuite/NetTopologySuite -1.7k stars - 94 watching - 338 forks
// omni-collections: https://github.com/Codeturion/omni-collections - 9 stars - 0 watchers - 0 forks
// BKDTree: https://github.com/DevAM-Tools/BKDTree - 2 stars - 1 watchers - 0 forks

using DataStructTester.Testers;
using System.Collections;
using DataStructures;
using DataStructTester;

using DataStructures.Hybrid_Quad_Tree;
using DataStructures.QTree;
using DataStructures.KD_Tree;

var pointsParser = new CSVParser();
pointsParser.LoadPoints("C:\\Users\\Admin\\source\\repos\\AUS2 S1\\AUS2 S1\\Data\\slovakia_nodes.csv");

Console.WriteLine($"Body nahrate\nPocet bodov: {pointsParser.GetRowCount()}\n");

var rangesParser = new CSVParser();
rangesParser.LoadRanges("C:\\Users\\Admin\\source\\repos\\AUS2 S1\\AUS2 S1\\Data\\testovacie_oblasti.csv");

Console.WriteLine($"Obdlzniky nahrate\nPocet obdlznikov: {rangesParser.GetRowCount()}\n");

var randomDataTreeTester = new RandomDataTreeTester(
    kdTree: new KDTree<int, int>(2),
    operationsCount: 1000,
    insertProbability: 70,
    seed: 42
);
/*
var randomDataQuadTreeTester = new RandomDataQuadTreeTester(
    quadTree: new QTree<int>(),
    operationsCount: 1000,
    insertProbability: 70,
    seed: 42
);*/