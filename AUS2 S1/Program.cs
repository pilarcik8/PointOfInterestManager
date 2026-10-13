// Made by Gemini 3.8 Flash
using AUS2_S1;
using AUS2_S1.KD_Tree;

// --- 1. STROM (QTree<string>, bez maxDepth) ---
Console.WriteLine("=== 1. STROM (QTree<string>, bez maxDepth) ===");
Polygon boundary = new Polygon(0, 0, 100, 100);
QTree<string> tree = new QTree<string>(boundary);

// 5 polygonov (2 maju rovnaku poziciu)
tree.CreateAndInsertPolygon(10, 10, 20, 20); // 1. polygon
tree.CreateAndInsertPolygon(10, 10, 20, 20); // 2. ten isty polygon
tree.CreateAndInsertPolygon(60, 10, 80, 30); // 3. polygon
tree.CreateAndInsertPolygon(10, 60, 30, 80); // 4. polygon
tree.CreateAndInsertPolygon(60, 60, 80, 80); // 5. polygon

// 5 pointov (2 maju rovnake kluce)
tree.CreateAndInsertPoint("Point1", _ => (15, 15)); // 1. bod
tree.CreateAndInsertPoint("Point2", _ => (15, 15)); // 2. bod s rovnakymi klucmi
tree.CreateAndInsertPoint("Point3", _ => (25, 25)); // 3. bod (vyvola split s predchadzajucimi)
tree.CreateAndInsertPoint("Point4", _ => (75, 20)); // 4. bod
tree.CreateAndInsertPoint("Point5", _ => (20, 75)); // 5. bod

Console.WriteLine("Polygony a body boli uspesne vlozene do 1. stromu.");
var result1 = tree.Find(new Polygon(0, 0, 50, 50));
Console.WriteLine($"Najdene polygony v (0,0)-(50,50): {result1.Polygons.Count}");
Console.WriteLine($"Najdene body v (0,0)-(50,50): {result1.Points.Count}");
foreach (var p in result1.Points)
{
    Console.WriteLine($" - {p.Data}: [{p.X}, {p.Y}]");
}

Console.WriteLine();

// --- 2. STROM (QTree<int>, maxDepth = 2) ---
Console.WriteLine("=== 2. STROM (QTree<int>, maxDepth = 2) ===");
Polygon boundary2 = new Polygon(0, 0, 100, 100);
QTree<int> tree2 = new QTree<int>(boundary2, maxDepth: 2);

// 5 polygonov (2 maju rovnaku poziciu)
tree2.CreateAndInsertPolygon(10, 10, 20, 20); // 1. polygon
tree2.CreateAndInsertPolygon(10, 10, 20, 20); // 2. ten isty polygon
tree2.CreateAndInsertPolygon(60, 10, 80, 30); // 3. polygon
tree2.CreateAndInsertPolygon(10, 60, 30, 80); // 4. polygon
tree2.CreateAndInsertPolygon(60, 60, 80, 80); // 5. polygon

// 5 pointov s inym typom dat (int namiesto string, 2 maju rovnake kluce)
tree2.CreateAndInsertPoint(101, _ => (15, 15)); // 1. bod
tree2.CreateAndInsertPoint(102, _ => (15, 15)); // 2. bod s rovnakymi klucmi
tree2.CreateAndInsertPoint(103, _ => (25, 25)); // 3. bod
tree2.CreateAndInsertPoint(104, _ => (75, 20)); // 4. bod
tree2.CreateAndInsertPoint(105, _ => (20, 75)); // 5. bod

Console.WriteLine("Polygony a body boli uspesne vlozene do 2. stromu.");
var result2 = tree2.Find(new Polygon(0, 0, 50, 50));
Console.WriteLine($"Najdene polygony v (0,0)-(50,50): {result2.Polygons.Count}");
Console.WriteLine($"Najdene body v (0,0)-(50,50): {result2.Points.Count}");
foreach (var p in result2.Points)
{
    Console.WriteLine($" - ID {p.Data}: [{p.X}, {p.Y}]");
}

Console.WriteLine();

// --- 3. STROM (KDTree<double>, k = 2) ---
Console.WriteLine("=== 3. STROM (KDTree<double>, k = 2) ===");
KDTree<double> kdTree = new KDTree<double>(k: 2);

// Pomocna funkcia na vytvorenie klucov
LinkedList<KDKey<double>> MakeKeys(double x, double y)
{
    var keys = new LinkedList<KDKey<double>>();
    keys.AddLast(new KDKey<double>(x));
    keys.AddLast(new KDKey<double>(y));
    return keys;
}

// Mnozina bodov vopred na vybudovanie pomocou medianov
// Data je double (napr. ID alebo hodnota), kluce su X a Y suradnice
var initialPoints = new List<(double data, LinkedList<KDKey<double>> keys)>
{
    (1, MakeKeys(10, 20)),  // P1
    (2, MakeKeys(25, 40)),  // P2
    (3, MakeKeys(50, 50)),  // P3
    (4, MakeKeys(70, 10)),  // P4
    (5, MakeKeys(80, 85)),  // P5
    (6, MakeKeys(15, 30))   // P6
};

// Vybudovanie vyvazeneho KD stromu pomocou medianov
kdTree.Build(initialPoints);
Console.WriteLine($"KDTree vybudovany z {initialPoints.Count} pociatocnych bodov pomocou medianov.");

// Dynamicke vlozenie dalsieho bodu
kdTree.Insert(7, MakeKeys(35, 35));
Console.WriteLine("Dynamicky vlozeny bod: P7 [35, 35]");

// Bodove vyhladavanie
var foundNode = kdTree.Find(MakeKeys(50, 50));
Console.WriteLine($"Bodove vyhladavanie pre [50, 50]: {(foundNode != null ? $"Najdeny, data count: {foundNode.Data.Count}" : "Nenajdeny")}");

// Intervalove vyhladavanie v rozmedzi X in [10, 60] a Y in [10, 60]
var rangeResults = kdTree.FindRange(MakeKeys(10, 10), MakeKeys(60, 60));

Console.WriteLine($"Intervalove vyhladavanie v <[10, 10], [60, 60]>: najdenych {rangeResults.Count} uzlov");
foreach (var node in rangeResults)
{
    Console.WriteLine($" - Data: [{string.Join(", ", node.Data)}]");
}