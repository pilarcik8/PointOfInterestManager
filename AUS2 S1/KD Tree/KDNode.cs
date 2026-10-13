// Made by Gemini 3.8 Flash
using AUS2_S1.KD_Tree;

namespace AUS2_S1
{
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
        
        public int CompareTo(KDKey<T> key, int dimension)
        {
            if (Keys.Count == 0)
            {
                throw new InvalidOperationException("Node has no data to compare.");
            }
            return key.CompareTo(Keys.ElementAt(dimension)); //mohol by byt problem ak je vela klucov
        }
    }
}