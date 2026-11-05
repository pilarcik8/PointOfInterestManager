using System;
using System.Collections.Generic;
using System.Numerics;

namespace DataStructures.Hybrid_Quad_Tree
{
    internal sealed class HybridKDTree<TData, TPosition>
        where TPosition : INumber<TPosition>
    {
        private sealed class Node
        {
            public LinkedList<TData> Data { get; } = new();
            public TPosition X { get; }
            public TPosition Y { get; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
            public Node(TData data, TPosition x, TPosition y)
            {
                Data.AddLast(data);
                X = x;
                Y = y;
            }
        }

        private Node? _root;
        public int Count { get; private set; }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        public void Insert(TData data, TPosition x, TPosition y)
        {
            if (_root == null)
            {
                _root = new Node(data, x, y);
                Count++;
                return;
            }

            Node current = _root;
            int depth = 0;
            while (true)
            {
                if (current.X == x && current.Y == y)
                {
                    current.Data.AddLast(data);
                    Count++;
                    return;
                }

                TPosition value = depth % 2 == 0 ? x : y;
                TPosition nodeValue = depth % 2 == 0 ? current.X : current.Y;
                bool goLeft = value < nodeValue;

                if (goLeft)
                {
                    if (current.Left == null)
                    {
                        current.Left = new Node(data, x, y);
                        break;
                    }

                    current = current.Left;
                }
                else
                {
                    if (current.Right == null)
                    {
                        current.Right = new Node(data, x, y);
                        break;
                    }

                    current = current.Right;
                }

                depth++;
            }

            Count++;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39 
        public void FindRange(
            TPosition minX,
            TPosition minY,
            TPosition maxX,
            TPosition maxY,
            LinkedList<TData> result)
        {
            if (_root == null)
            {
                return;
            }

            var nodesToVisit = new LinkedList<(Node node, int depth)>();
            nodesToVisit.AddLast((_root, 0));
            while (nodesToVisit.Count > 0)
            {
                var current = nodesToVisit.Last!.Value;
                nodesToVisit.RemoveLast();
                Node node = current.node;
                int depth = current.depth;

                if (node.X >= minX && node.X <= maxX &&
                    node.Y >= minY && node.Y <= maxY)
                {
                    foreach (var data in node.Data)
                    {
                        result.AddLast(data);
                    }
                }

                TPosition value = depth % 2 == 0 ? node.X : node.Y;
                TPosition minValue = depth % 2 == 0 ? minX : minY;
                TPosition maxValue = depth % 2 == 0 ? maxX : maxY;
                if (node.Right != null && maxValue >= value)
                {
                    nodesToVisit.AddLast((node.Right, depth + 1));
                }

                if (node.Left != null && minValue <= value)
                {
                    nodesToVisit.AddLast((node.Left, depth + 1));
                }
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 36, 39
        public LinkedList<(TData data, TPosition x, TPosition y)> GetAllPoints()
        {
            var result = new LinkedList<(TData data, TPosition x, TPosition y)>();
            if (_root == null)
            {
                return result;
            }

            var nodesToVisit = new LinkedList<Node>();
            nodesToVisit.AddLast(_root);
            while (nodesToVisit.Count > 0)
            {
                Node node = nodesToVisit.Last!.Value;
                nodesToVisit.RemoveLast();
                foreach (var data in node.Data)
                {
                    result.AddLast((data, node.X, node.Y));
                }

                if (node.Right != null)
                {
                    nodesToVisit.AddLast(node.Right);
                }

                if (node.Left != null)
                {
                    nodesToVisit.AddLast(node.Left);
                }
            }

            return result;
        }
    }
}
