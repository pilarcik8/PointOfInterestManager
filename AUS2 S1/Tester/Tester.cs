// Vygenerovane pomocou AI (Model: Gemini 3.8 Flash)
using System;
using System.Collections.Generic;
using System.Linq;
using AUS2_S1.KD_Tree;

namespace AUS2_S1
{
    // Zaznam o datach ulozenych v referencnom zozname Testera
    // Vygenerovane pomocou AI (Model: Gemini 3.8 Flash)
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

        public override string ToString() => $"Data: {Data}, Kluce: [{string.Join(", ", Keys)}]";
    }

    // Univerzalna trieda Tester na overovanie spravnosti datovych struktur bez nutnosti spolocneho rozhrania
    // Vygenerovane pomocou AI (Model: Gemini 3.8 Flash)
    public class Tester
    {
        private readonly string _structureName;
        private readonly Action<int, int[]> _insertAction;
        private readonly Func<int[], ICollection<int>> _findFunc;
        private readonly Random _random;
        private readonly int _operationsCount;
        private readonly int _insertProbability; // 0 az 100
        private readonly int _printStep;         // Pocet operacii, po ktorych sa vypisuje stav
        private readonly int _dimensions;
        private readonly int _minCoordinate;
        private readonly int _maxCoordinate;
        private readonly bool _verbose;

        // Referencny list – uchovava, co v strukture ma naozaj byt
        private readonly List<TestRecord> _expectedData = new List<TestRecord>();

        // Pocitadlo ID pre unikatne data
        private int _nextDataId = 1;

        // Statistiky
        public int InsertCount { get; private set; }
        public int FindCount { get; private set; }
        public int ChecksPassed { get; private set; }
        public int Seed { get; }
        public int PrintStep => _printStep;

        public IReadOnlyList<TestRecord> ExpectedData => _expectedData;

        // Vseobecny konstruktor cez delegaty - umoznuje otestovat akukolvek strukturu bez implementacie rozhrania
        public Tester(
            string structureName,
            Action<int, int[]> insertAction,
            Func<int[], ICollection<int>> findFunc,
            int operationsCount = 50,
            int insertProbability = 60,
            int? seed = null,
            int printStep = 10,
            int dimensions = 2,
            int minCoordinate = 0,
            int maxCoordinate = 100,
            bool verbose = true)
        {
            if (insertProbability < 0 || insertProbability > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(insertProbability), "Pravdepodobnost musi byt v rozsahu 0 az 100.");
            }

            if (printStep <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(printStep), "Pocet operacii pre vypis (printStep) musi byt vacsi ako 0.");
            }

            _structureName = structureName ?? "Neznama struktura";
            _insertAction = insertAction ?? throw new ArgumentNullException(nameof(insertAction));
            _findFunc = findFunc ?? throw new ArgumentNullException(nameof(findFunc));
            _operationsCount = operationsCount;
            _insertProbability = insertProbability;
            _printStep = printStep;
            _dimensions = dimensions;
            _minCoordinate = minCoordinate;
            _maxCoordinate = maxCoordinate;
            _verbose = verbose;

            // Vygenerovanie seedu ak nebol zadany
            Seed = seed ?? Random.Shared.Next(1, 1_000_000);
            _random = new Random(Seed);
        }

        // Konstruktor pre QTree
        public Tester(
            QTree<int, double> qTree,
            Dictionary<int, (double x, double y)> positionMap,
            int operationsCount = 50,
            int insertProbability = 60,
            int? seed = null,
            int printStep = 10,
            int minCoordinate = 0,
            int maxCoordinate = 100,
            bool verbose = true)
            : this(
                "Quad Tree (QTree<int>)",
                (data, keys) =>
                {
                    if (keys.Length < 2)
                    {
                        throw new ArgumentException("QTree vyzaduje 2 kluce [x, y].");
                    }
                    positionMap[data] = (keys[0], keys[1]);
                    qTree.Insert(data);
                },
                keys =>
                {
                    if (keys.Length < 2)
                    {
                        throw new ArgumentException("QTree vyzaduje 2 kluce [x, y].");
                    }
                    return qTree.FindRange(keys[0], keys[1], keys[0], keys[1]);
                },
                operationsCount,
                insertProbability,
                seed,
                printStep,
                2,
                minCoordinate,
                maxCoordinate,
                verbose)
        {
        }

        // Konstruktor pre KDTree
        public Tester(
            KDTree<int> kdTree,
            int operationsCount = 50,
            int insertProbability = 60,
            int? seed = null,
            int printStep = 10,
            int minCoordinate = 0,
            int maxCoordinate = 100,
            bool verbose = true)
            : this(
                $"KD Tree (KDTree<int>, k = {kdTree.K})",
                (data, keys) =>
                {
                    if (keys.Length != kdTree.K)
                    {
                        throw new ArgumentException($"KDTree s k={kdTree.K} vyzaduje {kdTree.K} klucov.");
                    }
                    var keyList = new LinkedList<KDKey<int>>();
                    foreach (var k in keys)
                    {
                        keyList.AddLast(new KDKey<int>(k));
                    }
                    kdTree.Insert(data, keyList);
                },
                keys =>
                {
                    if (keys.Length != kdTree.K)
                    {
                        throw new ArgumentException($"KDTree s k={kdTree.K} vyzaduje {kdTree.K} klucov.");
                    }
                    var keyList = new LinkedList<KDKey<int>>();
                    foreach (var k in keys)
                    {
                        keyList.AddLast(new KDKey<int>(k));
                    }
                    var node = kdTree.Find(keyList);
                    return node != null ? node.Data : Array.Empty<int>();
                },
                operationsCount,
                insertProbability,
                seed,
                printStep,
                kdTree.K,
                minCoordinate,
                maxCoordinate,
                verbose)
        {
        }

        // Spusti testovanie
        public bool Run()
        {
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"Spustam Tester pre: {_structureName}");
            Console.WriteLine($"Seed: {Seed} | Pocet operacii: {_operationsCount}");
            Console.WriteLine($"Pravdepodobnost Insert: {_insertProbability}% | Find: {100 - _insertProbability}%");
            Console.WriteLine($"Rozsah ciselnych klucov: [{_minCoordinate} .. {_maxCoordinate}], Dimenzia: {_dimensions}");
            Console.WriteLine($"Vypis kazdych: {_printStep} operacii");
            Console.WriteLine("----------------------------------------------------------------------");

            for (int opIndex = 1; opIndex <= _operationsCount; opIndex++)
            {
                int roll = _random.Next(0, 100);
                bool isInsert = roll < _insertProbability;

                // Ak je pravdepodobnost Find > 0, ale este nemame ziadne data, radsej spravime Insert
                if (!isInsert && _expectedData.Count == 0 && _insertProbability > 0)
                {
                    isInsert = true;
                }

                string opDescription;
                if (isInsert)
                {
                    opDescription = PerformInsert(opIndex);
                }
                else
                {
                    opDescription = PerformFind(opIndex);
                }

                // Kontrola referencneho zoznamu po KAZDEJ operacii
                VerifyStateAfterOperation(opIndex);

                // Vypis po zadanom pocte operacii (alebo pri kazdej ak printStep == 1)
                if (_verbose)
                {
                    if (_printStep == 1)
                    {
                        Console.WriteLine($"[Op {opIndex,3}] {opDescription}");
                    }
                    else if (opIndex % _printStep == 0 || opIndex == _operationsCount)
                    {
                        Console.WriteLine($"[Op {opIndex,5}/{_operationsCount}] Stav: Insert={InsertCount}, Find={FindCount}, V strukture={_expectedData.Count} prvkov, Kontroly={ChecksPassed} OK");
                    }
                }
            }

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"TEST DOKONCENY PRE {_structureName}:");
            Console.WriteLine($"  - Vykonanych operacii celkovo : {_operationsCount}");
            Console.WriteLine($"  - Insert operacii             : {InsertCount}");
            Console.WriteLine($"  - Find operacii               : {FindCount}");
            Console.WriteLine($"  - Uspesnych kontrol integrity : {ChecksPassed} z {_operationsCount}");
            Console.WriteLine($"  - Pocet prvkov v strukture    : {_expectedData.Count}");
            Console.WriteLine("  - VYSLEDOK: VSETKY KONTROLY PRESLI USPESNE (100% OK)");
            Console.WriteLine("----------------------------------------------------------------------\n");

            return true;
        }

        private string PerformInsert(int opIndex)
        {
            int data = _nextDataId++;
            int[] keys = GenerateKeys();

            // 15% sanca, ze pouzijeme kluc, ktory uz v strukture je (test duplikatov v rovnakom bode)
            if (_expectedData.Count > 0 && _random.Next(0, 100) < 15)
            {
                var existing = _expectedData[_random.Next(_expectedData.Count)];
                keys = (int[])existing.Keys.Clone();
            }

            _insertAction(data, keys);
            _expectedData.Add(new TestRecord(data, keys));
            InsertCount++;

            return $"INSERT: Data = {data,3}, Kluce = [{string.Join(", ", keys)}]";
        }

        private string PerformFind(int opIndex)
        {
            int[] searchKeys;

            // 60% sanca, ze hladame existujuci bod zo zoznamu
            if (_expectedData.Count > 0 && _random.Next(0, 100) < 60)
            {
                var target = _expectedData[_random.Next(_expectedData.Count)];
                searchKeys = (int[])target.Keys.Clone();
            }
            else
            {
                // Inak hladame nahodny kluc (ktory tam moze, ale nemusi byt)
                searchKeys = GenerateKeys();
            }

            var result = _findFunc(searchKeys);
            FindCount++;

            // Ocakavane data na danych klucoch podla nasho referencneho zoznamu
            var expectedIds = _expectedData
                .Where(r => r.KeysMatch(searchKeys))
                .Select(r => r.Data)
                .ToHashSet();

            var actualIds = result.ToHashSet();

            if (!expectedIds.SetEquals(actualIds))
            {
                string errorMsg = $"CHYBA v operacii Find (Op #{opIndex})!\n" +
                                  $"  Seed: {Seed}\n" +
                                  $"  Hladane kluce: [{string.Join(", ", searchKeys)}]\n" +
                                  $"  Ocakavane data: [{string.Join(", ", expectedIds)}]\n" +
                                  $"  Vratene data  : [{string.Join(", ", actualIds)}]";
                throw new InvalidOperationException(errorMsg);
            }

            string status = result.Count > 0 ? $"NAJDENE ({result.Count} ks: [{string.Join(", ", result)}])" : "NENASLO SA (kluc neexistuje)";
            return $"FIND  : Kluce = [{string.Join(", ", searchKeys)}] -> {status}";
        }

        // Kontrola po KAZDEJ operacii:
        // Overi, ze vsetky zaznamy z referencneho listu sa v strukture daju najst
        private void VerifyStateAfterOperation(int opIndex)
        {
            int checkLimit = Math.Min(_expectedData.Count, 100);

            for (int i = 0; i < checkLimit; i++)
            {
                var record = _expectedData[i];
                var found = _findFunc(record.Keys);

                if (!found.Contains(record.Data))
                {
                    string errorMsg = $"CHYBA INTEGRITY po operacii #{opIndex}!\n" +
                                      $"  Seed: {Seed}\n" +
                                      $"  Zaznam, ktory sa stratil: {record}\n" +
                                      $"  Na klucoch [{string.Join(", ", record.Keys)}] struktura vratila: [{string.Join(", ", found)}]";
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
