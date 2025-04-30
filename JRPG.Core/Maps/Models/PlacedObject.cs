namespace JRPG.Core.Maps.Models
{
    public class PlacedObject
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool HasCollision { get; set; }
    }
}
