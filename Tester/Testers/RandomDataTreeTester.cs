using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures.QTree;
using DataStructures.KD_Tree;
using DataStructures.Hybrid_Quad_Tree;

namespace DataStructTester.Testers
{
    public class RandomDataTreeTester
    {
        private  int _operationsCount;
        private  int _insertProbability;
        private  int _seed;
        private  int _checkInterval;
        private  int _maxDepth;

        // Vygenerovane / upravene pomocou AI - Kapitola 27 (vytvorenie konštruktora), Kapitola 33 (zmena na QTree<int, double>)
        public void TestQTreeTester(
            int operationsCount,
            int insertProbability,
            int seed,
            int maxDepht,
            int checkInterval = 10
            )
        {
            _operationsCount = operationsCount;
            _insertProbability = insertProbability;
            _seed = seed;
            _maxDepth = maxDepht;
            _checkInterval = checkInterval;
            RunQTreeTest();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 27
        public void TestKDTreeTester(
            int operationsCount,
            int insertProbability,
            int seed,
            int checkInterval = 10)
        {
            _operationsCount = operationsCount;
            _insertProbability = insertProbability;
            _seed = seed;
            _checkInterval = checkInterval;
            RunKDTreeTest();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 73, 74
        public void TestHybridTreeTester(
            int operationsCount,
            int insertProbability,
            int seed,
            int maxDepht,
            int checkInterval = 10)
        {
            _operationsCount = operationsCount;
            _insertProbability = insertProbability;
            _seed = seed;
            _maxDepth = maxDepht;
            _checkInterval = checkInterval;
            RunHybridTreeTest();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 34
        private void RunQTreeTest()
        {
            var random = new Random(_seed);
            var referenceList = new List<QTreeInfo>();
            int nextId = 1;

            // rezerva 1 aby max != min
            int randMaxX = random.Next(1, int.MaxValue);
            int randMaxY = random.Next(1, int.MaxValue);
            int randMinX = randMaxX - random.Next(0, randMaxX);
            int randMinY = randMaxY - random.Next(0, randMaxY);

            var _qTree = new QTree<QTreeInfo, double>(minX: randMinX, minY: randMinY, maxX: randMaxX, maxY: randMaxY, 
                                                    getPosition: item => (item.X, item.Y), maxDepth: _maxDepth);

            for (int operationNumber = 1; operationNumber <= _operationsCount; operationNumber++)
            {
                bool doInsert = random.Next(100) < _insertProbability || referenceList.Count == 0;
                int maxLength = random.Next(1, int.MaxValue);

                //insert
                if (doInsert)
                {
                    int data = nextId++;

                    // ocakavame ze niektore hodnoty budu mimo
                    int boundaryOffsetX = random.Next(0, randMaxX / 1);
                    int boundaryOffsetY = random.Next(0, randMaxY / 1);

                    int xUpperBound = (int)Math.Min((long)int.MaxValue, (long)randMaxX + boundaryOffsetX);
                    int yUpperBound = (int)Math.Min((long)int.MaxValue, (long)randMaxY + boundaryOffsetY);
                    double x = random.Next(0, xUpperBound);
                    double y = random.Next(0, yUpperBound);

                    var item = new QTreeInfo(data, x, y);

                    _qTree!.Insert(item);
                    referenceList.Add(item);

                    // kontrola
                    if (operationNumber % _checkInterval == 0 || operationNumber == _operationsCount)
                    {
                        foreach (var refItem in referenceList)
                        {
                            var found = _qTree!.FindRange(refItem.X, refItem.Y, refItem.X, refItem.Y);
                            var foundData = found.Select(f => f.Data).ToList();

                            if (!foundData.Contains(refItem.Data))
                            {
                                throw new InvalidOperationException($"Chyba integrity po operacii #{operationNumber}: prvok {refItem.Data} na [{refItem.X}, {refItem.Y}] sa nenasiel.");
                            }
                        }

                        Console.WriteLine($"[Op {operationNumber,4}/{_operationsCount}] Kontrola integrity OK");
                    }
                    continue;
                }

                // range 
                double minX = random.Next(0, maxLength);
                double maxX = random.Next((int)Math.Round(minX) + 1, maxLength);
                double minY = random.Next(0, maxLength);
                double maxY = random.Next((int)Math.Round(minY) + 1, maxLength);

                var actual = _qTree!.FindRange(minX, minY, maxX, maxY);
                var actualData = actual.Select(a => a.Data).ToList();

                List<int> expected = referenceList
                    .Where(item => item.X >= minX && item.X <= maxX && item.Y >= minY && item.Y <= maxY)
                    .Select(item => item.Data).ToList();

                foreach (var item in actualData)
                {
                    if (!expected.Contains(item))
                    {
                        throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre QTree: ocakavany prvok {item} sa nenasiel.");
                    }
                    expected.Remove(item); // vymaze iba prvy najdeny (duplicity mozu byt)
                }
                if (expected.Count != 0)
                {
                    throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre QTree: strom nevrátil {expected.Count} ocakavanych prvkov. Pocet prvkov {actualData.Count}");
                }

                if (operationNumber % _checkInterval == 0 || operationNumber == _operationsCount)
                {
                    Console.WriteLine($"[Op {operationNumber,4}/{_operationsCount}] Kontrola integrity OK");
                }

            }
            Console.WriteLine("--------------------------------------------------");
        }

        private readonly struct QTreeInfo
        {
            public QTreeInfo(int data, double x, double y)
            {
                Data = data;
                X = x;
                Y = y;
            }

            public int Data { get; }
            public double X { get; }
            public double Y { get; }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 34
        private void RunKDTreeTest()
        {
            var random = new Random(_seed);
            var referenceList = new List<(int data, int[] keys)>();
            int nextId = 1;

            var _kdTree = new KDTree<int, int>(2);

            for (int operationNumber = 1; operationNumber <= _operationsCount; operationNumber++)
            {
                bool doInsert = random.Next(100) < _insertProbability || referenceList.Count == 0;
                int maxLength = random.Next(1, int.MaxValue);

                if (doInsert)
                {
                    int data = nextId++;
                    int[] keys = new int[_kdTree!.K];
                    for (int d = 0; d < _kdTree.K; d++)
                    {
                        keys[d] = random.Next(0, maxLength);
                    }

                    var keyList = new LinkedList<KDKey<int>>();
                    foreach (var k in keys)
                    {
                        keyList.AddLast(new KDKey<int>(k));
                    }

                    _kdTree.Insert(data, keyList);
                    referenceList.Add((data, keys));
                }
                else
                {
                    int[] minKeys = new int[_kdTree!.K];
                    int[] maxKeys = new int[_kdTree.K];
                    var minKeyList = new LinkedList<KDKey<int>>();
                    var maxKeyList = new LinkedList<KDKey<int>>();

                    for (int d = 0; d < _kdTree.K; d++)
                    {
                        minKeys[d] = random.Next(0, maxLength);
                        maxKeys[d] = random.Next(minKeys[d] + 1, maxLength);
                        minKeyList.AddLast(new KDKey<int>(minKeys[d]));
                        maxKeyList.AddLast(new KDKey<int>(maxKeys[d]));
                    }

                    var foundNodes = _kdTree.FindRange(minKeyList, maxKeyList);
                    var actual = foundNodes.SelectMany(node => node.Data).ToList();

                    var expected = referenceList
                        .Where(item =>
                        {
                            for (int d = 0; d < _kdTree.K; d++)
                            {
                                if (item.keys[d] < minKeys[d] || item.keys[d] > maxKeys[d])
                                {
                                    return false;
                                }
                            }
                            return true;
                        })
                        .Select(item => item.data)
                        .ToList();

                    foreach (var item in actual)
                    {
                        if (!expected.Contains(item))
                        {
                            throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre KDTree: neočakávaný prvok {item}.");
                        }
                        expected.Remove(item); // vymaze iba prvy najdeny vyskyt (podpora duplicit)
                    }

                    if (expected.Count != 0)
                    {
                        throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre KDTree: strom nevrátil {expected.Count} očakávaných prvkov.");
                    }
                }

                // Kontrola integrity iba kazdych checkInterval operacii alebo na konci
                if (operationNumber % _checkInterval == 0 || operationNumber == _operationsCount)
                {
                    foreach (var (data, keys) in referenceList)
                    {
                        var keyList = new LinkedList<KDKey<int>>();
                        foreach (var k in keys)
                        {
                            keyList.AddLast(new KDKey<int>(k));
                        }

                        var node = _kdTree!.Find(keyList);
                        if (node == null || !node.Data.Contains(data))
                        {
                            throw new InvalidOperationException($"Chyba integrity po operacii #{operationNumber}: prvok {data} sa v KDTree nenasiel.");
                        }
                    }
                    Console.WriteLine($"[Op {operationNumber,4}/{_operationsCount}] Kontrola integrity OK (prvkov v strukture: {referenceList.Count})");
                }
            }
            Console.WriteLine("--------------------------------------------------");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 72, 73
        private void RunHybridTreeTest()
        {
            var random = new Random(_seed);
            var referenceList = new List<QTreeInfo>();
            int nextId = 1;

            // rezerva 1 aby max != min
            var randMaxX = random.Next(1, int.MaxValue);
            var randMaxY = random.Next(1, int.MaxValue);
            var randMinX = randMaxX - random.Next(0, randMaxX);
            var randMinY = randMaxY - random.Next(0, randMaxY);

            var _hybridQTree = new HybridQTree<QTreeInfo, double>(
                minX: randMinX, minY: randMinY, maxX: randMaxX, maxY: randMaxY,
                getPosition: item => (item.X, item.Y), maxDepth: _maxDepth);

            for (int operationNumber = 1; operationNumber <= _operationsCount; operationNumber++)
            {
                bool doInsert = random.Next(100) < _insertProbability || referenceList.Count == 0;
                int maxLength = random.Next(1, int.MaxValue);

                // insert
                if (doInsert)
                {
                    int data = nextId++;

                    // ocakavame ze niektore hodnoty budu mimo
                    int boundaryOffsetX = random.Next(0, randMaxX / 1);
                    int boundaryOffsetY = random.Next(0, randMaxY / 1);

                    int xUpperBound = (int)Math.Min((long)int.MaxValue, (long)randMaxX + boundaryOffsetX);
                    int yUpperBound = (int)Math.Min((long)int.MaxValue, (long)randMaxY + boundaryOffsetY);
                    double x = random.Next(0, xUpperBound);
                    double y = random.Next(0, yUpperBound);

                    var item = new QTreeInfo(data, x, y);

                    _hybridQTree.Insert(item);
                    referenceList.Add(item);

                    // kontrola integrity
                    if (operationNumber % _checkInterval == 0 || operationNumber == _operationsCount)
                    {
                        foreach (var refItem in referenceList)
                        {
                            var found = _hybridQTree.FindRange(refItem.X, refItem.Y, refItem.X, refItem.Y);
                            var foundData = found.Select(f => f.Data).ToList();

                            if (!foundData.Contains(refItem.Data))
                            {
                                throw new InvalidOperationException($"Chyba integrity po operacii #{operationNumber}: prvok {refItem.Data} na [{refItem.X}, {refItem.Y}] sa v HybridQTree nenasiel.");
                            }
                        }

                        Console.WriteLine($"[Op {operationNumber,4}/{_operationsCount}] Kontrola integrity OK (prvkov v strukture: {referenceList.Count})");
                    }
                    continue;
                }

                // range query
                double minX = random.Next(0, maxLength);
                double maxX = random.Next((int)Math.Round(minX) + 1, maxLength);
                double minY = random.Next(0, maxLength);
                double maxY = random.Next((int)Math.Round(minY) + 1, maxLength);

                var actual = _hybridQTree.FindRange(minX, minY, maxX, maxY);
                var actualData = actual.Select(a => a.Data).ToList();

                List<int> expected = referenceList
                    .Where(item => item.X >= minX && item.X <= maxX && item.Y >= minY && item.Y <= maxY)
                    .Select(item => item.Data).ToList();

                foreach (var item in actualData)
                {
                    if (!expected.Contains(item))
                    {
                        throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre HybridQTree: neočakávaný prvok {item}.");
                    }
                    expected.Remove(item); // vymaze iba prvy najdeny vyskyt (podpora duplicit)
                }

                if (expected.Count != 0)
                {
                    throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre HybridQTree: strom nevrátil {expected.Count} očakávaných prvkov.");
                }

                if (operationNumber % _checkInterval == 0 || operationNumber == _operationsCount)
                {
                    Console.WriteLine($"[Op {operationNumber,4}/{_operationsCount}] Kontrola integrity OK");
                }
            }
            Console.WriteLine("--------------------------------------------------");
        }
    }
}
