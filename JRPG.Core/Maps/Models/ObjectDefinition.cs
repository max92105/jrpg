using System.Collections.Generic;
using System.Windows;

namespace JRPG.Core.Maps.Models
{
    public class ObjectDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // e.g., "Props", "Furniture", "NPC"
        public int Width { get; set; }
        public int Height { get; set; }

        public List<TileGroupEntry> Tiles { get; set; } = new List<TileGroupEntry>();
        public List<Point> CollisionPolygon { get; set; } = new List<Point>();

        public bool IsNpc { get; set; } = false;

        // Optional metadata
        public Dictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();
    }
}
