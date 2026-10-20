// Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash)
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AUS2_S1
{
    // Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash)
    public class QNode<TData, TPosition>(TPosition topLeftX, TPosition topLeftY, TPosition bottomRightX, TPosition bottomRightY)
        where TPosition : INumber<TPosition>
    {
        public TPosition TopLeftX { get; set; } = topLeftX;
        public TPosition TopLeftY { get; set; } = topLeftY;
        public TPosition BottomRightX { get; set; } = bottomRightX;
        public TPosition BottomRightY { get; set; } = bottomRightY;
        // (TL, TR, BL, BR)
        public QNode<TData, TPosition>[] Children { get; set; } = new QNode<TData, TPosition>[4];
        public LinkedList<TData> Data { get; set; } = new LinkedList<TData>();
        public bool HasChildren => Children.Any(c => c != null);

        public bool Intersects(TPosition minX, TPosition minY, TPosition maxX, TPosition maxY)
        {
            return TopLeftX <= maxX &&
                   BottomRightX >= minX &&
                   TopLeftY <= maxY &&
                   BottomRightY >= minY;
        }

        // index kvadrantu podla stredu
        public int GetQuadrantIndex(TPosition x, TPosition y)
        {
            TPosition two = TPosition.CreateChecked(2);
            TPosition midX = (TopLeftX + BottomRightX) / two;
            TPosition midY = (TopLeftY + BottomRightY) / two;

            if (x < midX)
            {
                return (y < midY) ? 0 : 2;
            }
            else
            {
                return (y < midY) ? 1 : 3;
            }
        }

        public QNode<TData, TPosition>[] Split()
        {
            TPosition two = TPosition.CreateChecked(2);
            TPosition midX = (TopLeftX + BottomRightX) / two;
            TPosition midY = (TopLeftY + BottomRightY) / two;

            return
            [
                new QNode<TData, TPosition>(TopLeftX, TopLeftY, midX, midY),         // TL
                new QNode<TData, TPosition>(midX, TopLeftY, BottomRightX, midY),     // TR
                new QNode<TData, TPosition>(TopLeftX, midY, midX, BottomRightY),     // BL
                new QNode<TData, TPosition>(midX, midY, BottomRightX, BottomRightY)  // BR
            ];
        }

        public bool IsInside(TPosition x, TPosition y)
        {
            return x >= TopLeftX && x <= BottomRightX &&
                   y >= TopLeftY && y <= BottomRightY;
        }
    }

    // Pre spatnu kompatibilitu: QNode s jednym parametrom pouzije double
    public class QNode<TData>(double topLeftX, double topLeftY, double bottomRightX, double bottomRightY)
        : QNode<TData, double>(topLeftX, topLeftY, bottomRightX, bottomRightY)
    {
    }
}