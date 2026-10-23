using AUS2_S1.KD_Tree;

namespace AUS2_S1.KD_Tree
{
    // Vygenerovane / upravene pomocou AI - Kapitola 17, 19, 24
    public class KDNode<T> where T : IComparable<T>
    {
        public LinkedList<T> Data { get; }
        public KDNode<T>? Left { get; set; }
        public KDNode<T>? Right { get; set; }

        public LinkedList<KDKey<T>> Keys;

        public KDNode(T data, LinkedList<KDKey<T>> Keys)
        {
            Data = new LinkedList<T>();
            Data.AddLast(data);
            Left = null;
            Right = null;
            this.Keys = Keys;
        }
    }
}