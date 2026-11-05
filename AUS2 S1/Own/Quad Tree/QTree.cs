using System;
using System.Collections.Generic;
using System.Numerics;

namespace DataStructures.QTree
{
    // Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash) - Prompt 25 (zovšeobecnenie), Prompt 33 (TPosition)
    public class QTree<TData, TPosition> where TPosition : INumber<TPosition>
    {
        public QNode<TData, TPosition> Root;
        public int MaxDepth;
        public LinkedList<TData> OutOfBoundsData;
        private Func<TData, (TPosition x, TPosition y)> GetPosition;

        // Vygenerovane / upravene pomocou AI - Kapitola 4, 33
        public QTree(TPosition minX, TPosition minY, TPosition maxX, TPosition maxY, Func<TData, (TPosition x, TPosition y)> getPosition, int maxDepth)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("maxDepth is negative");
            }

            OutOfBoundsData = new LinkedList<TData>();
            Root = new QNode<TData, TPosition>(minX, minY, maxX, maxY);
            MaxDepth = maxDepth;
            GetPosition = getPosition;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 11, 12, 13 (pôvodná logika), Kapitola 25 (univerzálny Insert), Kapitola 33 (TPosition)
        public void Insert(TData item)
        {
            var (x, y) = GetPosition(item);

            // mimo rozsahu Root
            if (!Root.Bounds.IsInside(x, y))
            {
                OutOfBoundsData.AddLast(item);
                return;
            }

            QNode<TData, TPosition> current = Root;
            int depth = 0;

            while (true)
            {
                // ovewlow
                if (depth >= MaxDepth)
                {
                    current.Data.AddLast(item);
                    return;
                }

                //ideme do syna
                if (current.IsSplit)
                {
                    int targetIndex = current.GetQuadrantIndex(x, y);

                    // neexistuje potrebny syn tak ho vytvorime
                    if (current.Children[targetIndex] == null)
                    {
                        QNode<TData, TPosition>[] quadrants = current.Split()!;
                        current.Children[targetIndex] = quadrants[targetIndex];
                    }

                    current = current.Children[targetIndex];
                    depth++;
                    continue;
                }

                // List je prazdny - vloz priamo (šetríme pamäť)
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
                QNode<TData, TPosition>[]? splitQuadrants = current.Split();
                if (splitQuadrants == null)
                {
                    // Uz sa neda splitnut - overflow
                    current.Data.AddLast(item);
                    return;
                }

                LinkedList<TData> existingData = current.Data;
                current.Data = new LinkedList<TData>();

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

        // Vygenerovane / upravene pomocou AI - Kapitola 14, 33 (TPosition), 58
        public LinkedList<TData> FindRange(TPosition minX, TPosition minY, TPosition maxX, TPosition maxY)
        {
            var result = new LinkedList<TData>();

            if (Root == null)
            {
                return result;
            }

            var nodesToVisit = new LinkedList<QNode<TData, TPosition>>();
            nodesToVisit.AddLast(Root);

            while (nodesToVisit.Count > 0)
            {
                QNode<TData, TPosition> current = nodesToVisit.First!.Value;
                nodesToVisit.RemoveFirst();

                if (!current.Bounds.Intersects(minX, minY, maxX, maxY))
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

            // Out of bounds data
            foreach (var item in OutOfBoundsData)
            {
                var (x, y) = GetPosition(item);
                if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                {
                    result.AddLast(item);
                }
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 58
        public void DeleteNode(QNode<TData, TPosition> node)
        {
            node.Data.Clear();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 58
        public bool DeleteNode(TPosition x, TPosition y)
        {
            return DeleteNodeAtPosition(x, y, default!, matchItem: false);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 58
        public bool DeleteNode(TData item)
        {
            var (x, y) = GetPosition(item);
            return DeleteNodeAtPosition(x, y, item, matchItem: true);
        }

        private bool DeleteNodeAtPosition(TPosition x, TPosition y, TData item, bool matchItem)
        {
            if (!Root.Bounds.IsInside(x, y))
            {
                var outOfBoundsNode = OutOfBoundsData.First;
                while (outOfBoundsNode != null)
                {
                    var next = outOfBoundsNode.Next;
                    var (itemX, itemY) = GetPosition(outOfBoundsNode.Value);
                    if (itemX == x && itemY == y &&
                        (!matchItem || EqualityComparer<TData>.Default.Equals(outOfBoundsNode.Value, item)))
                    {
                        OutOfBoundsData.Remove(outOfBoundsNode);
                        return true;
                    }

                    outOfBoundsNode = next;
                }

                return false;
            }

            QNode<TData, TPosition> current = Root;
            while (current != null)
            {
                var dataNode = current.Data.First;
                while (dataNode != null)
                {
                    var next = dataNode.Next;
                    var (itemX, itemY) = GetPosition(dataNode.Value);
                    if (itemX == x && itemY == y &&
                        (!matchItem || EqualityComparer<TData>.Default.Equals(dataNode.Value, item)))
                    {
                        current.Data.Remove(dataNode);
                        return true;
                    }

                    dataNode = next;
                }

                if (!current.IsSplit)
                {
                    return false;
                }

                int index = current.GetQuadrantIndex(x, y);
                if (current.Children[index] == null)
                {
                    return false;
                }

                current = current.Children[index];
            }

            return false;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 58
        public QNode<TData, TPosition>? FindNode(TPosition x, TPosition y)
        {
            if (Root == null || !Root.Bounds.IsInside(x, y))
            {
                return null;
            }

            QNode<TData, TPosition> current = Root;
            while (current != null)
            {
                if (current.Data.Count > 0)
                {
                    var (dx, dy) = GetPosition(current.Data.First!.Value);
                    if (dx == x && dy == y)
                    {
                        return current;
                    }
                }

                if (!current.IsSplit)
                {
                    break;
                }

                int index = current.GetQuadrantIndex(x, y);
                if (current.Children[index] == null)
                {
                    break;
                }
                current = current.Children[index];
            }

            return null;
        }
    }
}
