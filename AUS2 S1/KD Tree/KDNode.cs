// Made by Gemini 3.8 Flash
namespace AUS2_S1
{
    public class KDNode<T> where T : IKDComparable<T>
    {
        public T Data { get; set; }
        public KDNode<T>? Left { get; set; }
        public KDNode<T>? Right { get; set; }

        public KDNode(T data)
        {
            Data = data;
            Left = null;
            Right = null;
        }
    }
}