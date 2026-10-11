// Made by Claude Opus 4.6
namespace AUS2_S1
{
    public class Point<T>
    {
        public double X { get; }
        public double Y { get; }
        public T Data { get; }

        public Point(T data, Func<T, (double x, double y)> getPosition)
        {
            Data = data;
            var pos = getPosition(data);
            X = pos.x;
            Y = pos.y;
        }
    }
}

