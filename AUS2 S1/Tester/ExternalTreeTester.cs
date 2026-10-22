using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using FastG;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Index.Quadtree;
using SpatialTrees.Quadtrees;
using SpatialQuadtree = SpatialTrees.Quadtrees.Quadtree;

namespace AUS2_S1.Tester
{
    internal sealed class ExternalPoint : IMapObject2d
    {
        public long Id { get; }
        public float X { get; }
        public float Y { get; }
        public int ObjectType { get; set; } = 1;
        public Point2 Location { get; set; }
        public AARectangle BoundingBox => new(Y, X, 0f, 0f);

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        public ExternalPoint(long id, double x, double y)
        {
            Id = id;
            X = (float)x;
            Y = (float)y;
            Location = new Point2(Y, X);
        }
    }

    public static class ExternalTreeTester
    {
        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        public static void Run(
            CSVParser points,
            CSVParser ranges,
            int maxPoints = int.MaxValue,
            int maxRanges = int.MaxValue)
        {
            var data = LoadPoints(points, maxPoints);
            var areas = LoadRanges(ranges, maxRanges);

            TestNetTopologyKdTree(data, areas);
            TestNetTopologyQuadTree(data, areas);
            TestSpatialTreesQuadTree(data, areas);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static List<ExternalPoint> LoadPoints(CSVParser points, int maxPoints)
        {
            var result = new List<ExternalPoint>();
            var id = points.ColumnData.First;
            var x = points.ColumnA.First;
            var y = points.ColumnB.First;

            while (id != null && x != null && y != null && result.Count < maxPoints)
            {
                result.Add(new ExternalPoint(
                    long.Parse(id.Value, CultureInfo.InvariantCulture),
                    double.Parse(x.Value, CultureInfo.InvariantCulture),
                    double.Parse(y.Value, CultureInfo.InvariantCulture)));
                id = id.Next;
                x = x.Next;
                y = y.Next;
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static List<(double minX, double minY, double maxX, double maxY)> LoadRanges(
            CSVParser ranges,
            int maxRanges)
        {
            var result = new List<(double, double, double, double)>();
            var minX = ranges.ColumnMinA.First;
            var minY = ranges.ColumnMinB.First;
            var maxX = ranges.ColumnMaxA.First;
            var maxY = ranges.ColumnMaxB.First;

            while (minX != null && minY != null && maxX != null && maxY != null &&
                   result.Count < maxRanges)
            {
                result.Add((
                    double.Parse(minX.Value, CultureInfo.InvariantCulture),
                    double.Parse(minY.Value, CultureInfo.InvariantCulture),
                    double.Parse(maxX.Value, CultureInfo.InvariantCulture),
                    double.Parse(maxY.Value, CultureInfo.InvariantCulture)));
                minX = minX.Next;
                minY = minY.Next;
                maxX = maxX.Next;
                maxY = maxY.Next;
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static void TestNetTopologyKdTree(
            List<ExternalPoint> points,
            List<(double minX, double minY, double maxX, double maxY)> ranges)
        {
            // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
            var tree = new KdTree<ExternalPoint>();
            // Meranie obsahuje iba vkladanie prvkov do externeho stromu.
            var stopwatch = Stopwatch.StartNew();
            foreach (var point in points)
            {
                tree.Insert(new Coordinate(point.X, point.Y), point);
            }
            stopwatch.Stop();
            long insertMilliseconds = stopwatch.ElapsedMilliseconds;

            // Meranie intervaloveho vyhladavania sa vykonava oddelene od vkladania.
            stopwatch.Restart();
            int found = 0;
            foreach (var range in ranges)
            {
                var nodes = tree.Query(new Envelope(range.minX, range.maxX, range.minY, range.maxY));
                foreach (var node in nodes)
                {
                    found += node.Count;
                }
            }
            stopwatch.Stop();

            ValidateCount("NetTopologySuite KdTree", points, ranges, found);
            PrintResult("NetTopologySuite KdTree", insertMilliseconds, stopwatch.ElapsedMilliseconds);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static void TestNetTopologyQuadTree(
            List<ExternalPoint> points,
            List<(double minX, double minY, double maxX, double maxY)> ranges)
        {
            // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
            var tree = new NetTopologySuite.Index.Quadtree.Quadtree<ExternalPoint>();
            // NetTopologySuite pracuje s obalmi Envelope, preto sa z bodu vytvori nulovy obal.
            var stopwatch = Stopwatch.StartNew();
            foreach (var point in points)
            {
                tree.Insert(new Envelope(point.X, point.X, point.Y, point.Y), point);
            }
            stopwatch.Stop();
            long insertMilliseconds = stopwatch.ElapsedMilliseconds;

            // Query moze vratit kandidatov, preto sa kazdy kandidat overi v oblasti.
            stopwatch.Restart();
            int found = 0;
            foreach (var range in ranges)
            {
                var candidates = tree.Query(
                    new Envelope(range.minX, range.maxX, range.minY, range.maxY));
                foreach (var point in candidates)
                {
                    if (IsInside(point.X, point.Y, range))
                    {
                        found++;
                    }
                }
            }
            stopwatch.Stop();

            ValidateCount("NetTopologySuite Quadtree", points, ranges, found);
            PrintResult("NetTopologySuite Quadtree", insertMilliseconds, stopwatch.ElapsedMilliseconds);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static void TestSpatialTreesQuadTree(
            List<ExternalPoint> points,
            List<(double minX, double minY, double maxX, double maxY)> ranges)
        {
            // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
            var tree = new SpatialQuadtree(new AARectangle(16f, 47f, 7f, 3f), 16, 16);
            // SpatialTrees vyzaduje objekt implementujuci IMapObject2d.
            var stopwatch = Stopwatch.StartNew();
            foreach (var point in points)
            {
                tree.AddItem(point);
            }
            stopwatch.Stop();
            long insertMilliseconds = stopwatch.ElapsedMilliseconds;

            // Zoznam kandidatov sa znovu pouzije pre kazdu oblast.
            stopwatch.Restart();
            int found = 0;
            var candidates = new List<IMapObject2d>();
            foreach (var range in ranges)
            {
                candidates.Clear();
                tree.GetCollidingItems(
                    new AARectangle((float)range.minY, (float)range.minX,
                        (float)(range.maxY - range.minY), (float)(range.maxX - range.minX)),
                    1,
                    candidates);
                foreach (var candidate in candidates)
                {
                    if (IsInside(((ExternalPoint)candidate).X, ((ExternalPoint)candidate).Y, range))
                    {
                        found++;
                    }
                }
            }
            stopwatch.Stop();

            ValidateCount("SpatialTrees Quadtree", points, ranges, found);
            PrintResult("SpatialTrees Quadtree", insertMilliseconds, stopwatch.ElapsedMilliseconds);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static void ValidateCount(
            string name,
            List<ExternalPoint> points,
            List<(double minX, double minY, double maxX, double maxY)> ranges,
            int actual)
        {
            int expected = 0;
            foreach (var range in ranges)
            {
                foreach (var point in points)
                {
                    if (IsInside(point.X, point.Y, range))
                    {
                        expected++;
                    }
                }
            }

            if (expected != actual)
            {
                throw new InvalidOperationException(
                    $"{name}: ocakavanych {expected} bodov, ziskanych {actual}.");
            }
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static bool IsInside(
            double x,
            double y,
            (double minX, double minY, double maxX, double maxY) range)
        {
            return x >= range.minX && x <= range.maxX &&
                   y >= range.minY && y <= range.maxY;
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt X
        private static void PrintResult(string name, long insertMilliseconds, long queryMilliseconds)
        {
            Console.WriteLine(
                $"{name}: Insert {insertMilliseconds} ms, FindRange {queryMilliseconds} ms - OK");
        }
    }
}
