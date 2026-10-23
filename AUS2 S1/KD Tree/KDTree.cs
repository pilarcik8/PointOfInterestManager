using System.Collections.Generic;
using AUS2_S1.KD_Tree;

namespace AUS2_S1.KD_Tree
{
    // Vygenerovane / upravene pomocou AI - Kapitola 17 (kostra), 24 (refaktor na KDKey)
    public class KDTree<T> where T : IComparable<T>
    {
        public KDNode<T>? Root { get; set; }
        public int K { get; set; }
        public int MaxDepth { get; set; }

        // Vygenerovane / upravene pomocou AI - Kapitola 27 (pridanie maxDepth pre Tester)
        public KDTree(int k, int maxDepth = int.MaxValue)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("Max depth is negative");
            }
            Root = null;
            K = k;
            MaxDepth = maxDepth;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 24, Kapitola 27
        public void Insert(T data, LinkedList<KDKey<T>> keys)
        {
            if (keys.Count != K)
            {
                throw new ArgumentException($"Pocet klucov ({keys.Count}) sa nezhoduje s dimenziou stromu ({K}).");
            }

            if (Root == null)
            {
                Root = new KDNode<T>(data, keys);
                return;
            }

            KDNode<T> current = Root;
            int depth = 0;

            while (true)
            {
                int dimension = depth % K;

                // dosiahnuta max hlbka -> dame do listu
                if (depth == MaxDepth)
                {
                    current.Data.AddLast(data);
                    return;
                }

                // Skontroluj ci sa vsetky kluce zhoduju (duplikat)
                if (AllKeysMatch(keys, current.Keys))
                {
                    current.Data.AddLast(data);
                    return;
                }

                int cmp = keys.ElementAt(dimension).CompareTo(current.Keys.ElementAt(dimension));

                if (cmp < 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = new KDNode<T>(data, keys);
                        return;
                    }
                    current = current.Left;
                }
                else
                {
                    // Hodnoty vacsie alebo rovne idu do praveho podstromu
                    if (current.Right == null)
                    {
                        current.Right = new KDNode<T>(data, keys);
                        return;
                    }
                    current = current.Right;
                }

                depth++;
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 22, 28
        public KDNode<T>? Find(LinkedList<KDKey<T>> targetKeys)
        {
            if (targetKeys.Count != K)
            {
                throw new ArgumentException($"Pocet klucov ({targetKeys.Count}) sa nezhoduje s dimenziou stromu ({K}).");
            }

            KDNode<T>? current = Root;
            int depth = 0;

            while (current != null)
            {
                if (AllKeysMatch(targetKeys, current.Keys))
                {
                    return current;
                }

                int dimension = depth % K;
                int cmp = targetKeys.ElementAt(dimension).CompareTo(current.Keys.ElementAt(dimension));

                if (cmp < 0)
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }

                depth++;
            }

            return null;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 22, 28, 29
        public LinkedList<KDNode<T>> FindRange(LinkedList<KDKey<T>> minKeys, LinkedList<KDKey<T>> maxKeys)
        {
            var result = new LinkedList<KDNode<T>>();
            if (Root == null)
            {
                return result;
            }

            var nodesToVisit = new LinkedList<(KDNode<T> node, int depth)>();
            nodesToVisit.AddLast((Root, 0));

            while (nodesToVisit.Count > 0)
            {
                var (current, depth) = nodesToVisit.Last!.Value;
                nodesToVisit.RemoveLast();

                // Skontrolujeme, ci aktualny uzol lezi v intervale vo vsetkych dimenziach
                bool inside = true;
                for (int d = 0; d < K; d++)
                {
                    if (current.Keys.ElementAt(d).CompareTo(minKeys.ElementAt(d)) < 0 ||
                        current.Keys.ElementAt(d).CompareTo(maxKeys.ElementAt(d)) > 0)
                    {
                        inside = false;
                        break;
                    }
                }

                if (inside)
                {
                    result.AddLast(current);
                }

                int dimension = depth % K;

                // Do praveho podstromu vetvime len vtedy, ak max hodnota v danej dimenzii >= hodnote uzla
                if (current.Right != null && maxKeys.ElementAt(dimension).CompareTo(current.Keys.ElementAt(dimension)) >= 0)
                {
                    nodesToVisit.AddLast((current.Right, depth + 1));
                }

                // Do laveho podstromu vetvime len vtedy, ak min hodnota v danej dimenzii <= hodnote uzla
                if (current.Left != null && minKeys.ElementAt(dimension).CompareTo(current.Keys.ElementAt(dimension)) <= 0)
                {
                    nodesToVisit.AddLast((current.Left, depth + 1));
                }
            }

            return result;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 24
        private bool AllKeysMatch(LinkedList<KDKey<T>> keysA, LinkedList<KDKey<T>> keysB)
        {
            for (int d = 0; d < K; d++)
            {
                if (keysA.ElementAt(d).CompareTo(keysB.ElementAt(d)) != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}