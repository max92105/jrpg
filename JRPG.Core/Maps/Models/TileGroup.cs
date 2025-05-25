using System.Collections.Generic;
using System.Windows;

namespace JRPG.Core.Maps.Models
{
    public class TileGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TileGroupEntry> Tiles { get; set; } = new List<TileGroupEntry>();

        public List<Point> CollisionPolygon { get; set; } = new List<Point>();
    }
}
