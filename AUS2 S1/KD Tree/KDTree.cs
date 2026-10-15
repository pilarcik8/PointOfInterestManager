// Made by Gemini 3.8 Flash
using AUS2_S1.KD_Tree;

namespace AUS2_S1
{
    public class KDTree<T> where T : IComparable<T>
    {
        public KDNode<T>? Root { get; set; }
        public int K { get; set; }
        public int MaxDepth { get; set; }
        public KDTree(int k, int maxDepth)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentException("Max depth is negative");
            }
            Root = null;
            K = k;
            MaxDepth = maxDepth;
        }

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

        public void Build(List<(T data, LinkedList<KDKey<T>> keys)> items)
        {
            if (items == null || items.Count == 0)
            {
                Root = null;
                return;
            }

            Root = BuildSubtree(new List<(T data, LinkedList<KDKey<T>> keys)>(items), 0);
        }

        private KDNode<T>? BuildSubtree(List<(T data, LinkedList<KDKey<T>> keys)> items, int depth)
        {
            if (items.Count == 0)
            {
                return null;
            }

            int dimension = depth % K;
            items.Sort((a, b) => a.keys.ElementAt(dimension).CompareTo(b.keys.ElementAt(dimension)));

            int medianIndex = items.Count / 2;
            KDNode<T> node = new KDNode<T>(items[medianIndex].data, items[medianIndex].keys);

            List<(T data, LinkedList<KDKey<T>> keys)> leftItems = items.GetRange(0, medianIndex);
            List<(T data, LinkedList<KDKey<T>> keys)> rightItems = items.GetRange(medianIndex + 1, items.Count - (medianIndex + 1));

            node.Left = BuildSubtree(leftItems, depth + 1);
            node.Right = BuildSubtree(rightItems, depth + 1);

            return node;
        }

        // Bodove vyhladavanie
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

        // Intervalove vyhladavanie
        public List<KDNode<T>> FindRange(LinkedList<KDKey<T>> minKeys, LinkedList<KDKey<T>> maxKeys)
        {
            List<KDNode<T>> result = new List<KDNode<T>>();
            FindRangeSubtree(Root, minKeys, maxKeys, 0, result);
            return result;
        }

        public List<KDNode<T>> FindInterval(LinkedList<KDKey<T>> minKeys, LinkedList<KDKey<T>> maxKeys)
        {
            return FindRange(minKeys, maxKeys);
        }

        private void FindRangeSubtree(KDNode<T>? current, LinkedList<KDKey<T>> minKeys, LinkedList<KDKey<T>> maxKeys, int depth, List<KDNode<T>> result)
        {
            if (current == null)
            {
                return;
            }

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
                result.Add(current);
            }

            int dimension = depth % K;

            // Do laveho podstromu vetvime len vtedy, ak min hodnota v danej dimenzii <= hodnote uzla
            if (minKeys.ElementAt(dimension).CompareTo(current.Keys.ElementAt(dimension)) <= 0)
            {
                FindRangeSubtree(current.Left, minKeys, maxKeys, depth + 1, result);
            }

            // Do praveho podstromu vetvime len vtedy, ak max hodnota v danej dimenzii >= hodnote uzla
            if (maxKeys.ElementAt(dimension).CompareTo(current.Keys.ElementAt(dimension)) >= 0)
            {
                FindRangeSubtree(current.Right, minKeys, maxKeys, depth + 1, result);
            }
        }

        // Pomocna metoda: vsetky kluce sa zhoduju
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