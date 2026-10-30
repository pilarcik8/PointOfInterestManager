using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using BKDTree;
using FastG;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using SpatialTrees.Quadtrees;
using AUS2_S1.Hybrid_Quad_Tree;
using AUS2_S1.KD_Tree;
using QuadTree;
using QuadTrees;
using QuadTrees.QTreePointF;
using System.Runtime.ConstrainedExecution;

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

    // Vygenerovane / upravene pomocou AI - Kapitola 53
    internal interface ICorrectnessTree : IBenchmarkTree
    {
        IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY);
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
        public double X { get; }
        public double Y { get; }
        public int ObjectType { get; set; } = 1;
        public Point2 Location { get; set; }
        public AARectangle BoundingBox { get; }

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42; Kapitola Y: Uchovanie povodnych suradnic pre presnu validaciu.
        public ExternalPoint(long id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
            Location = new Point2((float)y, (float)x);
            BoundingBox = new AARectangle(Location, PointBoundingBoxSize, PointBoundingBoxSize);
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class OwnKdTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private readonly AUS2_S1.KD_Tree.KDTree<CoordinateValue, long> _tree = new(2);

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public void Insert(long id, double x, double y)
        {
            var keys = new LinkedList<KDKey<CoordinateValue>>();
            keys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(id, x)));
            keys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(id, y)));
            _tree.Insert(id, keys);
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

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            var minKeys = new LinkedList<KDKey<CoordinateValue>>();
            var maxKeys = new LinkedList<KDKey<CoordinateValue>>();
            minKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, minX)));
            minKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, minY)));
            maxKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, maxX)));
            maxKeys.AddLast(new KDKey<CoordinateValue>(new CoordinateValue(0, maxY)));

            var nodes = _tree.FindRange(minKeys, maxKeys);
            var result = new List<long>();
            foreach (var node in nodes)
            {
                foreach (var item in node.Data)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
    internal sealed class OwnQuadTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private readonly Dictionary<long, (double x, double y)> _positions = new();
        private readonly QTree<long, double> _tree;

        // Vygenerovane / upravene pomocou AI - Kapitola 41, 42
        public OwnQuadTreeAdapter(double minX, double minY, double maxX, double maxY, int maxDepth)
        {
            _tree = new QTree<long, double>(
                minX, minY, maxX, maxY, id => _positions[id], maxDepth);
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

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            return _tree.FindRange(minX, minY, maxX, maxY).ToArray();
        }
    }


    // Vygenerovane / upravene pomocou AI - Kapitola 43
    internal sealed class HybridQuadTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private readonly Dictionary<long, (double x, double y)> _positions = new();
        private readonly HybridQTree<long, double> _tree;

        // Vygenerovane / upravene pomocou AI - Kapitola 43
        public HybridQuadTreeAdapter(double minX, double minY, double maxX, double maxY, int maxDepth)
        {
            _tree = new HybridQTree<long, double>(
                minX: minX,
                minY: minY,
                maxX: maxX,
                maxY: maxY,
                maxDepth: maxDepth,
                getPosition: id => _positions[id]
                );
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

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            return _tree.FindRange(minX, minY, maxX, maxY).ToArray();
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 44, 46
    internal sealed class QuadTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private const float PointSize = 0.000001f;
        private readonly QuadTree<long> _tree;
        public QuadTreeAdapter(double minX, double minY, double maxX, double maxY, int maxDepth)
        {
            float padding = 0.0001f;
            var root = new Quad(
                (float)minX,
                (float)minY,
                (float)maxX + PointSize + padding,
                (float)maxY + PointSize + padding);

            _tree = new QuadTree<long>(
                splitCount: 4,
                depthLimit: maxDepth,
                region: root
            );
        }
        private List<long> _result = new();

        // Vygenerovane / upravene pomocou AI - Kapitola 44, 46
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

        // Vygenerovane / upravene pomocou AI - Kapitola 44, 46
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _result.Clear();
            _tree.SearchArea(
                new Quad(
                    (float)minX,
                    (float)minY,
                    (float)maxX + PointSize,
                    (float)maxY + PointSize),
                ref _result);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            FindRange(minX, minY, maxX, maxY);
            return _result.ToArray();
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 46
    internal sealed class QuadTreesPointQuadTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private sealed class QuadPoint : IPointFQuadStorable
        {
            public long Id { get; }
            public PointF Point { get; }

            // Vygenerovane / upravene pomocou AI - Kapitola 46
            public QuadPoint(long id, double x, double y)
            {
                Id = id;
                Point = new PointF((float)x, (float)y);
            }
        }

        private readonly QuadTreePointF<QuadPoint> _tree;

        public QuadTreesPointQuadTreeAdapter(double minX, double minY, double maxX, double maxY)
        {
            float padding = 0.0001f;
            _tree = new QuadTreePointF<QuadPoint>(new RectangleF((float)minX, (float)minY, (float)(maxX - minX) + padding, (float)(maxY - minY) + padding));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 46
        public void Insert(long id, double x, double y)
        {
            _tree.Add(new QuadPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 46
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.GetObjects(CreateQueryRectangle(minX, minY, maxX, maxY));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            var objects = _tree.GetObjects(
                CreateQueryRectangle(minX, minY, maxX, maxY));
            var ids = new List<long>(objects.Count);
            foreach (var point in objects)
            {
                ids.Add(point.Id);
            }

            return ids;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 46
        private static RectangleF CreateQueryRectangle(
            double minX, double minY, double maxX, double maxY)
        {
            return new RectangleF(
                (float)minX,
                (float)minY,
                (float)(maxX - minX),
                (float)(maxY - minY));
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 47
    internal sealed class OmniKdTreeAdapter : IBenchmarkTree, ICorrectnessTree
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

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            var result = _tree.FindInRange(
                new[] { minX, minY },
                new[] { maxX, maxY });
            var ids = new List<long>();
            foreach (var point in result)
            {
                ids.Add(point.Id);
            }

            return ids;
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 50
    internal sealed class BkdTreeAdapter : IBenchmarkTree, ICorrectnessTree
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

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            var result = new List<long>();
            var lowerLimit = new Option<Point>(
                true,
                new Point(0, minX, minY));
            var upperLimit = new Option<Point>(
                true,
                new Point(0, maxX, maxY));

            _tree.DoForEach(
                point =>
                {
                    result.Add(point.Id);
                    return false;
                },
                lowerLimit,
                upperLimit,
                upperLimitInclusive: true);

            return result;
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41
    internal sealed class NetTopologySuiteKdTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private readonly KdTree<ExternalPoint> _tree = new();

        // Vygenerovane / upravene pomocou AI - Kapitola 41
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Coordinate(x, y), new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.Query(new Envelope(minX, maxX, minY, maxY));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            var result = _tree.Query(new Envelope(minX, maxX, minY, maxY));
            var ids = new List<long>();
            foreach (var point in result)
            {
                ids.Add(point.Data.Id);
            }

            return ids;
        }
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 41
    internal sealed class SpatialTreesQuadTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private readonly SpatialTrees.Quadtrees.Quadtree _tree;
        private readonly List<IMapObject2d> _result = new();

        public SpatialTreesQuadTreeAdapter(double minX, double minY, double maxX, double maxY, int maxDepth = 16)
        {
            float padding = 0.0001f;
            _tree = new SpatialTrees.Quadtrees.Quadtree(
                new AARectangle((float)minY, (float)minX, (float)(maxY - minY) + padding, (float)(maxX - minX) + padding),
                maxDepth,
                maxDepth);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41
        public void Insert(long id, double x, double y)
        {
            _tree.AddItem(new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _result.Clear();
            _tree.GetCollidingItems(
                new AARectangle((float)minY, (float)minX,
                    (float)(maxY - minY), (float)(maxX - minX)),
                1,
                _result);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 53
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            FindRange(minX, minY, maxX, maxY);
            var ids = new List<long>(_result.Count);
            foreach (var item in _result)
            {
                if (item is ExternalPoint point)
                {
                    ids.Add(point.Id);
                }
            }

            return ids;
        }
    }

    public static class ExternalDataTreeTester
    {
        // Vygenerovane / upravene pomocou AI - Kapitola 41
        public static void RunBenchmark(
            CSVParser points,
            CSVParser ranges,
            string outputFilePath = "test_speed_results.txt",
            int maxPoints = int.MaxValue,
            int maxRanges = int.MaxValue)
        {
            using var output = new StreamWriter(outputFilePath, false);
            var pointData = LoadPoints(points, maxPoints);
            var rangeData = LoadRanges(ranges, maxRanges);

            var pointMinX = pointData.Min(p => p.x);
            var pointMaxX = pointData.Max(p => p.x);
            var pointMinY = pointData.Min(p => p.y);
            var pointMaxY = pointData.Max(p => p.y);

            const int maxDepth = 12;

            // MOJE
            //RunBenchmarkForTree("Vlastny KD strom", new OwnKdTreeAdapter(), pointData, rangeData, output);
            
            RunBenchmarkForTree("Vlastny Quad strom", new OwnQuadTreeAdapter(minX: pointMinX,
                                                                        minY: pointMinY,
                                                                        maxX: pointMaxX,
                                                                        maxY: pointMaxY,
                                                                        maxDepth: maxDepth),
                                                                        pointData, rangeData, output);
            RunBenchmarkForTree("Vlastny Hybridny Quad strom", new HybridQuadTreeAdapter(minX: pointMinX,
                                                                                        minY: pointMinY,
                                                                                        maxX: pointMaxX,
                                                                                        maxY: pointMaxY,
                                                                                        maxDepth: maxDepth),
                                                                                     pointData, rangeData, output); 
            // EXTERNE
            //QUAD
            RunBenchmarkForTree("QuadTree", new QuadTreeAdapter(minX: pointMinX,
                                                                minY: pointMinY,
                                                                maxX: pointMaxX,
                                                                maxY: pointMaxY,
                                                                maxDepth: maxDepth), pointData, rangeData, output);
            // max depth nema takze v parametri neni
            RunBenchmarkForTree("QuadTrees", new QuadTreesPointQuadTreeAdapter(minX: pointMinX,
                                                                               minY: pointMinY,
                                                                               maxX: pointMaxX,
                                                                               maxY: pointMaxY),
                                                                               pointData, rangeData, output);
            RunBenchmarkForTree("SpatialTrees Quad strom", new SpatialTreesQuadTreeAdapter(minX: pointMinX,
                                                                                          minY: pointMinY,
                                                                                          maxX: pointMaxX,
                                                                                          maxY: pointMaxY,
                                                                                     maxDepth: maxDepth), pointData, rangeData, output);
            //KD
             /*   
            RunBenchmarkForTree("Omni KD strom", new OmniKdTreeAdapter(), pointData, rangeData, output);
            RunBenchmarkForTree("BKDTree", new BkdTreeAdapter(), pointData, rangeData, output);
            RunBenchmarkForTree("NetTopologySuite KD strom", new NetTopologySuiteKdTreeAdapter(), pointData, rangeData, output);
             */
        }

        // Kapitola 52
        public static void RunCorrectnessTest(
            CSVParser points,
            CSVParser ranges,
            int maxPoints = int.MaxValue,
            int maxRanges = int.MaxValue,
            int insertCheckInterval = 0,
            int findCheckInterval = 0,
            int stopCheckInsertAfter = int.MaxValue,
            string outputFilePath = "test_funkcnosti.txt")
        {
            if (insertCheckInterval < 0 || findCheckInterval < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "Interval kontroly spravnosti musi byt vacsi ako nula.");
            }

            using var output = new StreamWriter(outputFilePath, false);
            var pointData = LoadPoints(points, maxPoints);
            var rangeData = LoadRanges(ranges, maxRanges);
            var pointMinX = pointData.Min(p => p.x);
            var pointMaxX = pointData.Max(p => p.x);
            var pointMinY = pointData.Min(p => p.y);
            var pointMaxY = pointData.Max(p => p.y);

            const int maxDepth = 12;

            // MOJE            
            RunCorrectnessTestForTree(
                "Vlastny KD strom",
                new OwnKdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);
          
            RunCorrectnessTestForTree(
                "Vlastny Quad strom",
                new OwnQuadTreeAdapter(minX: pointMinX,
                                       minY: pointMinY,
                                       maxX: pointMaxX,
                                       maxY: pointMaxY,
                                       maxDepth: maxDepth),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);
              
            RunCorrectnessTestForTree(
                "Vlastny Hybridny Quad strom",
                new HybridQuadTreeAdapter(minX: pointMinX,
                                          minY: pointMinY,
                                          maxX: pointMaxX,
                                          maxY: pointMaxY,
                                          maxDepth: maxDepth),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);

            // EXTERNE
            // QUAD
            RunCorrectnessTestForTree(
                "QuadTree",
                new QuadTreeAdapter(minX: pointMinX,
                                    minY: pointMinY,
                                    maxX: pointMaxX,
                                    maxY: pointMaxY,
                                    maxDepth: maxDepth),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);

            RunCorrectnessTestForTree(
                "QuadTrees GetObjects",
                new QuadTreesPointQuadTreeAdapter(minX: pointMinX,
                                                  minY: pointMinY,
                                                  maxX: pointMaxX,
                                                  maxY: pointMaxY),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);

            RunCorrectnessTestForTree(
                "SpatialTrees Quad strom",
                new SpatialTreesQuadTreeAdapter(minX: pointMinX,
                                                minY: pointMinY,
                                                maxX: pointMaxX,
                                                maxY: pointMaxY,
                                                maxDepth: maxDepth),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);

            // KD    
            
            RunCorrectnessTestForTree(
                "Omni KD strom",
                new OmniKdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);

            RunCorrectnessTestForTree(
                "BKDTree",
                new BkdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);

            RunCorrectnessTestForTree(
                "NetTopologySuite KD strom",
                new NetTopologySuiteKdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                stopCheckInsertAfter,
                output);
            
        }

        // Kapitola 41, 52, 53, 54
        private static void RunBenchmarkForTree(
            string name,
            IBenchmarkTree tree,
            LinkedList<(long id, double x, double y)> points,
            LinkedList<(double minX, double minY, double maxX, double maxY)> ranges,
            TextWriter output)
        {
            // --- 1. MERANIE VKLADANIA ---
            var stopwatch = Stopwatch.StartNew();
            int index = 0;
            foreach (var point in points)
            {
                Console.WriteLine($"{name}: Vkladanie bodu {index}/{points.Count - 1}.");
                tree.Insert(point.id, point.x, point.y);
                index++;
            }
            stopwatch.Stop();
            double insertMs = stopwatch.Elapsed.TotalMilliseconds;

            // --- 2. MERANIE VYHĽADÁVANIA ---
            stopwatch.Restart();
            index = 0;
            foreach (var range in ranges)
            {
                Console.WriteLine($"{name}: Hladanie obdlznika {index}/{ranges.Count - 1}.");
                tree.FindRange(range.minX, range.minY, range.maxX, range.maxY);
                index++;
            }
            stopwatch.Stop();
            double findRangeMs = stopwatch.Elapsed.TotalMilliseconds;

            // --- 3. VÝPOČET PRIEMEROV ---
            double avgInsertPerPoint = insertMs / points.Count;
            double avgSearchPerRange = findRangeMs / ranges.Count;

            // --- 4. ZÁPIS VÝSLEDKOV ---
            WriteTestOutput(output,
                $"{name}:\n" +
                $"  Insert:    {insertMs:F2} ms (priemer: {avgInsertPerPoint:F4} ms/bod)\n" +
                $"  FindRange: {findRangeMs:F2} ms (priemer: {avgSearchPerRange:F4} ms/oblasť)");
        }

        // Kapitola 52, 53, 54
        private static void RunCorrectnessTestForTree(
            string name,
            ICorrectnessTree tree,
            LinkedList<(long id, double x, double y)> points,
            LinkedList<(double minX, double minY, double maxX, double maxY)> ranges,
            int insertCheckInterval,
            int findCheckInterval,
            int stopCheckInsertAfter,
            TextWriter output)
        {
            WriteTestOutput(output, $"\n{name}: Spustenie testu spravnosti.");
            var reference = new Dictionary<long, (double x, double y)>();
            int pointCount = points.Count;

            var pointMinX = points.Min(p => p.x);
            var pointMaxX = points.Max(p => p.x);
            var pointMinY = points.Min(p => p.y);
            var pointMaxY = points.Max(p => p.y);

            int mildstone = pointCount / 10;

            // Vlozenie bodov a kontrola spravnosti po kazdom n-tom inserti a na konci. 
            for (int i = 0; i < pointCount; i++)
            {
                if (i == mildstone)
                {
                    Console.WriteLine($"{name}: Vkladanie bodu {i}/{pointCount - 1}.");
                    mildstone += pointCount / 10;
                }

                var point = points.ElementAt(i);
                tree.Insert(point.id, point.x, point.y);
                reference.Add(point.id, (point.x, point.y));

                if (i >= stopCheckInsertAfter)
                {
                    continue;
                }

                if (insertCheckInterval == 0)
                {
                    continue;
                }

                if (i % insertCheckInterval != 0 && i != pointCount - 1)
                {
                    continue;
                }
                // kontrola findrange celej oblasti == kontrola insertu
                var actual = tree.FindRangeResults(pointMinX, pointMinY, pointMaxX, pointMaxY).ToHashSet();
                if (!actual.SetEquals(reference.Keys))
                {
                    WriteTestOutput(output, $"/{name}: Chyba spravnosti pri Insert #{i}.");
                    //return; //nebudem testovat findrange ked insert nefunguje
                }
                Console.WriteLine($"{name}: Kontrola spravnosti pri poradi Insert {i}/{pointCount - 1} - OK");
            }

            // kontrola findrandge
            int rangeCount = ranges.Count;
            mildstone = rangeCount / 10;

            for (int i = 0; i < rangeCount; i++)
            {
                var range = ranges.ElementAt(i);

                if (i + 1 == mildstone)
                {
                    Console.WriteLine($"{name}: Hladanie obdlznika {i}/{rangeCount - 1}.");
                    mildstone += rangeCount / 10;
                }

                if (findCheckInterval == 0)
                {
                    continue;
                }

                //kontrolujeme n-ty interval a na koniec
                if (i % findCheckInterval != 0 && i != ranges.Count - 1)
                {
                    continue;
                }

                var expected = reference
                    .Where(item =>
                        item.Value.x >= range.minX &&
                        item.Value.x <= range.maxX &&
                        item.Value.y >= range.minY &&
                        item.Value.y <= range.maxY)
                    .Select(item => item.Key)
                    .ToHashSet();
                var actual = tree.FindRangeResults(
                    range.minX, range.minY, range.maxX, range.maxY).ToHashSet();

                if (!expected.Count.Equals(actual.Count))
                {
                    WriteTestOutput(
                        output,
                        $"{name}: Chyba spravnosti pri FindRange #{i}. " +
                        $"Ocakavane {expected.Count}, ziskane {actual.Count}.");
                    continue;
                }

                if (!expected.SetEquals(actual))
                {
                    WriteTestOutput(
                        output,
                        $"{name}: Chyba spravnosti pri FindRange #{i}. " +
                        $"Ocakavane {expected.Count}, ziskane {actual.Count}.");
                    continue;
                }

                Console.WriteLine($"{name}: kontrola spravnosti po FindRange #{i}/{ranges.Count} OK.");
            }
            WriteTestOutput(output, $"{name}: Test spravnosti uspesne ukonceny.");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 54
        private static void WriteTestOutput(TextWriter output, string message)
        {
            Console.WriteLine(message);
            output.WriteLine(message);
            output.Flush();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 41
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

        // Vygenerovane / upravene pomocou AI - Kapitola 41
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
