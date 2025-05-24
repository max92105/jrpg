using System.Collections.Generic;

namespace JRPG.Core.Models
{
    public class TileDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsWalkable { get; set; }

        public string Sprite { get; set; }              // For static tile
        public string[] Frames { get; set; }            // For animation
        public double FrameDuration { get; set; }       // Animation speed

        // New for tilesheet support
        public string Tilesheet { get; set; }           // Path to full tilesheet
        public int? SheetX { get; set; }                // Column index
        public int? SheetY { get; set; }                // Row index

        public List<System.Windows.Point> CollisionPolygon { get; set; }
    }
}
