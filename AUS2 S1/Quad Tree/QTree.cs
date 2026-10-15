// Made by Gemini 3.8 Flash
namespace AUS2_S1
{
    public class QTree<T>
    {
        public QNode<T> Root;
        public int MaxDepth;
        public LinkedList<T> OutOfBoundsData;
        private Func<T, (double x, double y)> GetPosition;

        public QTree(double topLeftX, double topLeftY, double bottomRightX, double bottomRightY, Func<T, (double x, double y)> getPosition, int maxDepth) //rozsah klucov a hlbka stromu
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

            // mimo rozsahu Root
            if (!Root.IsInside(x, y))
            {
                OutOfBoundsData.AddLast(item);
                return;
            }

            QNode<T> current = Root;
            int depth = 0;

            while (true)
            {
                // dosiahnuta max hlbka -> dame do listu
                if (depth >= MaxDepth)
                {
                    current.Data.AddLast(item);
                    return;
                }

                // Splitnu sa ak ma uzol deti
                if (current.HasChildren)
                {
                    int targetIndex = current.GetQuadrantIndex(x, y);

                    // neexistuje potrebny syn tak ho vytvorime
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
                    int eIndex = current.GetQuadrantIndex(eX, eY);
                    if (current.Children[eIndex] == null)
                    {
                        current.Children[eIndex] = splitQuadrants[eIndex];
                    }
                    current.Children[eIndex].Data.AddLast(existing);
                }

                int newIndex = current.GetQuadrantIndex(x, y);
                if (current.Children[newIndex] == null)
                {
                    current.Children[newIndex] = splitQuadrants[newIndex];
                }

                current = current.Children[newIndex];
                depth++;
            }
        }

        // Vyhladavanie v obdlznikovom intervale
        public LinkedList<T> Find(double minX, double minY, double maxX, double maxY)
        {
            var result = new LinkedList<T>();

            if (Root == null)
            {
                return result;
            }

            var nodesToVisit = new LinkedList<QNode<T>>();
            nodesToVisit.AddLast(Root);

            while (nodesToVisit.Count > 0)
            {
                QNode<T> current = nodesToVisit.First!.Value;
                nodesToVisit.RemoveFirst();

                if (!current.Intersects(minX, minY, maxX, maxY))
                {
                    continue;
                }

                foreach (var item in current.Data)
                {
                    var (x, y) = GetPosition(item);
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        result.AddLast(item);
                    }
                }

                foreach (var child in current.Children)
                {
                    if (child != null)
                    {
                        nodesToVisit.AddLast(child);
                    }
                }
            }

            return result;
        }
    }
}