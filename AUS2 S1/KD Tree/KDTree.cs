using System.Collections.Generic;
using AUS2_S1.KD_Tree;

namespace AUS2_S1.KD_Tree
{
    // Vygenerovane / upravene pomocou AI - Kapitola 17 (kostra), 24 (refaktor na KDKey)
    public class KDTree<TKey, TData> where TKey : IComparable<TKey>
    {
        public KDNode<TKey, TData>? Root { get; set; }
        public int K { get; set; }

        // Vygenerovane / upravene pomocou AI - Kapitola 27 (pridanie maxDepth pre Tester)
        public KDTree(int k)
        {
            Root = null;
            K = k;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 24, Kapitola 27, Kapitola 57
        public void Insert(TData data, LinkedList<KDKey<TKey>> keys)
        {
            if (keys.Count != K)
            {
                throw new ArgumentException($"Pocet klucov ({keys.Count}) sa nezhoduje s dimenziou stromu ({K}).");
            }

            if (Root == null)
            {
                Root = new KDNode<TKey, TData>(data, keys);
                return;
            }

            KDNode<TKey, TData> current = Root;
            int depth = 0;

            while (true)
            {
                int dimension = depth % K;

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
                        current.Left = new KDNode<TKey, TData>(data, keys);
                        return;
                    }
                    current = current.Left;
                }
                else
                {
                    // Hodnoty vacsie alebo rovne idu do praveho podstromu
                    if (current.Right == null)
                    {
                        current.Right = new KDNode<TKey, TData>(data, keys);
                        return;
                    }
                    current = current.Right;
                }

                depth++;
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 22, 28
        public KDNode<TKey, TData>? Find(LinkedList<KDKey<TKey>> targetKeys, bool includeDeleted = false)
        {
            if (targetKeys.Count != K)
            {
                throw new ArgumentException($"Pocet klucov ({targetKeys.Count}) sa nezhoduje s dimenziou stromu ({K}).");
            }

            KDNode<TKey, TData>? current = Root;
            int depth = 0;

            while (current != null)
            {
                if (AllKeysMatch(targetKeys, current.Keys))
                {
                    if (!includeDeleted && current.WasDeleted)
                    {
                        return null;
                    }
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

        // Vygenerovane / upravene pomocou AI - Kapitola 58
        public void DeleteNode(KDNode<TKey, TData> node)
        {
            node.DeleteNode();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 58
        public bool DeleteNode(LinkedList<KDKey<TKey>> keys)
        {
            var node = Find(keys, includeDeleted: true);
            if (node != null && !node.WasDeleted)
            {
                node.DeleteNode();
                return true;
            }
            return false;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 22, 28, 29
        public LinkedList<KDNode<TKey, TData>> FindRange(LinkedList<KDKey<TKey>> minKeys, LinkedList<KDKey<TKey>> maxKeys)
        {
            var result = new LinkedList<KDNode<TKey, TData>>();
            if (Root == null)
            {
                return result;
            }

            var nodesToVisit = new LinkedList<(KDNode<TKey, TData> node, int depth)>();
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

                if (inside && !current.WasDeleted)
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

        public LinkedList<KDNode<TKey, TData>> FindRadius(List<KDKey<TKey>> center, KDKey<TKey> radius)
        {
            var maxKeys = new LinkedList<KDKey<TKey>>(center.Select((key, index) => new KDKey<TKey>((dynamic)key.Value - (dynamic)radius.Value)));
            var minKeys = new LinkedList<KDKey<TKey>>(center.Select((key, index) => new KDKey<TKey>((dynamic)key.Value + (dynamic)radius.Value)));

            var squereResult = this.FindRange(maxKeys, minKeys).ToList();

            var filteredResult = new LinkedList<KDNode<TKey, TData>>();

            foreach (var node in squereResult)
            {
                bool inside = true;
                for (int d = 0; d < K; d++)
                {
                    var distance = Math.Abs((dynamic)node.Keys.ElementAt(d).Value - (dynamic)center[d].Value);
                    if (distance > (dynamic)radius.Value)
                    {
                        filteredResult.AddLast(node);
                        inside = false;
                        break;
                    }
                }

                if (!inside)
                {
                    squereResult.Remove(node);
                }
            }

            return filteredResult;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 24
        private bool AllKeysMatch(LinkedList<KDKey<TKey>> keysA, LinkedList<KDKey<TKey>> keysB)
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
