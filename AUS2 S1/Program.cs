// Made by Gemini 3.8 Flash
using AUS2_S1;
using AUS2_S1.KD_Tree;

// --- 1. STROM (QTree<string>, bez maxDepth) ---
Console.WriteLine("=== 1. STROM (QTree<string>, bez maxDepth) ===");

// Pozicie bodov - kazdy string ma priradenu (x, y) poziciu
var stringPositions = new Dictionary<string, (double x, double y)>
{
    ["Point1"] = (15, 15),
    ["Point2"] = (15, 15),  // rovnaka pozicia ako Point1
    ["Point3"] = (25, 25),
    ["Point4"] = (75, 20),
    ["Point5"] = (20, 75),
};

QTree<string> tree = new QTree<string>(
    0, 0, 100, 100, item => stringPositions[item], 2
);

// 5 bodov (2 maju rovnake kluce)
tree.Insert("Point1"); // 1. bod
tree.Insert("Point2"); // 2. bod s rovnakymi klucmi (ulozi sa do toho isteho uzla)
tree.Insert("Point3"); // 3. bod (vyvola split)
tree.Insert("Point4"); // 4. bod
tree.Insert("Point5"); // 5. bod

Console.WriteLine("Body boli uspesne vlozene do 1. stromu.");
var result1 = tree.Find(0, 0, 50, 50);
Console.WriteLine($"Najdene body v (0,0)-(50,50): {result1.Count}");
foreach (var p in result1)
{
    var pos = stringPositions[p];
    Console.WriteLine($" - {p}: [{pos.x}, {pos.y}]");
}

Console.WriteLine();

// --- 2. STROM (QTree<int>, maxDepth = 2) ---
Console.WriteLine("=== 2. STROM (QTree<int>, maxDepth = 2) ===");

var intPositions = new Dictionary<int, (double x, double y)>
{
    [101] = (15, 15),
    [102] = (15, 15),  // rovnaka pozicia
    [103] = (25, 25),
    [104] = (75, 20),
    [105] = (20, 75),
};

QTree<int> tree2 = new QTree<int>(0, 0, 100, 100, item => intPositions[item], maxDepth: 2);

// 5 bodov s inym typom dat (int, 2 maju rovnake kluce)
tree2.Insert(101); // 1. bod
tree2.Insert(102); // 2. bod s rovnakymi klucmi
tree2.Insert(103); // 3. bod
tree2.Insert(104); // 4. bod
tree2.Insert(105); // 5. bod

Console.WriteLine("Body boli uspesne vlozene do 2. stromu.");
var result2 = tree2.Find(0, 0, 50, 50);
Console.WriteLine($"Najdene body v (0,0)-(50,50): {result2.Count}");
foreach (var p in result2)
{
    var pos = intPositions[p];
    Console.WriteLine($" - ID {p}: [{pos.x}, {pos.y}]");
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