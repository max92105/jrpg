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

        // Shapes are stored in normalized coordinates (0..1) over the object's tile bounds,
        // so (0,0) is the object's top-left corner and (1,1) its bottom-right corner.
        public List<Point> CollisionPolygon { get; set; } = new List<Point>();

        // Parts of the object drawn over the player (tree leaves, roofs), whatever the object's layer
        public List<List<Point>> AbovePlayerZones { get; set; } = new List<List<Point>>();

        public bool IsNpc { get; set; } = false;

        // Optional metadata
        public Dictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();

        /// <summary>Top-left tile and size (in tiles) of the object's tile group.</summary>
        public (int minX, int minY, int width, int height) GetTileBounds()
        {
            if (Tiles == null || Tiles.Count == 0)
                return (0, 0, 1, 1);

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var t in Tiles)
            {
                if (t.X < minX) minX = t.X;
                if (t.Y < minY) minY = t.Y;
                if (t.X > maxX) maxX = t.X;
                if (t.Y > maxY) maxY = t.Y;
            }

            return (minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
    }
}
