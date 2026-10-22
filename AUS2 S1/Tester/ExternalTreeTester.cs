using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using FastG;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using SpatialTrees.Quadtrees;
using AUS2_S1.KD_Tree;

namespace AUS2_S1.Tester
{
    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal interface IBenchmarkTree
    {
        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        void Insert(long id, double x, double y);
        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        void FindRange(double minX, double minY, double maxX, double maxY);
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal sealed class CoordinateValue : IComparable<CoordinateValue>
    {
        public long Id { get; }
        public double Value { get; }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public CoordinateValue(long id, double value)
        {
            Id = id;
            Value = value;
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public int CompareTo(CoordinateValue? other)
        {
            return other == null ? 1 : Value.CompareTo(other.Value);
        }
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal sealed class ExternalPoint : IMapObject2d
    {
        private const float PointBoundingBoxSize = 0.000001f; // nevie urobit point tak robim extremne maly box

        public long Id { get; }
        public int ObjectType { get; set; } = 1;
        public Point2 Location { get; set; }
        public AARectangle BoundingBox { get; }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public ExternalPoint(long id, double x, double y)
        {
            Id = id;
            Location = new Point2((float)y, (float)x);
            BoundingBox = new AARectangle(Location, PointBoundingBoxSize, PointBoundingBoxSize);
        }
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal sealed class OwnKdTreeAdapter : IBenchmarkTree
    {
        private readonly KDTree<CoordinateValue> _tree = new(2);

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void Insert(long id, double x, double y)
        {
            var data = new CoordinateValue(id, x);
            var keys = new LinkedList<KDKey<CoordinateValue>>();
            keys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(id, x)));
            keys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(id, y)));
            _tree.Insert(data, keys);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            var minKeys = new LinkedList<KDKey<CoordinateValue>>();
            var maxKeys = new LinkedList<KDKey<CoordinateValue>>();
            minKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, minX)));
            minKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, minY)));
            maxKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, maxX)));
            maxKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, maxY)));
            _tree.FindRange(minKeys, maxKeys);
        }
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal sealed class OwnQuadTreeAdapter : IBenchmarkTree
    {
        private readonly Dictionary<long, (double x, double y)> _positions = new();
        private readonly QTree<long, double> _tree;

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public OwnQuadTreeAdapter()
        {
            _tree = new QTree<long, double>(
                47.0, 16.0, 50.0, 23.0, id => _positions[id], 16);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void Insert(long id, double x, double y)
        {
            _positions[id] = (x, y);
            _tree.Insert(id);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.FindRange(minX, minY, maxX, maxY);
        }
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal sealed class NetTopologyKdTreeAdapter : IBenchmarkTree
    {
        private readonly KdTree<ExternalPoint> _tree = new();

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Coordinate(x, y), new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.Query(new Envelope(minX, maxX, minY, maxY));
        }
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal sealed class NetTopologyQuadTreeAdapter : IBenchmarkTree
    {
        private readonly NetTopologySuite.Index.Quadtree.Quadtree<ExternalPoint> _tree = new();

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Envelope(x, x, y, y), new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.Query(new Envelope(minX, maxX, minY, maxY));
        }
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    internal sealed class SpatialTreesQuadTreeAdapter : IBenchmarkTree
    {
        private readonly SpatialTrees.Quadtrees.Quadtree _tree =
            new(new AARectangle(16f, 47f, 7f, 3f), 16, 16);
        private readonly List<IMapObject2d> _result = new();

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void Insert(long id, double x, double y)
        {
            _tree.AddItem(new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _result.Clear();
            _tree.GetCollidingItems(
                new AARectangle((float)minY, (float)minX,
                    (float)(maxY - minY), (float)(maxX - minX)),
                1,
                _result);
        }
    }

    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
    public static class ExternalTreeTester
    {
        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        public static void Run(
            CSVParser points,
            CSVParser ranges,
            int maxPoints = int.MaxValue,
            int maxRanges = int.MaxValue)
        {
            var pointData = LoadPoints(points, maxPoints);
            var rangeData = LoadRanges(ranges, maxRanges);
            RunBenchmark("Vlastny KD strom", new OwnKdTreeAdapter(), pointData, rangeData);
            RunBenchmark("Vlastny Quad strom", new OwnQuadTreeAdapter(), pointData, rangeData);
            RunBenchmark("NetTopologySuite KD strom", new NetTopologyKdTreeAdapter(), pointData, rangeData);
            RunBenchmark("NetTopologySuite Quad strom", new NetTopologyQuadTreeAdapter(), pointData, rangeData);
            RunBenchmark("SpatialTrees Quad strom", new SpatialTreesQuadTreeAdapter(), pointData, rangeData);
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        private static void RunBenchmark(
            string name,
            IBenchmarkTree tree,
            List<(long id, double x, double y)> points,
            List<(double minX, double minY, double maxX, double maxY)> ranges)
        {
            var stopwatch = Stopwatch.StartNew();
            foreach (var point in points)
            {
                tree.Insert(point.id, point.x, point.y);
            }
            stopwatch.Stop();
            long insertMilliseconds = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            foreach (var range in ranges)
            {
                tree.FindRange(range.minX, range.minY, range.maxX, range.maxY);
            }
            stopwatch.Stop();

            Console.WriteLine(
                $"{name}: Insert {insertMilliseconds} ms, FindRange {stopwatch.ElapsedMilliseconds} ms");
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        private static List<(long id, double x, double y)> LoadPoints(CSVParser points, int maximum)
        {
            var result = new List<(long, double, double)>();
            var id = points.ColumnData.First;
            var x = points.ColumnA.First;
            var y = points.ColumnB.First;
            while (id != null && x != null && y != null && result.Count < maximum)
            {
                result.Add((
                    long.Parse(id.Value, CultureInfo.InvariantCulture),
                    double.Parse(x.Value, CultureInfo.InvariantCulture),
                    double.Parse(y.Value, CultureInfo.InvariantCulture)));
                id = id.Next;
                x = x.Next;
                y = y.Next;
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt vytvorenie jednoducheho testera externych stromov
        private static LinkedList<(double minX, double minY, double maxX, double maxY)> LoadRanges(
            CSVParser ranges,
            int maximum)
        {
            var result = new LinkedList<(double, double, double, double)>();
            var minX = ranges.ColumnMinA.First;
            var minY = ranges.ColumnMinB.First;
            var maxX = ranges.ColumnMaxA.First;
            var maxY = ranges.ColumnMaxB.First;
            while (minX != null && minY != null && maxX != null && maxY != null &&
                   result.Count < maximum)
            {
                result.AddFirst((
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
    }
}
