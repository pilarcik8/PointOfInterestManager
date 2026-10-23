using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using BKDTree;
using FastG;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using SpatialTrees.Quadtrees;
using AUS2_S1.Hybrid_Quad_Tree;
using AUS2_S1.KD_Tree;
using QuadTree;

namespace AUS2_S1.Tester
{
    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal interface IBenchmarkTree
    {
        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        void Insert(long id, double x, double y);
        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        void FindRange(double minX, double minY, double maxX, double maxY);
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class CoordinateValue : IComparable<CoordinateValue>
    {
        public long Id { get; }
        public double Value { get; }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public CoordinateValue(long id, double value)
        {
            Id = id;
            Value = value;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public int CompareTo(CoordinateValue? other)
        {
            return other == null ? 1 : Value.CompareTo(other.Value);
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class ExternalPoint : IMapObject2d
    {
        private const float PointBoundingBoxSize = 0.000001f; // nevie urobit point tak robim extremne maly box

        public long Id { get; }
        public int ObjectType { get; set; } = 1;
        public Point2 Location { get; set; }
        public AARectangle BoundingBox { get; }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public ExternalPoint(long id, double x, double y)
        {
            Id = id;
            Location = new Point2((float)y, (float)x);
            BoundingBox = new AARectangle(Location, PointBoundingBoxSize, PointBoundingBoxSize);
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class OwnKdTreeAdapter : IBenchmarkTree
    {
        private readonly KDTree<CoordinateValue> _tree = new(2);

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void Insert(long id, double x, double y)
        {
            var data = new CoordinateValue(id, x);
            var keys = new LinkedList<KDKey<CoordinateValue>>();
            keys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(id, x)));
            keys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(id, y)));
            _tree.Insert(data, keys);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
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

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class OwnQuadTreeAdapter : IBenchmarkTree
    {
        private readonly Dictionary<long, (double x, double y)> _positions = new();
        private readonly QTree<long, double> _tree;

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public OwnQuadTreeAdapter()
        {
            _tree = new QTree<long, double>(
                47.0, 16.0, 50.0, 23.0, id => _positions[id], 16);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void Insert(long id, double x, double y)
        {
            _positions[id] = (x, y);
            _tree.Insert(id);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.FindRange(minX, minY, maxX, maxY);
        }
    }


    // Vygenerovane / upravene pomocou AI - Kapitola 43
    internal sealed class HybridQuadTreeAdapter : IBenchmarkTree
    {
        private readonly Dictionary<long, (double x, double y)> _positions = new();
        private readonly HybridQTree<long, double> _tree;

        // Vygenerovane / upravene pomocou AI - Kapitola 43
        public HybridQuadTreeAdapter()
        {
            _tree = new HybridQTree<long, double>(
                47.0, 16.0, 50.0, 23.0, id => _positions[id], 16);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 43
        public void Insert(long id, double x, double y)
        {
            _positions[id] = (x, y);
            _tree.Insert(id);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 43
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.FindRange(minX, minY, maxX, maxY);
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 46
    internal sealed class QuadTreeMasterAdapter : IBenchmarkTree
    {
        private const float PointSize = 0.000001f;
        private readonly QuadTree<long> _tree =
            new(16, 16, 47f, 16f, 3f, 7f);
        private List<long> _result = new();

        // Vygenerovane / upravene pomocou AI - Kapitola 46
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(
                id,
                new Quad(
                    (float)x,
                    (float)y,
                    (float)x + PointSize,
                    (float)y + PointSize));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 46
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.SearchArea(
                new Quad(
                    (float)minX,
                    (float)minY,
                    (float)maxX,
                    (float)maxY),
                ref _result);
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 47
    internal sealed class OmniKdTreeAdapter : IBenchmarkTree
    {
        private sealed class Point
        {
            public long Id { get; }
            public double X { get; }
            public double Y { get; }

            public Point(long id, double x, double y)
            {
                Id = id;
                X = x;
                Y = y;
            }
        }

        private readonly Omni.Collections.Spatial.KDTree.KdTree<Point> _tree =
            Omni.Collections.Spatial.KDTree.KdTree<Point>.Create2D(
                point => point.X,
                point => point.Y);

        // Vygenerovane / upravene pomocou AI - Kapitola 47
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Point(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 47
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.FindInRange(
                new[] { minX, minY },
                new[] { maxX, maxY });
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 50
    internal sealed class BkdTreeAdapter : IBenchmarkTree
    {
        private sealed class Point
        {
            public long Id { get; }
            public double X { get; }
            public double Y { get; }

            public Point(long id, double x, double y)
            {
                Id = id;
                X = x;
                Y = y;
            }
        }

        private readonly struct PointComparer : IDimensionalComparer<Point>
        {
            public int Compare(in Point left, in Point right, int dimension)
            {
                return dimension switch
                {
                    0 => left.X.CompareTo(right.X),
                    1 => left.Y.CompareTo(right.Y),
                    _ => throw new ArgumentOutOfRangeException(nameof(dimension))
                };
            }
        }

        private readonly BKDTree<Point, PointComparer> _tree =
            new(2, new PointComparer());


        // Vygenerovane / upravene pomocou AI - Kapitola 50
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Point(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 50
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            var lowerLimit = new Option<Point>(
                true,
                new Point(0, minX, minY));
            var upperLimit = new Option<Point>(
                true,
                new Point(0, maxX, maxY));

            _tree.DoForEach(
                _ => false,
                lowerLimit,
                upperLimit,
                upperLimitInclusive: true);
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class NetTopologySuiteKdTreeAdapter : IBenchmarkTree
    {
        private readonly KdTree<ExternalPoint> _tree = new();

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Coordinate(x, y), new ExternalPoint(id, x, y));
        }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.Query(new Envelope(minX, maxX, minY, maxY));
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class NetTopologyQuadTreeAdapter : IBenchmarkTree
    {
        private readonly NetTopologySuite.Index.Quadtree.Quadtree<ExternalPoint> _tree = new();

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Envelope(x, x, y, y), new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.Query(new Envelope(minX, maxX, minY, maxY));
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class SpatialTreesQuadTreeAdapter : IBenchmarkTree
    {
        private readonly SpatialTrees.Quadtrees.Quadtree _tree =
            new(new AARectangle(16f, 47f, 7f, 3f), 16, 16);
        private readonly List<IMapObject2d> _result = new();

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void Insert(long id, double x, double y)
        {
            _tree.AddItem(new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
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

    public static class ExternalDataTreeTester
    {
    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public static void Run(
            CSVParser points,
            CSVParser ranges,
            int maxPoints = int.MaxValue,
            int maxRanges = int.MaxValue)
        {
            var pointData = LoadPoints(points, maxPoints);
            var rangeData = LoadRanges(ranges, maxRanges);
            // MOJE
            RunBenchmark("Vlastny KD strom", new OwnKdTreeAdapter(), pointData, rangeData);
            RunBenchmark("Vlastny Quad strom", new OwnQuadTreeAdapter(), pointData, rangeData);
            RunBenchmark("Vlastny Hybridny Quad strom", new HybridQuadTreeAdapter(), pointData, rangeData);
            // EXTERNE
            //QUAD
            RunBenchmark("QuadTree-master", new QuadTreeMasterAdapter(), pointData, rangeData);
            RunBenchmark("NetTopologySuite Quad strom", new NetTopologyQuadTreeAdapter(), pointData, rangeData);
            RunBenchmark("SpatialTrees Quad strom", new SpatialTreesQuadTreeAdapter(), pointData, rangeData);
            //KD
            RunBenchmark("Omni KD strom", new OmniKdTreeAdapter(), pointData, rangeData);
            RunBenchmark("BKDTree", new BkdTreeAdapter(), pointData, rangeData);
            RunBenchmark("NetTopologySuite KD strom", new NetTopologySuiteKdTreeAdapter(), pointData, rangeData);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        private static void RunBenchmark(
            string name,
            IBenchmarkTree tree,
            LinkedList<(long id, double x, double y)> points,
            LinkedList<(double minX, double minY, double maxX, double maxY)> ranges)
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

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        private static LinkedList<(long id, double x, double y)> LoadPoints(CSVParser points, int maximum)
        {
            var result = new LinkedList<(long, double, double)>();
            var id = points.ColumnData.First;
            var x = points.ColumnA.First;
            var y = points.ColumnB.First;
            while (id != null && x != null && y != null && result.Count < maximum)
            {
                result.AddFirst((
                    long.Parse(id.Value, CultureInfo.InvariantCulture),
                    double.Parse(x.Value, CultureInfo.InvariantCulture),
                    double.Parse(y.Value, CultureInfo.InvariantCulture)));
                id = id.Next;
                x = x.Next;
                y = y.Next;
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
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
