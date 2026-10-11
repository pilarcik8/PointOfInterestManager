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
    }
}