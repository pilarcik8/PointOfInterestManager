using AUS2_S1;

// 1. Test pre QuadTree s fixnym seedom 42 a vypisom kazdych 10 operacii
var qMap = new Dictionary<int, (double x, double y)>();
var testQTree = new QTree<int, double>(
    topLeftX: 0,
    topLeftY: 0,
    bottomRightX: 100,
    bottomRightY: 100,
    getPosition: id => qMap[id],
    maxDepth: 6
);

var qTester = new Tester(
    qTree: testQTree,
    positionMap: qMap,
    operationsCount: 50,
    insertProbability: 70,
    seed: 42,
    printStep: 10
);
qTester.Run();

// 2. Test pre KDTree s nahodnym seedom a vypisom kazdych 10 operacii
var testKDTree = new KDTree<int>(
    k: 2,
    maxDepth: 100
);

var kdTester = new Tester(
    kdTree: testKDTree,
    operationsCount: 50,
    insertProbability: 70,
    seed: null,
    printStep: 10
);
kdTester.Run();
