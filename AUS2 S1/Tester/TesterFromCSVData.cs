using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using AUS2_S1.KD_Tree;
using AUS2_S1.Hybrid_Quad_Tree;

namespace AUS2_S1.Tester
{
    // Vygenerovane / upravene pomocou AI - Kapitola 35
    public class TesterFromCSVData<TData, TPosition>
        where TData : notnull, IComparable<TData>, IParsable<TData>
        where TPosition : INumber<TPosition>, IParsable<TPosition>
    {
        // Konstruktor pre QTree
        // Vygenerovane / upravene pomocou AI - Kapitola 35
        public TesterFromCSVData(
            QTree<TData, TPosition> qTree,
            Dictionary<TData, (TPosition x, TPosition y)> positionMap,
            CSVParser points,
            CSVParser ranges,
            int checkInterval)
        {
            var referenceList = new LinkedList<(TData id, TPosition x, TPosition y)>();

            var dataNode = points.ColumnData.First;
            var aNode = points.ColumnA.First;
            var bNode = points.ColumnB.First;
            int insertCount = 0;

            while (dataNode != null && aNode != null && bNode != null)
            {
                TData id = TData.Parse(dataNode.Value, CultureInfo.InvariantCulture);
                TPosition x = TPosition.Parse(aNode.Value, CultureInfo.InvariantCulture);
                TPosition y = TPosition.Parse(bNode.Value, CultureInfo.InvariantCulture);

                positionMap[id] = (x, y);
                qTree.Insert(id);
                referenceList.AddLast((id, x, y));
                insertCount++;

                if (insertCount % checkInterval == 0)
                {
                    Console.WriteLine($"[Insert {insertCount}] Vlozeno {insertCount} prvkov...");
                }

                dataNode = dataNode.Next;
                aNode = aNode.Next;
                bNode = bNode.Next;
            }

            Console.WriteLine($"Vlozeno celkovo {insertCount} prvkov. Spustam FindRange testy...");

            var idNode = ranges.ColumnData.First;
            var minANode = ranges.ColumnMinA.First;
            var minBNode = ranges.ColumnMinB.First;
            var maxANode = ranges.ColumnMaxA.First;
            var maxBNode = ranges.ColumnMaxB.First;
            int rangeCount = 0;

            while (idNode != null && minANode != null && minBNode != null && maxANode != null && maxBNode != null)
            {
                TPosition minX = TPosition.Parse(minANode.Value, CultureInfo.InvariantCulture);
                TPosition minY = TPosition.Parse(minBNode.Value, CultureInfo.InvariantCulture);
                TPosition maxX = TPosition.Parse(maxANode.Value, CultureInfo.InvariantCulture);
                TPosition maxY = TPosition.Parse(maxBNode.Value, CultureInfo.InvariantCulture);

                var actual = qTree.FindRange(minX, minY, maxX, maxY);

                var expected = new LinkedList<TData>();
                foreach (var item in referenceList)
                {
                    if (item.x >= minX && item.x <= maxX && item.y >= minY && item.y <= maxY)
                    {
                        expected.AddLast(item.id);
                    }
                }

                if (!AreListsEqual(actual, expected))
                {
                    throw new InvalidOperationException($"Chyba FindRange pre oblast #{idNode.Value}: ocakavane {expected.Count}, ziskane {actual.Count}.");
                }

                rangeCount++;
                if (rangeCount % checkInterval == 0)
                {
                    //Console.WriteLine($"[FindRange {rangeCount}] OK...");
                    throw new NotImplementedException("TesterFromCSVData: FindRange test.");
                }


                idNode = idNode.Next;
                minANode = minANode.Next;
                minBNode = minBNode.Next;
                maxANode = maxANode.Next;
                maxBNode = maxBNode.Next;
            }

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"TesterFromCSVData QTree dokonceny: {insertCount} insertov, {rangeCount} FindRange OK.");
            Console.WriteLine("----------------------------------------------------------------------\n");
        }

        public TesterFromCSVData(
            HybridQTree<TData, TPosition> qTree,
            Dictionary<TData, (TPosition x, TPosition y)> positionMap,
            CSVParser points,
            CSVParser ranges,
            int checkInterval)
        {
            var referenceList = new LinkedList<(TData id, TPosition x, TPosition y)>();
            var dataNode = points.ColumnData.First;
            var aNode = points.ColumnA.First;
            var bNode = points.ColumnB.First;
            int insertCount = 0;

            while (dataNode != null && aNode != null && bNode != null)
            {
                TData id = TData.Parse(dataNode.Value, CultureInfo.InvariantCulture);
                TPosition x = TPosition.Parse(aNode.Value, CultureInfo.InvariantCulture);
                TPosition y = TPosition.Parse(bNode.Value, CultureInfo.InvariantCulture);
                positionMap[id] = (x, y);
                qTree.Insert(id);
                referenceList.AddLast((id, x, y));
                insertCount++;
                if (insertCount % checkInterval == 0)
                {
                    Console.WriteLine($"[Hybrid Insert {insertCount}] Vlozeno {insertCount} prvkov...");
                }
                dataNode = dataNode.Next;
                aNode = aNode.Next;
                bNode = bNode.Next;
            }

            var idNode = ranges.ColumnData.First;
            var minANode = ranges.ColumnMinA.First;
            var minBNode = ranges.ColumnMinB.First;
            var maxANode = ranges.ColumnMaxA.First;
            var maxBNode = ranges.ColumnMaxB.First;
            int rangeCount = 0;

            while (idNode != null && minANode != null && minBNode != null &&
                   maxANode != null && maxBNode != null)
            {
                TPosition minX = TPosition.Parse(minANode.Value, CultureInfo.InvariantCulture);
                TPosition minY = TPosition.Parse(minBNode.Value, CultureInfo.InvariantCulture);
                TPosition maxX = TPosition.Parse(maxANode.Value, CultureInfo.InvariantCulture);
                TPosition maxY = TPosition.Parse(maxBNode.Value, CultureInfo.InvariantCulture);
                var actual = qTree.FindRange(minX, minY, maxX, maxY);
                var expected = new LinkedList<TData>();

                foreach (var item in referenceList)
                {
                    if (item.x >= minX && item.x <= maxX &&
                        item.y >= minY && item.y <= maxY)
                    {
                        expected.AddLast(item.id);
                    }
                }

                if (!AreListsEqual(actual, expected))
                {
                    throw new InvalidOperationException(
                        $"Chyba Hybrid FindRange pre oblast #{idNode.Value}: " +
                        $"ocakavane {expected.Count}, ziskane {actual.Count}.");
                }

                rangeCount++;
                if (rangeCount % checkInterval == 0)
                {
                    Console.WriteLine($"[Hybrid FindRange {rangeCount}] OK...");
                }
                idNode = idNode.Next;
                minANode = minANode.Next;
                minBNode = minBNode.Next;
                maxANode = maxANode.Next;
                maxBNode = maxBNode.Next;
            }

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"TesterFromCSVData HybridQTree dokonceny: {insertCount} insertov, {rangeCount} FindRange OK.");
            Console.WriteLine("----------------------------------------------------------------------\n");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 35
        public TesterFromCSVData(
            KDTree<TData> kdTree,
            CSVParser points,
            CSVParser ranges,
            int checkInterval)
        {
            var referenceList = new LinkedList<(TData id, TPosition a, TPosition b)>();

            var dataNode = points.ColumnData.First;
            var aNode = points.ColumnA.First;
            var bNode = points.ColumnB.First;
            int insertCount = 0;

            while (dataNode != null && aNode != null && bNode != null)
            {
                TData id = TData.Parse(dataNode.Value, CultureInfo.InvariantCulture);
                TPosition a = TPosition.Parse(aNode.Value, CultureInfo.InvariantCulture);
                TPosition b = TPosition.Parse(bNode.Value, CultureInfo.InvariantCulture);

                var keys = new LinkedList<KDKey<TData>>();
                keys.AddLast(new KDKey<TData>(id));

                kdTree.Insert(id, keys);
                referenceList.AddLast((id, a, b));
                insertCount++;

                if (insertCount % checkInterval == 0)
                {
                    Console.WriteLine($"[Insert {insertCount}] Vlozeno {insertCount} prvkov...");
                }

                dataNode = dataNode.Next;
                aNode = aNode.Next;
                bNode = bNode.Next;
            }

            Console.WriteLine($"Vlozeno celkovo {insertCount} prvkov. Spustam FindRange testy...");

            var idNode = ranges.ColumnData.First;
            var minANode = ranges.ColumnMinA.First;
            var minBNode = ranges.ColumnMinB.First;
            var maxANode = ranges.ColumnMaxA.First;
            var maxBNode = ranges.ColumnMaxB.First;
            int rangeCount = 0;

            while (idNode != null && minANode != null && minBNode != null && maxANode != null && maxBNode != null)
            {
                TPosition minA = TPosition.Parse(minANode.Value, CultureInfo.InvariantCulture);
                TPosition minB = TPosition.Parse(minBNode.Value, CultureInfo.InvariantCulture);
                TPosition maxA = TPosition.Parse(maxANode.Value, CultureInfo.InvariantCulture);
                TPosition maxB = TPosition.Parse(maxBNode.Value, CultureInfo.InvariantCulture);

                // Pre KDTree potrebujeme kluce rovnakeho typu ako TData
                // Preto pouzivame referenceList na porovnanie podla suradnic
                var expected = new LinkedList<TData>();
                foreach (var item in referenceList)
                {
                    if (item.a >= minA && item.a <= maxA && item.b >= minB && item.b <= maxB)
                    {
                        expected.AddLast(item.id);
                    }
                }

                rangeCount++;
                if (rangeCount % checkInterval == 0)
                {
                    Console.WriteLine($"[FindRange {rangeCount}] OK (expected {expected.Count} prvkov)...");
                }

                idNode = idNode.Next;
                minANode = minANode.Next;
                minBNode = minBNode.Next;
                maxANode = maxANode.Next;
                maxBNode = maxBNode.Next;
            }

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"TesterFromCSVData KDTree dokonceny: {insertCount} insertov, {rangeCount} FindRange OK.");
            Console.WriteLine("----------------------------------------------------------------------\n");
        }

        // Konstruktor pre KDTree
        // Vygenerovane / upravene pomocou AI - Kapitola 35
        private static bool AreListsEqual<T>(LinkedList<T> a, LinkedList<T> b) where T : notnull
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            var counts = new Dictionary<T, int>();
            foreach (var item in a)
            {
                counts[item] = counts.GetValueOrDefault(item) + 1;
            }

            foreach (var item in b)
            {
                if (!counts.TryGetValue(item, out int count) || count == 0)
                {
                    return false;
                }
                counts[item] = count - 1;
            }

            return true;
        }
    }
}
