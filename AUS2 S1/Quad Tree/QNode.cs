using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AUS2_S1
{
    internal struct Bounds<TPosition>(TPosition minX, TPosition minY, TPosition maxX, TPosition maxY)
        where TPosition : INumber<TPosition>
    {
        public TPosition MinX { get; set; } = minX;
        public TPosition MinY { get; set; } = minY;
        public TPosition MaxX { get; set; } = maxX;
        public TPosition MaxY { get; set; } = maxY;

        public TPosition MidX => (MinX + MaxX) / TPosition.CreateChecked(2);
        public TPosition MidY => (MinY + MaxY) / TPosition.CreateChecked(2);

        // Vygenerovane / upravene pomocou AI - Kapitola 25 (pomenovanie Contains), 33
        public bool IsInside(TPosition x, TPosition y)
        {
            return x >= MinX && x <= MaxX &&
                   y >= MinY && y <= MaxY;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 25, 33
        public bool Intersects(TPosition minX, TPosition minY, TPosition maxX, TPosition maxY)
        {
            return MinX <= maxX &&
                   MaxX >= minX &&
                   MinY <= maxY &&
                   MaxY >= minY;
        }

        public bool CanSplit =>
            MidX != MinX &&
            MidX != MaxX &&
            MidY != MinY &&
            MidY != MaxY;
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

        public bool WasDeleted { get; set; } = false;

        public bool IsSplit => Children.Any(c => c != null);

        // Vygenerovane / upravene pomocou AI - Kapitola 58
        public void DeleteNode()
        {
            WasDeleted = true;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 25 (predtym v QTree), 33
        public int GetQuadrantIndex(TPosition x, TPosition y)
        {
            TPosition two = TPosition.CreateChecked(2);
            TPosition midX = (Bounds.MinX + Bounds.MaxX) / two;
            TPosition midY = (Bounds.MinY + Bounds.MaxY) / two;

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
                new QNode<TData, TPosition>(Bounds.MinX, Bounds.MinY, Bounds.MidX, Bounds.MidY),         // TL
                new QNode<TData, TPosition>(Bounds.MidX, Bounds.MinY, Bounds.MaxX, Bounds.MidY),     // TR
                new QNode<TData, TPosition>(Bounds.MinX, Bounds.MidY, Bounds.MidX, Bounds.MaxY),     // BL
                new QNode<TData, TPosition>(Bounds.MidX, Bounds.MidY, Bounds.MaxX, Bounds.MaxY)  // BR
            ];
        }
    }
}