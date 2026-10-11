// Made by Gemini 3.8 Flash
namespace AUS2_S1
{
    public class KDTree<T> where T : IKDComparable<T>
    {
        public KDNode<T>? Root { get; set; }
        public int K { get; set; }

        public KDTree(int k)
        {
            Root = null;
            K = k;
        }

        public void Insert(T data)
        {
            if (Root == null)
            {
                Root = new KDNode<T>(data);
                return;
            }

            KDNode<T> current = Root;
            int depth = 0;

            while (true)
            {
                int dimension = depth % K;
                int cmp = data.CompareTo(current.Data, dimension);

                if (cmp < 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = new KDNode<T>(data);
                        return;
                    }
                    current = current.Left;
                }
                else
                {
                    // Hodnoty vacsie alebo rovne idu do praveho podstromu
                    if (current.Right == null)
                    {
                        current.Right = new KDNode<T>(data);
                        return;
                    }
                    current = current.Right;
                }

                depth++;
            }
        }

        // Vlozenie / vybudovanie stromu z vopred znamej mnoziny zaznamov pomocou medianov
        public void Insert(List<T> items)
        {
            Build(items);
        }

        public void Build(List<T> items)
        {
            if (items == null || items.Count == 0)
            {
                Root = null;
                return;
            }

            Root = BuildSubtree(new List<T>(items), 0);
        }

        private KDNode<T>? BuildSubtree(List<T> items, int depth)
        {
            if (items.Count == 0)
            {
                return null;
            }

            int dimension = depth % K;
            items.Sort((a, b) => a.CompareTo(b, dimension));

            int medianIndex = items.Count / 2;
            KDNode<T> node = new KDNode<T>(items[medianIndex]);

            List<T> leftItems = items.GetRange(0, medianIndex);
            List<T> rightItems = items.GetRange(medianIndex + 1, items.Count - (medianIndex + 1));

            node.Left = BuildSubtree(leftItems, depth + 1);
            node.Right = BuildSubtree(rightItems, depth + 1);

            return node;
        }

        // Bodove vyhladavanie
        public T? Find(T target)
        {
            KDNode<T>? current = Root;
            int depth = 0;

            while (current != null)
            {
                bool matchesAll = true;
                for (int d = 0; d < K; d++)
                {
                    if (target.CompareTo(current.Data, d) != 0)
                    {
                        matchesAll = false;
                        break;
                    }
                }

                if (matchesAll)
                {
                    return current.Data;
                }

                int dimension = depth % K;
                int cmp = target.CompareTo(current.Data, dimension);

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

            return default;
        }

        // Intervalove vyhladavanie
        public List<T> FindRange(T min, T max)
        {
            List<T> result = new List<T>();
            FindRangeSubtree(Root, min, max, 0, result);
            return result;
        }

        public List<T> FindInterval(T min, T max)
        {
            return FindRange(min, max);
        }

        private void FindRangeSubtree(KDNode<T>? current, T min, T max, int depth, List<T> result)
        {
            if (current == null)
            {
                return;
            }

            // Skontrolujeme, ci aktualny uzol lezi v intervale vo vsetkych dimenziach
            bool inside = true;
            for (int d = 0; d < K; d++)
            {
                if (current.Data.CompareTo(min, d) < 0 || current.Data.CompareTo(max, d) > 0)
                {
                    inside = false;
                    break;
                }
            }

            if (inside)
            {
                result.Add(current.Data);
            }

            int dimension = depth % K;

            // Do laveho podstromu vetvime len vtedy, ak min hodnota v danej dimenzii <= hodnote uzla
            if (min.CompareTo(current.Data, dimension) <= 0)
            {
                FindRangeSubtree(current.Left, min, max, depth + 1, result);
            }

            // Do praveho podstromu vetvime len vtedy, ak max hodnota v danej dimenzii >= hodnote uzla
            if (max.CompareTo(current.Data, dimension) >= 0)
            {
                FindRangeSubtree(current.Right, min, max, depth + 1, result);
            }
        }
    }
}