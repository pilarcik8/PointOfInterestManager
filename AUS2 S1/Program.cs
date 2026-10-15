// 1. Test pre QuadTree s fixným seedom 42
using AUS2_S1;

var qMap = new Dictionary<int, (double x, double y)>();
var testQTree = new QTree<int>(0, 0, 100, 100, id => qMap[id], maxDepth: 6);

var qTester = new Tester(testQTree, qMap, operationsCount: 50, insertProbability: 70, seed: 42);
qTester.Run();

// 2. Test pre KDTree s náhodným seedom
var testKDTree = new KDTree<int>(k: 2, maxDepth: 100);

var kdTester = new Tester(testKDTree, operationsCount: 50, insertProbability: 70, seed: null);
kdTester.Run();
