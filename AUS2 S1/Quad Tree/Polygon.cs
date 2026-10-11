// Made by Claude Opus 4.6
namespace AUS2_S1
{
    public class Polygon
    {
        public double TopLeftX { get; set; }
        public double TopLeftY { get; set; }
        public double BottomRightX { get; set; }
        public double BottomRightY { get; set; }

        public Polygon(double topLeftX, double topLeftY, double bottomRightX, double bottomRightY)
        {
            TopLeftX = topLeftX;
            TopLeftY = topLeftY;
            BottomRightX = bottomRightX;
            BottomRightY = bottomRightY;
        }

        // -1 = other overlaps but is NOT a subset
        //  0 = other has the same position
        //  1 = other is a subset of this polygon
        public int CompareWith(Polygon other)
        {
            if (other.TopLeftX == TopLeftX && other.TopLeftY == TopLeftY &&
                other.BottomRightX == BottomRightX && other.BottomRightY == BottomRightY)
            {
                return 0;
            }

            if (other.TopLeftX >= TopLeftX && other.TopLeftY >= TopLeftY &&
                other.BottomRightX <= BottomRightX && other.BottomRightY <= BottomRightY)
            {
                return 1;
            }

            return -1;
        }

        // Splits polygon into 4 quadrants (TL, TR, BL, BR)
        public Polygon[] Split()
        {
            double midX = (TopLeftX + BottomRightX) / 2;
            double midY = (TopLeftY + BottomRightY) / 2;

            return new Polygon[]
            {
                new Polygon(TopLeftX, TopLeftY, midX, midY),         // TL
                new Polygon(midX, TopLeftY, BottomRightX, midY),     // TR
                new Polygon(TopLeftX, midY, midX, BottomRightY),     // BL
                new Polygon(midX, midY, BottomRightX, BottomRightY)  // BR
            };
        }
    }
}