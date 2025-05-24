using System.Collections.Generic;

namespace JRPG.Core.Maps.Models
{
    public class TileGroup
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TileGroupEntry> Tiles { get; set; } = new List<TileGroupEntry>();
    }
}
