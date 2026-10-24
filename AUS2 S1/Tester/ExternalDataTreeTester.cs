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

    // Kapitola Y: Rozhranie pre oddelenie kontroly spravnosti od profileroveho benchmarku.
    internal interface ICorrectnessTree : IBenchmarkTree
    {
        // Kapitola Y: Vykona dotaz a vrati identifikatory pre porovnanie s referencnym vysledkom.
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

        // Kapitola Y: Vracia identifikatory iba pre samostatny test spravnosti.
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
                    result.Add(item.Id);
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

        // Kapitola Y: Vracia identifikatory iba pre samostatny test spravnosti.
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

        // Kapitola Y: Vracia identifikatory iba pre samostatny test spravnosti.
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            return _tree.FindRange(minX, minY, maxX, maxY).ToArray();
        }
    }

    // Vygenerovane / upravene pomocou AI - X
    internal sealed class QuadTreeMasterAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private const float PointSize = 0.000001f;
        private readonly QuadTree<long> _tree =
            new(16, 16, 47f, 16f, 3f, 7f);
        private List<long> _result = new();

        // Vygenerovane / upravene pomocou AI - X
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

        // Vygenerovane / upravene pomocou AI - X
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

        // Kapitola Y: Vracia kopiu vysledkov, aby dalsi dotaz nezmenil overovany vysledok.
        public IReadOnlyCollection<long> FindRangeResults(
            double minX, double minY, double maxX, double maxY)
        {
            FindRange(minX, minY, maxX, maxY);
            return _result.ToArray();
        }
    }

    // Kapitola Z: Adapter pre QuadTrees-master s presnym GetObjects dotazom.
    internal sealed class QuadTreesPointQuadTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private sealed class QuadPoint : IPointFQuadStorable
        {
            public long Id { get; }
            public PointF Point { get; }

            // Kapitola Z: Uchovava identifikator a suradnice bodu pre obe formy testu.
            public QuadPoint(long id, double x, double y)
            {
                Id = id;
                Point = new PointF((float)x, (float)y);
            }
        }

        private readonly QuadTreePointF<QuadPoint> _tree =
            new(new RectangleF(47f, 16f, 3f, 7f));

        // Kapitola Y: Vlozi bod do QuadTrees-master.
        public void Insert(long id, double x, double y)
        {
            _tree.Add(new QuadPoint(id, x, y));
        }

        // Kapitola Y: Vykona profilerovy dotaz bez kopirovania vysledkov.
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.GetObjects(CreateQueryRectangle(minX, minY, maxX, maxY));
        }

        // Kapitola Y: Vrati presne vysledky metody GetObjects pre kontrolu spravnosti.
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

        // Kapitola Y: Prevedie testovany rozsah na typ pozadovany QuadTrees-master.
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

    // Vygenerovane / upravene pomocou AI - X
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

        // Vygenerovane / upravene pomocou AI - X
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Point(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - X
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.FindInRange(
                new[] { minX, minY },
                new[] { maxX, maxY });
        }

        // Kapitola Y: Vracia identifikatory iba pre samostatny test spravnosti.
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

    // Vygenerovane / upravene pomocou AI - X
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


        // Vygenerovane / upravene pomocou AI - X
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Point(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - X
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

        // Kapitola Y: Zhromazdi vysledky callbackom iba pocas testu spravnosti.
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

    // Vygenerovane / upravene pomocou AI - Kapitola X
    internal sealed class NetTopologySuiteKdTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private readonly KdTree<ExternalPoint> _tree = new();

    // Vygenerovane / upravene pomocou AI - Kapitola X
        public void Insert(long id, double x, double y)
        {
            _tree.Insert(new Coordinate(x, y), new ExternalPoint(id, x, y));
        }

    // Vygenerovane / upravene pomocou AI - Kapitola X
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _tree.Query(new Envelope(minX, maxX, minY, maxY));
        }

        // Kapitola Y: Vracia identifikatory iba pre samostatny test spravnosti.
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

    // Vygenerovane / upravene pomocou AI - Kapitola X
    internal sealed class SpatialTreesQuadTreeAdapter : IBenchmarkTree, ICorrectnessTree
    {
        private readonly SpatialTrees.Quadtrees.Quadtree _tree =
            new(new AARectangle(16f, 47f, 7f, 3f), 16, 16);
        private readonly List<IMapObject2d> _result = new();

        // Vygenerovane / upravene pomocou AI - Kapitola X
        public void Insert(long id, double x, double y)
        {
            _tree.AddItem(new ExternalPoint(id, x, y));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola X
        public void FindRange(double minX, double minY, double maxX, double maxY)
        {
            _result.Clear();
            _tree.GetCollidingItems(
                new AARectangle((float)minY, (float)minX,
                    (float)(maxY - minY), (float)(maxX - minX)),
                1,
                _result);
        }

        // Kapitola Y: Kopiruje vysledky iba pre samostatny test spravnosti.
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
    // Vygenerovane / upravene pomocou AI - Kapitola X; Kapitola Y: Spusta iba profilerove meranie rychlosti.
        public static void RunBenchmark(
            CSVParser points,
            CSVParser ranges,
            int maxPoints = int.MaxValue,
            int maxRanges = int.MaxValue)
        {
            var pointData = LoadPoints(points, maxPoints);
            var rangeData = LoadRanges(ranges, maxRanges);
            // MOJE
            RunBenchmarkForTree("Vlastny KD strom", new OwnKdTreeAdapter(), pointData, rangeData);
            RunBenchmarkForTree("Vlastny Quad strom", new OwnQuadTreeAdapter(), pointData, rangeData);
            RunBenchmarkForTree("Vlastny Hybridny Quad strom", new HybridQuadTreeAdapter(), pointData, rangeData);
            // EXTERNE
            //QUAD
            RunBenchmarkForTree("QuadTree-master", new QuadTreeMasterAdapter(), pointData, rangeData);
            RunBenchmarkForTree("QuadTrees-master", new QuadTreesPointQuadTreeAdapter(), pointData, rangeData);
            RunBenchmarkForTree("SpatialTrees Quad strom", new SpatialTreesQuadTreeAdapter(), pointData, rangeData);
            //KD
            RunBenchmarkForTree("Omni KD strom", new OmniKdTreeAdapter(), pointData, rangeData);
            RunBenchmarkForTree("BKDTree", new BkdTreeAdapter(), pointData, rangeData);
            RunBenchmarkForTree("NetTopologySuite KD strom", new NetTopologySuiteKdTreeAdapter(), pointData, rangeData);
        }

        // Kapitola Y, Z: Spusta iba test spravnosti s intervalom kontrolovanym volajucim.
        public static void RunCorrectnessTest(
            CSVParser points,
            CSVParser ranges,
            int maxPoints = int.MaxValue,
            int maxRanges = int.MaxValue,
            int insertCheckInterval = 0,
            int findCheckInterval = 0,
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
            RunCorrectnessTestForTree(
                "Vlastny KD strom",
                new OwnKdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval, 
                findCheckInterval,
                output);
            
            RunCorrectnessTestForTree(
                "Vlastny Quad strom",
                new OwnQuadTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);
            
            RunCorrectnessTestForTree(
                "Vlastny Hybridny Quad strom",
                new HybridQuadTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);

            RunCorrectnessTestForTree(
                "QuadTree",
                new QuadTreeMasterAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);

            RunCorrectnessTestForTree(
                "QuadTrees GetObjects",
                new QuadTreesPointQuadTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);

            RunCorrectnessTestForTree(
                "SpatialTrees Quad strom",
                new SpatialTreesQuadTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);

            RunCorrectnessTestForTree(
                "Omni KD strom",
                new OmniKdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);

            RunCorrectnessTestForTree(
                "BKDTree",
                new BkdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);

            RunCorrectnessTestForTree(
                "NetTopologySuite KD strom",
                new NetTopologySuiteKdTreeAdapter(),
                pointData,
                rangeData,
                insertCheckInterval,
                findCheckInterval,
                output);

        }

        // Kapitola Y, Z: Spusta benchmark pre jeden konkretny strom bez validacnych dotazov.
        private static void RunBenchmarkForTree(
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

        // Kapitola Y, Z: Kontroluje vysledky kazdy n-ty dotaz a vzdy aj posledny dotaz.
        private static void RunCorrectnessTestForTree(
            string name,
            ICorrectnessTree tree,
            LinkedList<(long id, double x, double y)> points,
            LinkedList<(double minX, double minY, double maxX, double maxY)> ranges,
            int insertCheckInterval,
            int findCheckInterval,
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
                if (i + 1 == mildstone)
                {
                    WriteTestOutput(output, $"{name}: Vkladanie bodu {i + 1}/{pointCount}.");
                    mildstone += pointCount / 10;
                }

                var point = points.ElementAt(i);
                tree.Insert(point.id, point.x, point.y);
                reference.Add(point.id, (point.x, point.y));

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
                    return;
                }
                Console.WriteLine($"{name}: Kontrola spravnosti pri Insert {i + 1}/{pointCount} (FindRange celej oblasti) - OK");                
            }

            // kontrola findrandge
            int rangeCount = ranges.Count;
            mildstone = rangeCount / 10;

            for (int i = 0; i < rangeCount; i++)
            {
                var range = ranges.ElementAt(i);

                if (i + 1 == mildstone)
                {
                    WriteTestOutput(output, $"{name}: Hladanie obdlznika {i + 1}/{rangeCount}.");
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

                if (!expected.SetEquals(actual))
                {
                    // Vypis chyby spravnosti a ukonci test.
                    WriteTestOutput(
                        output,
                        $"{name}: Chyba spravnosti pri FindRange #{i}. " +
                        $"Ocakavane {expected.Count}, ziskane {actual.Count}.");
                    return;
                }

                Console.WriteLine($"{name}: kontrola spravnosti po FindRange #{i}/{ranges.Count} OK.");
            }
        }

        private static void WriteTestOutput(TextWriter output, string message)
        {
            Console.WriteLine(message);
            output.WriteLine(message);
            output.Flush();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola X
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

        // Vygenerovane / upravene pomocou AI - Kapitola X
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
