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
        public HashSet<int> UsedTileIds => Tiles.Select(t => t.Id).Distinct().ToHashSet();

        public List<PlacedObject> Objects { get; set; } = new List<PlacedObject>();
        public Dictionary<int, ObjectDefinition> ObjectDefinitions { get; set; } = new Dictionary<int, ObjectDefinition>();

        // === GET ALL LAYERS ===
        public IEnumerable<Tile> GetTilesAt(int x, int y)
        {
            return Tiles.Where(t => t.X == x && t.Y == y).OrderBy(t => t.Z);
        }

        // === GET FIRST TILE ON Z=0 ===
        public int GetTileIdAt(int x, int y)
        {
            return Tiles.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == 0)?.Id ?? 0;
        }

        public Tile GetTileAt(int x, int y)
        {
            return Tiles.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == 0);
        }

        // === SET TILE AT Z=0 ===
        public void SetTile(int x, int y, int id)
        {
            SetTile(x, y, 0, id);
        }

        // === SET TILE AT ANY Z ===
        public void SetTile(int x, int y, int z, int id)
        {
            var tile = Tiles.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == z);
            if (tile != null)
                tile.Id = id;
            else
                Tiles.Add(new Tile { X = x, Y = y, Z = z, Id = id });
        }

        public void RemoveTileAt(int x, int y, int z = 0)
        {
            Tiles.RemoveAll(t => t.X == x && t.Y == y && t.Z == z);
        }
    }
}
