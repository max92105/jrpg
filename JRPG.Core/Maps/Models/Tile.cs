using JRPG.Core.Triggers.Models;

namespace JRPG.Core.Maps.Models
{
    public class Tile
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; } = 0;

        public Trigger Trigger { get; set; }
    }
}
