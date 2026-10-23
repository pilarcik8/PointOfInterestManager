using System;
using System.Collections.Generic;
using System.Linq;
using AUS2_S1.KD_Tree;

namespace AUS2_S1.Tester
{
    public class RandomDataTreeTester
    {
        private readonly QTree<int, double>? _qTree;
        private readonly KDTree<int>? _kdTree;

        private readonly int _operationsCount;
        private readonly int _insertProbability;
        private readonly int _seed;
        private readonly int _checkInterval;

        // Konstruktor pre QTree
        // Vygenerovane / upravene pomocou AI - Kapitola 27 (vytvorenie konštruktora), Kapitola 33 (zmena na QTree<int, double>)
        public RandomDataTreeTester(
            QTree<int, double> qTree,
            int operationsCount,
            int insertProbability,
            int seed,
            int checkInterval = 10)
        {
            _qTree = qTree;
            _operationsCount = operationsCount;
            _insertProbability = insertProbability;
            _seed = seed;
            _checkInterval = checkInterval;
            RunQTreeTest();
        }
    
        // Konstruktor pre KDTree
        // Vygenerovane / upravene pomocou AI - Kapitola 27
        public RandomDataTreeTester(
            KDTree<int> kdTree,
            int operationsCount,
            int insertProbability,
            int seed,
            int checkInterval = 10)
        {
            _kdTree = kdTree;
            _operationsCount = operationsCount;
            _insertProbability = insertProbability;
            _seed = seed;
            _checkInterval = checkInterval;
            RunKDTreeTest();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 34
        private void RunQTreeTest()
        {
            var random = new Random(_seed);
            var referenceList = new List<(int data, double x, double y)>();
            int nextId = 1;

            for (int operationNumber = 1; operationNumber <= _operationsCount; operationNumber++)
            {
                bool doInsert = random.Next(100) < _insertProbability || referenceList.Count == 0;
                int maxLength = random.Next(1, int.MaxValue);

                if (doInsert)
                {
                    int data = nextId++;
                    double x = random.Next(0, maxLength);
                    double y = random.Next(0, maxLength);

                    _qTree!.Insert(data);
                    referenceList.Add((data, x, y));
                }
                else
                {
                    double minX = random.Next(0, maxLength);
                    double maxX = random.Next((int)Math.Round(minX) + 1, maxLength);
                    double minY = random.Next(0, maxLength);
                    double maxY = random.Next((int)Math.Round(minY) + 1, maxLength);

                    var found = _qTree!.FindRange(minX, minY, maxX, maxY);
                    var expected = referenceList
                        .Where(item => item.x >= minX && item.x <= maxX && item.y >= minY && item.y <= maxY)
                        .Select(item => item.data)
                        .ToHashSet();

                    var actual = found.ToHashSet();
                    if (!expected.SetEquals(actual))
                    {
                        throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre rozsah [{minX}, {minY}] - [{maxX}, {maxY}].");
                    }
                }

                // Kontrola integrity iba kazdych checkInterval operacii alebo na konci
                if (operationNumber % _checkInterval == 0 || operationNumber == _operationsCount)
                {
                    foreach (var item in referenceList)
                    {
                        var found = _qTree!.FindRange(item.x, item.y, item.x, item.y);
                        if (!found.Contains(item.data))
                        {
                            throw new InvalidOperationException($"Chyba integrity po operacii #{operationNumber}: prvok {item.data} na [{item.x}, {item.y}] sa nenasiel.");
                        }
                    }

                    Console.WriteLine($"[Op {operationNumber,4}/{_operationsCount}] Kontrola integrity OK (prvkov v strukture: {referenceList.Count})");
                }
            }
            Console.WriteLine("--------------------------------------------------");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 34
        private void RunKDTreeTest()
        {
            var random = new Random(_seed);
            var referenceList = new List<(int data, int[] keys)>();
            int nextId = 1;

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
                    var actual = foundNodes.SelectMany(node => node.Data).ToHashSet();

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
                        .ToHashSet();

                    if (!expected.SetEquals(actual))
                    {
                        throw new InvalidOperationException($"Chyba FindRange v operacii #{operationNumber} pre KDTree.");
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
    }
}
