using System;
using System.Collections.Generic;
using System.Numerics;

namespace AUS2_S1.Hybrid_Quad_Tree
{
    public sealed class HybridQTree<TData, TPosition>
        where TPosition : INumber<TPosition>
    {
        private sealed class Node
        {
            public TPosition TopLeftX { get; }
            public TPosition TopLeftY { get; }
            public TPosition BottomRightX { get; }
            public TPosition BottomRightY { get; }
            public Node?[] Children { get; } = new Node?[4];
            public HybridKDTree<TData, TPosition> Data { get; set; } = new();
            
            // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
            public Node(TPosition topLeftX, TPosition topLeftY, TPosition bottomRightX, 
                    TPosition bottomRightY)
            {
                TopLeftX = topLeftX;
                TopLeftY = topLeftY;
                BottomRightX = bottomRightX;
                BottomRightY = bottomRightY;
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
            TPosition topLeftX,
            TPosition topLeftY,
            TPosition bottomRightX,
            TPosition bottomRightY,
            Func<TData, (TPosition x, TPosition y)> getPosition,
            int maxDepth = 16)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("maxDepth is negative", nameof(maxDepth));
            }

            _getPosition = getPosition ?? throw new ArgumentNullException(nameof(getPosition));
            _maxDepth = maxDepth;
            _root = new Node(topLeftX, topLeftY, bottomRightX, bottomRightY);
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
            TPosition midX = (node.TopLeftX + node.BottomRightX) / two;
            TPosition midY = (node.TopLeftY + node.BottomRightY) / two;

            return index switch
            {
                0 => new Node(node.TopLeftX, node.TopLeftY, midX, midY),
                1 => new Node(midX, node.TopLeftY, node.BottomRightX, midY),
                2 => new Node(node.TopLeftX, midY, midX, node.BottomRightY),
                _ => new Node(midX, midY, node.BottomRightX, node.BottomRightY)
            };
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static int GetQuadrantIndex(Node node, TPosition x, TPosition y)
        {
            TPosition two = TPosition.CreateChecked(2);
            TPosition midX = (node.TopLeftX + node.BottomRightX) / two;
            TPosition midY = (node.TopLeftY + node.BottomRightY) / two;
            return x < midX ? (y < midY ? 0 : 2) : (y < midY ? 1 : 3);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static bool IsInside(Node node, TPosition x, TPosition y)
        {
            return x >= node.TopLeftX && x <= node.BottomRightX &&
                   y >= node.TopLeftY && y <= node.BottomRightY;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        private static bool Intersects(
            Node node,
            TPosition minX,
            TPosition minY,
            TPosition maxX,
            TPosition maxY)
        {
            return node.TopLeftX <= maxX && node.BottomRightX >= minX &&
                   node.TopLeftY <= maxY && node.BottomRightY >= minY;
        }
    }
}
