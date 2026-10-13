// Made by Gemini 3.8 Flash
namespace AUS2_S1
{
    public class QTree<T>
    {
        public QNode<T> Root;
        public int MaxDepth;
        public LinkedList<T> OutOfBoundsData;
        private Func<T, (double x, double y)> GetPosition;

        public QTree(double topLeftX, double topLeftY, double bottomRightX, double bottomRightY, Func<T, (double x, double y)> getPosition, int maxDepth = int.MaxValue) //rozsah klucov a hlbka stromu
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("maxDepth is negative");
            }

            OutOfBoundsData = new LinkedList<T>();
            Root = new QNode<T>(topLeftX, topLeftY, bottomRightX, bottomRightY);
            MaxDepth = maxDepth;
            GetPosition = getPosition;
        }


        public void Insert(T item)
        {
            var (x, y) = GetPosition(item);

            // Ak je mimo rozsahu Root, uloz do OutOfBoundsData
            if (!Contains(Root, x, y))
            {
                OutOfBoundsData.AddLast(item);
                return;
            }

            QNode<T> current = Root;
            int depth = 0;

            while (true)
            {
                if (depth >= MaxDepth)
                {
                    current.Data.AddLast(item);
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
                    int targetIndex = GetQuadrantIndex(current, x, y);

                    if (current.Children[targetIndex] == null)
                    {
                        QNode<T>[] quadrants = current.Split();
                        current.Children[targetIndex] = quadrants[targetIndex];
                    }

                    current = current.Children[targetIndex];
                    depth++;
                    continue;
                }

                // List je prazdny - vloz priamo
                if (current.Data.Count == 0)
                {
                    current.Data.AddLast(item);
                    return;
                }

                // Skontroluj ci existujuce data maju rovnaku poziciu
                var (existX, existY) = GetPosition(current.Data.First!.Value);
                if (existX == x && existY == y)
                {
                    // Rovnaka pozicia - pridaj do toho isteho uzla (duplikaty)
                    current.Data.AddLast(item);
                    return;
                }

                // Treba splitnut - presun existujuce data do deti
                QNode<T>[] splitQuadrants = current.Split();
                LinkedList<T> existingData = current.Data;
                current.Data = new LinkedList<T>();

                foreach (var existing in existingData)
                {
                    var (eX, eY) = GetPosition(existing);
                    int eIndex = GetQuadrantIndex(current, eX, eY);
                    if (current.Children[eIndex] == null)
                    {
                        current.Children[eIndex] = splitQuadrants[eIndex];
                    }
                    current.Children[eIndex].Data.AddLast(existing);
                }

                int newIndex = GetQuadrantIndex(current, x, y);
                if (current.Children[newIndex] == null)
                {
                    current.Children[newIndex] = splitQuadrants[newIndex];
                }

                current = current.Children[newIndex];
                depth++;
            }
        }

        // Vyhladavanie v obdlznikovom intervale
        public List<T> Find(double minX, double minY, double maxX, double maxY)
        {
            var result = new List<T>();

            if (Root == null)
            {
                return result;
            }

            var nodesToVisit = new List<QNode<T>>();
            nodesToVisit.Add(Root);

            for (int i = 0; i < nodesToVisit.Count; i++)
            {
                QNode<T> current = nodesToVisit[i];

                if (!Intersects(current, minX, minY, maxX, maxY))
                {
                    continue;
                }

                foreach (var item in current.Data)
                {
                    var (x, y) = GetPosition(item);
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        result.Add(item);
                    }
                }

                foreach (var child in current.Children)
                {
                    if (child != null)
                    {
                        nodesToVisit.Add(child);
                    }
                }
            }

            return result;
        }

        private bool Intersects(QNode<T> node, double minX, double minY, double maxX, double maxY)
        {
            return node.TopLeftX <= maxX &&
                   node.BottomRightX >= minX &&
                   node.TopLeftY <= maxY &&
                   node.BottomRightY >= minY;
        }

        private bool Contains(QNode<T> node, double x, double y)
        {
            return x >= node.TopLeftX && x <= node.BottomRightX &&
                   y >= node.TopLeftY && y <= node.BottomRightY;
        }

        private int GetQuadrantIndex(QNode<T> node, double x, double y)
        {
            double midX = (node.TopLeftX + node.BottomRightX) / 2;
            double midY = (node.TopLeftY + node.BottomRightY) / 2;

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