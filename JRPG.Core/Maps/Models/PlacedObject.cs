using System.Collections.Generic;

namespace JRPG.Core.Maps.Models
{
    public class PlacedObject
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public int Z { get; set; } = 10;

        public Dictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();
    }
}
