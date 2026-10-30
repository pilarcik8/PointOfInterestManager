namespace AUS2_S1.KD_Tree
{
    // Vygenerovane / upravene pomocou AI - Kapitola 17, 19, 24
    public class KDNode<TKey, TData> where TKey : IComparable<TKey>
    {
        public LinkedList<TData> Data { get; }
        public KDNode<TKey, TData>? Left { get; set; }
        public KDNode<TKey, TData>? Right { get; set; }
        public LinkedList<KDKey<TKey>> Keys;
        public bool WasDeleted;

        public KDNode(TData data, LinkedList<KDKey<TKey>> keys)
        {
            Data = new LinkedList<TData>();
            Data.AddLast(data);
            Left = null;
            Right = null;
            Keys = keys;
            WasDeleted = false;
        }

        public void DeleteNode()
        {
            WasDeleted = true;
        }
    }
}
