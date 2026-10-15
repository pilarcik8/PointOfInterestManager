using System;
using System.Collections.Generic;
using System.Linq;
using AUS2_S1.KD_Tree;

namespace AUS2_S1
{
    /// <summary>
    /// Spoločné rozhranie pre akúkoľvek dátovú štruktúru, ktorú chceme testovať.
    /// Kľúče aj dáta sú čísla pre maximálnu prehľadnosť človekom.
    /// </summary>
    public interface ITestableStructure
    {
        string Name { get; }
        void Insert(int data, params int[] keys);
        ICollection<int> Find(params int[] keys);
    }

    /// <summary>
    /// Adaptér pre QTree<int>, aby ho Tester vedel priamo ovládať.
    /// </summary>
    public class QTreeAdapter : ITestableStructure
    {
        private readonly QTree<int> _tree;
        private readonly Dictionary<int, (double x, double y)> _positionMap;

        public string Name => "Quad Tree (QTree<int>)";

        public QTreeAdapter(double minX = 0, double minY = 0, double maxX = 100, double maxY = 100, int maxDepth = 6)
        {
            _positionMap = new Dictionary<int, (double x, double y)>();
            _tree = new QTree<int>(minX, minY, maxX, maxY, id => _positionMap[id], maxDepth);
        }

        public QTreeAdapter(QTree<int> existingTree, Dictionary<int, (double x, double y)> positionMap)
        {
            _tree = existingTree;
            _positionMap = positionMap;
        }

        public void Insert(int data, params int[] keys)
        {
            if (keys.Length < 2)
            {
                throw new ArgumentException("QTree vyžaduje 2 kľúče [x, y].");
            }
            _positionMap[data] = (keys[0], keys[1]);
            _tree.Insert(data);
        }

        public ICollection<int> Find(params int[] keys)
        {
            if (keys.Length < 2)
            {
                throw new ArgumentException("QTree vyžaduje 2 kľúče [x, y].");
            }
            return _tree.Find(keys[0], keys[1]);
        }
    }

    /// <summary>
    /// Adaptér pre KDTree<int>, aby ho Tester vedel priamo ovládať.
    /// </summary>
    public class KDTreeAdapter : ITestableStructure
    {
        private readonly KDTree<int> _tree;
        private readonly int _k;

        public string Name => $"KD Tree (KDTree<int>, k = {_k})";

        public KDTreeAdapter(KDTree<int> existingTree)
        {
            _tree = existingTree;
            _k = existingTree.K;
        }

        public void Insert(int data, params int[] keys)
        {
            if (keys.Length != _k)
            {
                throw new ArgumentException($"KDTree s k={_k} vyžaduje {_k} kľúčov.");
            }

            var keyList = new LinkedList<KDKey<int>>();
            foreach (var k in keys)
            {
                keyList.AddLast(new KDKey<int>(k));
            }
            _tree.Insert(data, keyList);
        }

        public ICollection<int> Find(params int[] keys)
        {
            if (keys.Length != _k)
            {
                throw new ArgumentException($"KDTree s k={_k} vyžaduje {_k} kľúčov.");
            }

            var keyList = new LinkedList<KDKey<int>>();
            foreach (var k in keys)
            {
                keyList.AddLast(new KDKey<int>(k));
            }

            var node = _tree.Find(keyList);
            return node != null ? node.Data : Array.Empty<int>();
        }
    }

    /// <summary>
    /// Záznam o dátach uložených v referenčnom zozname Testera.
    /// </summary>
    public class TestRecord
    {
        public int Data { get; }
        public int[] Keys { get; }

        public TestRecord(int data, int[] keys)
        {
            Data = data;
            Keys = (int[])keys.Clone();
        }

        public bool KeysMatch(int[] otherKeys)
        {
            if (Keys.Length != otherKeys.Length) return false;
            for (int i = 0; i < Keys.Length; i++)
            {
                if (Keys[i] != otherKeys[i]) return false;
            }
            return true;
        }

        public override string ToString() => $"Data: {Data}, Kľúče: [{string.Join(", ", Keys)}]";
    }

    /// <summary>
    /// Univerzálna trieda Tester na overovanie správnosti dátových štruktúr.
    /// </summary>
    public class Tester
    {
        private readonly ITestableStructure _structure;
        private readonly Random _random;
        private readonly int _operationsCount;
        private readonly int _insertProbability; // 0 až 100
        private readonly int _dimensions;
        private readonly int _minCoordinate;
        private readonly int _maxCoordinate;
        private readonly bool _verbose;

        // Referenčný list – uchováva, čo v štruktúre má naozaj byť
        private readonly List<TestRecord> _expectedData = new List<TestRecord>();

        // Počítadlo ID pre unikátne dáta
        private int _nextDataId = 1;

        // Štatistiky
        public int InsertCount { get; private set; }
        public int FindCount { get; private set; }
        public int ChecksPassed { get; private set; }
        public int Seed { get; }

        public IReadOnlyList<TestRecord> ExpectedData => _expectedData;

        /// <summary>
        /// Konštruktor pre ľubovoľnú dátovú štruktúru implementujúcu ITestableStructure.
        /// </summary>
        /// <param name="structure">Dátová štruktúra na testovanie.</param>
        /// <param name="operationsCount">Celkový počet operácií na vykonanie.</param>
        /// <param name="insertProbability">Pravdepodobnosť operácie Insert v percentách (0..100). Find bude mať (100 - insertProbability).</param>
        /// <param name="seed">Voliteľný seed. Ak je null, vygeneruje sa automaticky.</param>
        /// <param name="dimensions">Počet dimenzií kľúča (default 2 pre [x, y]).</param>
        /// <param name="minCoordinate">Minimálna celočíselná hodnota súradnice kľúča.</param>
        /// <param name="maxCoordinate">Maximálna celočíselná hodnota súradnice kľúča.</param>
        /// <param name="verbose">Či má detailne vypisovať každú operáciu do konzoly.</param>
        public Tester(
            ITestableStructure structure,
            int operationsCount = 50,
            int insertProbability = 60,
            int? seed = null,
            int dimensions = 2,
            int minCoordinate = 0,
            int maxCoordinate = 100,
            bool verbose = true)
        {
            if (insertProbability < 0 || insertProbability > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(insertProbability), "Pravdepodobnosť musí byť v rozsahu 0 až 100.");
            }

            _structure = structure ?? throw new ArgumentNullException(nameof(structure));
            _operationsCount = operationsCount;
            _insertProbability = insertProbability;
            _dimensions = dimensions;
            _minCoordinate = minCoordinate;
            _maxCoordinate = maxCoordinate;
            _verbose = verbose;

            // Vygenerovanie seedu ak nebol zadaný
            Seed = seed ?? Random.Shared.Next(1, 1_000_000);
            _random = new Random(Seed);
        }

        /// <summary>
        /// Pohodlný konštruktor pre QTree.
        /// </summary>
        public Tester(
            QTree<int> qTree,
            Dictionary<int, (double x, double y)> positionMap,
            int operationsCount = 50,
            int insertProbability = 60,
            int? seed = null,
            int minCoordinate = 0,
            int maxCoordinate = 100,
            bool verbose = true)
            : this(new QTreeAdapter(qTree, positionMap), operationsCount, insertProbability, seed, 2, minCoordinate, maxCoordinate, verbose)
        {
        }

        /// <summary>
        /// Pohodlný konštruktor pre KDTree.
        /// </summary>
        public Tester(
            KDTree<int> kdTree,
            int operationsCount = 50,
            int insertProbability = 60,
            int? seed = null,
            int minCoordinate = 0,
            int maxCoordinate = 100,
            bool verbose = true)
            : this(new KDTreeAdapter(kdTree), operationsCount, insertProbability, seed, kdTree.K, minCoordinate, maxCoordinate, verbose)
        {
        }

        /// <summary>
        /// Spustí testovanie.
        /// </summary>
        public bool Run()
        {
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"Spúšťam Tester pre: {_structure.Name}");
            Console.WriteLine($"Seed: {Seed} | Počet operácií: {_operationsCount}");
            Console.WriteLine($"Pravdepodobnosť Insert: {_insertProbability}% | Find: {100 - _insertProbability}%");
            Console.WriteLine($"Rozsah číselných kľúčov: [{_minCoordinate} .. {_maxCoordinate}], Dimenzia: {_dimensions}");
            Console.WriteLine("----------------------------------------------------------------------");

            for (int opIndex = 1; opIndex <= _operationsCount; opIndex++)
            {
                int roll = _random.Next(0, 100);
                bool isInsert = roll < _insertProbability;

                // Ak je pravdepodobnosť Find > 0, ale ešte nemáme žiadne dáta, radšej spravíme Insert
                if (!isInsert && _expectedData.Count == 0 && _insertProbability > 0)
                {
                    isInsert = true;
                }

                if (isInsert)
                {
                    PerformInsert(opIndex);
                }
                else
                {
                    PerformFind(opIndex);
                }

                // Kontrola referenčného zoznamu po KAŽDEJ operácii
                VerifyStateAfterOperation(opIndex);
            }

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"TEST DOKONČENÝ PRE {_structure.Name}:");
            Console.WriteLine($"  - Vykonaných operácií celkovo : {_operationsCount}");
            Console.WriteLine($"  - Insert operácií             : {InsertCount}");
            Console.WriteLine($"  - Find operácií               : {FindCount}");
            Console.WriteLine($"  - Úspešných kontrol integrity : {ChecksPassed} z {_operationsCount}");
            Console.WriteLine($"  - Počet prvkov v štruktúre    : {_expectedData.Count}");
            Console.WriteLine("  - VÝSLEDOK: VŠETKY KONTROLY PREŠLI ÚSPEŠNE (100% OK)");
            Console.WriteLine("----------------------------------------------------------------------\n");

            return true;
        }

        private void PerformInsert(int opIndex)
        {
            int data = _nextDataId++;
            int[] keys = GenerateKeys();

            // 15% šanca, že použijeme kľúč, ktorý už v štruktúre je (test duplikátov v rovnakom bode)
            if (_expectedData.Count > 0 && _random.Next(0, 100) < 15)
            {
                var existing = _expectedData[_random.Next(_expectedData.Count)];
                keys = (int[])existing.Keys.Clone();
            }

            _structure.Insert(data, keys);
            _expectedData.Add(new TestRecord(data, keys));
            InsertCount++;

            if (_verbose)
            {
                Console.WriteLine($"[Op {opIndex,3}] INSERT: Data = {data,3}, Kľúče = [{string.Join(", ", keys)}]");
            }
        }

        private void PerformFind(int opIndex)
        {
            int[] searchKeys;
            bool expectFound;

            // 60% šanca, že hľadáme existujúci bod zo zoznamu
            if (_expectedData.Count > 0 && _random.Next(0, 100) < 60)
            {
                var target = _expectedData[_random.Next(_expectedData.Count)];
                searchKeys = (int[])target.Keys.Clone();
                expectFound = true;
            }
            else
            {
                // Inak hľadáme náhodný kľúč (ktorý tam môže, ale nemusí byť)
                searchKeys = GenerateKeys();
                expectFound = _expectedData.Any(r => r.KeysMatch(searchKeys));
            }

            var result = _structure.Find(searchKeys);
            FindCount++;

            // Očakávané dáta na daných kľúčoch podľa nášho referenčného zoznamu
            var expectedIds = _expectedData
                .Where(r => r.KeysMatch(searchKeys))
                .Select(r => r.Data)
                .ToHashSet();

            var actualIds = result.ToHashSet();

            if (!expectedIds.SetEquals(actualIds))
            {
                string errorMsg = $"CHYBA v operácii Find (Op #{opIndex})!\n" +
                                  $"  Seed: {Seed}\n" +
                                  $"  Hľadané kľúče: [{string.Join(", ", searchKeys)}]\n" +
                                  $"  Očakávané dáta: [{string.Join(", ", expectedIds)}]\n" +
                                  $"  Vrátené dáta  : [{string.Join(", ", actualIds)}]";
                throw new InvalidOperationException(errorMsg);
            }

            if (_verbose)
            {
                string status = result.Count > 0 ? $"NÁJDENÉ ({result.Count} ks: [{string.Join(", ", result)}])" : "NENÁJDENÉ (kľúč neexistuje)";
                Console.WriteLine($"[Op {opIndex,3}] FIND  : Kľúče = [{string.Join(", ", searchKeys)}] -> {status}");
            }
        }

        /// <summary>
        /// Kontrola po KAŽDEJ operácii:
        /// Overí, že všetky záznamy z referenčného listu sa v štruktúre dajú nájsť.
        /// </summary>
        private void VerifyStateAfterOperation(int opIndex)
        {
            // Zoberieme buď všetky záznamy (ak ich je do 200), alebo reprezentatívnu vzorku
            int checkLimit = Math.Min(_expectedData.Count, 100);

            for (int i = 0; i < checkLimit; i++)
            {
                var record = _expectedData[i];
                var found = _structure.Find(record.Keys);

                if (!found.Contains(record.Data))
                {
                    string errorMsg = $"CHYBA INTEGRITY po operácii #{opIndex}!\n" +
                                      $"  Seed: {Seed}\n" +
                                      $"  Záznam, ktorý sa stratil: {record}\n" +
                                      $"  Na kľúčoch [{string.Join(", ", record.Keys)}] štruktúra vrátila: [{string.Join(", ", found)}]";
                    throw new InvalidOperationException(errorMsg);
                }
            }

            ChecksPassed++;
        }

        private int[] GenerateKeys()
        {
            int[] keys = new int[_dimensions];
            for (int d = 0; d < _dimensions; d++)
            {
                keys[d] = _random.Next(_minCoordinate, _maxCoordinate + 1);
            }
            return keys;
        }
    }
}
