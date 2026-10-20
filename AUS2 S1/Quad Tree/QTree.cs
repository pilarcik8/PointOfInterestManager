// Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash)
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AUS2_S1
{
    // Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash)
    public class QTree<TData, TPosition> where TPosition : INumber<TPosition>
    {
        public QNode<TData, TPosition> Root;
        public int MaxDepth;
        public LinkedList<TData> OutOfBoundsData;
        private Func<TData, (TPosition x, TPosition y)> GetPosition;

        public QTree(TPosition topLeftX, TPosition topLeftY, TPosition bottomRightX, TPosition bottomRightY, Func<TData, (TPosition x, TPosition y)> getPosition, int maxDepth)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("maxDepth is negative");
            }

            OutOfBoundsData = new LinkedList<TData>();
            Root = new QNode<TData, TPosition>(topLeftX, topLeftY, bottomRightX, bottomRightY);
            MaxDepth = maxDepth;
            GetPosition = getPosition;
        }

        public void Insert(TData item)
        {
            var (x, y) = GetPosition(item);

            // mimo rozsahu Root
            if (!Root.IsInside(x, y))
            {
                OutOfBoundsData.AddLast(item);
                return;
            }

            QNode<TData, TPosition> current = Root;
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
                        QNode<TData, TPosition>[] quadrants = current.Split();
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
                QNode<TData, TPosition>[] splitQuadrants = current.Split();
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

        // Vyhladavanie v obdlznikovom intervale
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