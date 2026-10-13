// Made by Gemini 3.8 Flash
namespace AUS2_S1
{
    public class QNode<T>(Polygon boundary)
    {
        public Polygon Boundary { get; set; } = boundary;
        // (TL, TR, BL, BR)

        public QNode<T>[] Children { get; set; } = new QNode<T>[4];


        public List<Polygon> OverflowPolygons { get; set; } = new List<Polygon>();//linkedlist
        public List<Point<T>> Points { get; set; } = new List<Point<T>>(); //zatial iba body a prazdny linkedlist


    }
}