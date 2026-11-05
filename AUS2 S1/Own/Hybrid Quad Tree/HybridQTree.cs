using System;
using System.Collections.Generic;
using System.Numerics;

namespace DataStructures.Hybrid_Quad_Tree
{
    public sealed class HybridQTree<TData, TPosition>
        where TPosition : INumber<TPosition>
    {
        private sealed class Node
        {
            public TPosition MinX { get; }
            public TPosition MinY { get; }
            public TPosition MaxX { get; }
            public TPosition MaxY { get; }
            public Node?[] Children { get; } = new Node?[4];
            public HybridKDTree<TData, TPosition> Data { get; set; } = new();

            // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
            public Node(TPosition minX, TPosition minY, TPosition maxX,
                    TPosition maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
            public bool HasChildren
            {
                get
                {
                    foreach (var child in Children)
                    {
                        if (child != null)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

        }

        private readonly Func<TData, (TPosition x, TPosition y)> _getPosition;
        private readonly int _maxDepth;
        private readonly Node _root;
        private readonly LinkedList<TData> _outOfBoundsData = new();

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        public HybridQTree(
            TPosition minX,
            TPosition minY,
            TPosition maxX,
            TPosition maxY,
            Func<TData, (TPosition x, TPosition y)> getPosition,
            int maxDepth = 16)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("maxDepth is negative", nameof(maxDepth));
            }

            _getPosition = getPosition ?? throw new ArgumentNullException(nameof(getPosition));
            _maxDepth = maxDepth;
            _root = new Node(minX, minY, maxX, maxY);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        public void Insert(TData item)
        {
            var (x, y) = _getPosition(item);
            if (!IsInside(_root, x, y))
            {
                _outOfBoundsData.AddLast(item);
                return;
            }

            Node current = _root;
            int depth = 0;
            while (true)
            {
                if (depth >= _maxDepth ||
                    (!current.HasChildren && current.Data.Count == 0))
                {
                    current.Data.Insert(item, x, y);
                    return;
                }

                if (!current.HasChildren)
                {
                    Split(current);
                }

                int index = GetQuadrantIndex(current, x, y);
                current.Children[index] ??= CreateChild(current, index);
                current = current.Children[index]!;
                depth++;
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static void Split(Node node)
        {
            var existing = node.Data.GetAllPoints();
            node.Data = new HybridKDTree<TData, TPosition>();
            foreach (var item in existing)
            {
                int index = GetQuadrantIndex(node, item.x, item.y);
                node.Children[index] ??= CreateChild(node, index);
                node.Children[index]!.Data.Insert(item.data, item.x, item.y);
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        public LinkedList<TData> FindRange(
            TPosition minX,
            TPosition minY,
            TPosition maxX,
            TPosition maxY)
        {
            var result = new LinkedList<TData>();
            var nodesToVisit = new LinkedList<Node>();
            nodesToVisit.AddLast(_root);
            while (nodesToVisit.Count > 0)
            {
                Node current = nodesToVisit.Last!.Value;
                nodesToVisit.RemoveLast();
                if (!Intersects(current, minX, minY, maxX, maxY))
                {
                    continue;
                }

                current.Data.FindRange(minX, minY, maxX, maxY, result);

                foreach (var child in current.Children)
                {
                    if (child != null)
                    {
                        nodesToVisit.AddLast(child);
                    }
                }
            }

            // Out of bounds data
            foreach (var item in _outOfBoundsData)
            {
                var (x, y) = _getPosition(item);
                if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                {
                    result.AddLast(item);
                }
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static Node CreateChild(Node node, int index)
        {
            TPosition two = TPosition.CreateChecked(2);
            TPosition midX = (node.MinX + node.MaxX) / two;
            TPosition midY = (node.MinY + node.MaxY) / two;

            return index switch
            {
                0 => new Node(node.MinX, node.MinY, midX, midY),
                1 => new Node(midX, node.MinY, node.MaxX, midY),
                2 => new Node(node.MinX, midY, midX, node.MaxY),
                _ => new Node(midX, midY, node.MaxX, node.MaxY)
            };
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static int GetQuadrantIndex(Node node, TPosition x, TPosition y)
        {
            TPosition two = TPosition.CreateChecked(2);
            TPosition midX = (node.MinX + node.MaxX) / two;
            TPosition midY = (node.MinY + node.MaxY) / two;
            return x < midX ? (y < midY ? 0 : 2) : (y < midY ? 1 : 3);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static bool IsInside(Node node, TPosition x, TPosition y)
        {
            return x >= node.MinX && x <= node.MaxX &&
                   y >= node.MinY && y <= node.MaxY;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static bool Intersects(
            Node node,
            TPosition minX,
            TPosition minY,
            TPosition maxX,
            TPosition maxY)
        {
            return node.MinX <= maxX && node.MaxX >= minX &&
                   node.MinY <= maxY && node.MaxY >= minY;
        }
    }
}
