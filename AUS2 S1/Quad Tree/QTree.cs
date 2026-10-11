// Made by Gemini 3.8 Flash
using System.Numerics;

namespace AUS2_S1
{
    public class QTree<T>
    {
        public QNode<T> Root;
        public int MaxDepth;

        public QTree(Polygon boundary, int maxDepth = int.MaxValue)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("maxDepth is negative");
            }

            Root = new QNode<T>(boundary);
            MaxDepth = maxDepth;
        }

        public QTree(QNode<T> root, int maxDepth = int.MaxValue)
        {
            if (maxDepth <= 0)
            {
                throw new ArgumentException("maxDepth is negative");
            }

            Root = root;
            MaxDepth = maxDepth;
        }

        public void CreateAndInsertPolygon(double minX, double minY, double maxX, double maxY)
        {
            Polygon polygon = new Polygon(minX, minY, maxX, maxY);
            InsertPolygon(polygon);
        }

        public void InsertPolygon(Polygon polygon)
        {
            QNode<T> current = Root;
            int depth = 0;

            while (true)
            {
                if (depth >= MaxDepth)
                {
                    current.OverflowPolygons.Add(polygon);
                    return;
                }

                Polygon[] quadrants = current.Boundary.Split();
                int targetIndex = -1;

                for (int i = 0; i < quadrants.Length; i++)
                {
                    if (quadrants[i].CompareWith(polygon) != -1)
                    {
                        targetIndex = i;
                        break;
                    }
                }

                if (targetIndex == -1)
                {
                    current.OverflowPolygons.Add(polygon);
                    return;
                }

                if (current.Children[targetIndex] == null)
                {
                    current.Children[targetIndex] = new QNode<T>(quadrants[targetIndex]);
                }

                current = current.Children[targetIndex];
                depth++;
            }
        }

        public void CreateAndInsertPoint(T data, Func<T, (double x, double y)> getPosition)
        {
            Point<T> point = new Point<T>(data, getPosition);
            InsertPoint(point);
        }

        public void InsertPoint(Point<T> point)
        {
            QNode<T> current = Root;
            int depth = 0;

            while (true)
            {
                if (depth >= MaxDepth)
                {
                    current.Points.Add(point);
                    return;
                }

                bool hasChildren = false;
                for (int i = 0; i < current.Children.Length; i++)
                {
                    if (current.Children[i] != null)
                    {
                        hasChildren = true;
                        break;
                    }
                }

                if (hasChildren)
                {
                    Polygon[] quadrants = current.Boundary.Split();
                    int targetIndex = GetQuadrantIndex(current.Boundary, point.X, point.Y);

                    if (current.Children[targetIndex] == null)
                    {
                        current.Children[targetIndex] = new QNode<T>(quadrants[targetIndex]);
                        current.Children[targetIndex].Points.Add(point);
                        return;
                    }

                    current = current.Children[targetIndex];
                    depth++;
                    continue;
                }

                if (current.Points.Count == 0)
                {
                    current.Points.Add(point);
                    return;
                }

                if (current.Points[0].X == point.X && current.Points[0].Y == point.Y)
                {
                    current.Points.Add(point);
                    return;
                }

                Polygon[] splitQuadrants = current.Boundary.Split();
                List<Point<T>> existingPoints = new List<Point<T>>(current.Points);
                current.Points.Clear();

                foreach (var p in existingPoints)
                {
                    int pIndex = GetQuadrantIndex(current.Boundary, p.X, p.Y);
                    if (current.Children[pIndex] == null)
                    {
                        current.Children[pIndex] = new QNode<T>(splitQuadrants[pIndex]);
                    }
                    current.Children[pIndex].Points.Add(p);
                }

                int newPointIndex = GetQuadrantIndex(current.Boundary, point.X, point.Y);
                if (current.Children[newPointIndex] == null)
                {
                    current.Children[newPointIndex] = new QNode<T>(splitQuadrants[newPointIndex]);
                    current.Children[newPointIndex].Points.Add(point);
                    return;
                }

                current = current.Children[newPointIndex];
                depth++;
            }
        }

        public (List<Polygon> Polygons, List<Point<T>> Points) Find(Polygon s)
        {
            var foundPolygons = new List<Polygon>();
            var foundPoints = new List<Point<T>>();

            if (Root == null || s == null)
            {
                return (foundPolygons, foundPoints);
            }

            List<QNode<T>> nodesToVisit = new List<QNode<T>>();
            nodesToVisit.Add(Root);

            for (int i = 0; i < nodesToVisit.Count; i++)
            {
                QNode<T> current = nodesToVisit[i];

                if (!Intersects(current.Boundary, s))
                {
                    continue;
                }

                foreach (var poly in current.OverflowPolygons)
                {
                    if (Intersects(poly, s))
                    {
                        foundPolygons.Add(poly);
                    }
                }

                foreach (var pt in current.Points)
                {
                    if (Contains(s, pt.X, pt.Y))
                    {
                        foundPoints.Add(pt);
                    }
                }

                foreach (var child in current.Children)
                {
                    if (child != null && Intersects(child.Boundary, s))
                    {
                        nodesToVisit.Add(child);
                    }
                }
            }

            return (foundPolygons, foundPoints);
        }

        public (List<Polygon> Polygons, List<Point<T>> Points) Find(double minX, double minY, double maxX, double maxY)
        {
            return Find(new Polygon(minX, minY, maxX, maxY));
        }

        private bool Intersects(Polygon a, Polygon b)
        {
            return a.TopLeftX <= b.BottomRightX &&
                   a.BottomRightX >= b.TopLeftX &&
                   a.TopLeftY <= b.BottomRightY &&
                   a.BottomRightY >= b.TopLeftY;
        }

        private bool Contains(Polygon boundary, double x, double y)
        {
            return x >= boundary.TopLeftX && x <= boundary.BottomRightX &&
                   y >= boundary.TopLeftY && y <= boundary.BottomRightY;
        }

        private int GetQuadrantIndex(Polygon boundary, double x, double y)
        {
            double midX = (boundary.TopLeftX + boundary.BottomRightX) / 2;
            double midY = (boundary.TopLeftY + boundary.BottomRightY) / 2;

            if (x < midX)
            {
                return (y < midY) ? 0 : 2;
            }
            else
            {
                return (y < midY) ? 1 : 3;
            }
        }
    }
}