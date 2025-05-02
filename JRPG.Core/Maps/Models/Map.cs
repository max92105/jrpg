using JRPG.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Core.Maps.Models
{
    public class Map
    {
        public string Name { get; set; } = "Untitled";
        public int Width { get; set; }
        public int Height { get; set; }

        public List<Tile> Tiles { get; set; } = new List<Tile>();
        public List<PlacedObject> Objects { get; set; } = new List<PlacedObject>();

        public Dictionary<int, TileDefinition> TileDefinitions { get; set; } = new Dictionary<int, TileDefinition>();
        public Dictionary<int, ObjectDefinition> ObjectDefinitions { get; set; } = new Dictionary<int, ObjectDefinition>();

        public int GetTileIdAt(int x, int y)
        {
            return Tiles.FirstOrDefault(t => t.X == x && t.Y == y)?.Id ?? 0; // default to 0
        }

        public Tile GetTileAt(int x, int y)
        {
            return Tiles.FirstOrDefault(t => t.X == x && t.Y == y);
        }

        public void SetTile(int x, int y, int id)
        {
            var tile = Tiles.FirstOrDefault(t => t.X == x && t.Y == y);
            if (tile != null)
                tile.Id = id;
            else
                Tiles.Add(new Tile { X = x, Y = y, Id = id });
        }
    }
}
