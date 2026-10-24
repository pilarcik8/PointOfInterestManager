using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AUS2_S1
{
    internal struct Bounds<TPosition>(TPosition topLeftX, TPosition topLeftY, TPosition bottomRightX, TPosition bottomRightY)
        where TPosition : INumber<TPosition>
    {
        public TPosition TopLeftX { get; set; } = topLeftX;
        public TPosition TopLeftY { get; set; } = topLeftY;
        public TPosition BottomRightX { get; set; } = bottomRightX;
        public TPosition BottomRightY { get; set; } = bottomRightY;

        public TPosition MidX => (TopLeftX + BottomRightX) / TPosition.CreateChecked(2);
        public TPosition MidY => (TopLeftY + BottomRightY) / TPosition.CreateChecked(2);

        // Vygenerovane / upravene pomocou AI - Kapitola 25 (pomenovanie Contains), 33
        public bool IsInside(TPosition x, TPosition y)
        {
            return x >= TopLeftX && x <= BottomRightX &&
                   y >= TopLeftY && y <= BottomRightY;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 25, 33
        public bool Intersects(TPosition minX, TPosition minY, TPosition maxX, TPosition maxY)
        {
            return TopLeftX <= maxX &&
                   BottomRightX >= minX &&
                   TopLeftY <= maxY &&
                   BottomRightY >= minY;
        }

        public bool CanSplit =>
            MidX != TopLeftX &&
            MidX != BottomRightX &&
            MidY != TopLeftY &&
            MidY != BottomRightY;
    }

    // Vygenerovane / upravene pomocou AI - Kapitola 33 (Zavedenie TPosition a INumber)
    public class QNode<TData, TPosition>(TPosition topLeftX, TPosition topLeftY, TPosition bottomRightX, TPosition bottomRightY)
        where TPosition : INumber<TPosition>
    {
        internal Bounds<TPosition> Bounds { get; set; } = new(topLeftX, topLeftY, bottomRightX, bottomRightY);
        
        // (TL, TR, BL, BR)
        // použujeme až keď máme 2+ synov
        public QNode<TData, TPosition>[] Children { get; set; } = new QNode<TData, TPosition>[4];
        
        // overlow + children
        public LinkedList<TData> Data { get; set; } = new LinkedList<TData>();

        public bool IsSplit => Children.Any(c => c != null);

        // Vygenerovane / upravene pomocou AI - Kapitola 25 (predtym v QTree), 33
        public int GetQuadrantIndex(TPosition x, TPosition y)
        {
            TPosition two = TPosition.CreateChecked(2);
            TPosition midX = (Bounds.TopLeftX + Bounds.BottomRightX) / two;
            TPosition midY = (Bounds.TopLeftY + Bounds.BottomRightY) / two;

            if (x < midX)
            {
                return (y < midY) ? 0 : 2;
            }
            else
            {
                return (y < midY) ? 1 : 3;
            }
        }

        // Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash) - Kapitola 8, 33
        public QNode<TData, TPosition>[]? Split()
        {
            if (!Bounds.CanSplit)
            {
                return null;
            }

            return
            [
                new QNode<TData, TPosition>(Bounds.TopLeftX, Bounds.TopLeftY, Bounds.MidX, Bounds.MidY),         // TL
                new QNode<TData, TPosition>(Bounds.MidX, Bounds.TopLeftY, Bounds.BottomRightX, Bounds.MidY),     // TR
                new QNode<TData, TPosition>(Bounds.TopLeftX, Bounds.MidY, Bounds.MidX, Bounds.BottomRightY),     // BL
                new QNode<TData, TPosition>(Bounds.MidX, Bounds.MidY, Bounds.BottomRightX, Bounds.BottomRightY)  // BR
            ];
        }
    }
}