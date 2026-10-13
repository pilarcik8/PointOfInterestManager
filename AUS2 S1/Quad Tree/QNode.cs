// Made by Gemini 3.8 Flash
namespace AUS2_S1
{
    public class QNode<T>(double topLeftX, double topLeftY, double bottomRightX, double bottomRightY)
    {
        public double TopLeftX { get; set; } = topLeftX;
        public double TopLeftY { get; set; } = topLeftY;
        public double BottomRightX { get; set; } = bottomRightX;
        public double BottomRightY { get; set; } = bottomRightY;
        // (TL, TR, BL, BR)
        public QNode<T>[] Children { get; set; } = new QNode<T>[4];
        public LinkedList<T> Data { get; set; } = new LinkedList<T>();

        public QNode<T>[] Split()
        {
            double midX = (TopLeftX + BottomRightX) / 2;
            double midY = (TopLeftY + BottomRightY) / 2;

            return new QNode<T>[]
            {
                new QNode<T>(TopLeftX, TopLeftY, midX, midY),         // TL
                new QNode<T>(midX, TopLeftY, BottomRightX, midY),     // TR
                new QNode<T>(TopLeftX, midY, midX, BottomRightY),     // BL
                new QNode<T>(midX, midY, BottomRightX, BottomRightY)  // BR
            };
        }
    }
}