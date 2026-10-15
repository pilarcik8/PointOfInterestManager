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
        public bool HasChildren => Children.Any(c => c != null);
        public bool Intersects(double minX, double minY, double maxX, double maxY)
        {
            return TopLeftX <= maxX &&
                   BottomRightX >= minX &&
                   TopLeftY <= maxY &&
                   BottomRightY >= minY;
        }

        // index kvadrantu podla strtinyq
        public int GetQuadrantIndex(double x, double y)
        {
            double midX = (TopLeftX + BottomRightX) / 2;
            double midY = (TopLeftY + BottomRightY) / 2;

            if (x < midX)
            {
                return (y < midY) ? 0 : 2;
            }
            else
            {
                return (y < midY) ? 1 : 3;
            }
        }
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

        public bool IsInside(double x, double y)
        {
            return x >= TopLeftX && x <= BottomRightX &&
                   y >= TopLeftY && y <= BottomRightY;
        }
    }
}