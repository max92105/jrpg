using System.Collections.Generic;
using System.Linq;

namespace JRPG.Core.Maps.Models
{
    public class Map
    {
        private static readonly List<Tile> NoTiles = new List<Tile>();

        public string Name { get; set; } = "Untitled";
        public int Width { get; set; }
        public int Height { get; set; }

        // Random battles while walking (off for towns, houses...). Maps saved before this
        // setting existed don't have it in their file and keep encounters on.
        public bool RandomEncounters { get; set; } = true;

        private List<Tile> tiles = new List<Tile>();
        public List<Tile> Tiles
        {
            get => tiles;
            set
            {
                tiles = value ?? new List<Tile>();
                InvalidateTileIndex();
            }
        }

        public HashSet<int> UsedTileIds => Tiles.Select(t => t.Id).Distinct().ToHashSet();

        public List<PlacedObject> Objects { get; set; } = new List<PlacedObject>();

        // Tiles grouped by cell, each cell sorted by Z. Built lazily from Tiles.
        private Dictionary<(int x, int y), List<Tile>> tileIndex;
        private int indexedTileCount;

        // Call after changing a tile's X, Y or Z directly.
        public void InvalidateTileIndex()
        {
            tileIndex = null;
        }

        private Dictionary<(int x, int y), List<Tile>> GetTileIndex()
        {
            // Count check catches tiles added or removed straight through the Tiles list
            if (tileIndex != null && indexedTileCount == tiles.Count)
                return tileIndex;

            tileIndex = new Dictionary<(int x, int y), List<Tile>>();
            foreach (var tile in tiles)
                AddToIndex(tile);

            indexedTileCount = tiles.Count;
            return tileIndex;
        }

        private void AddToIndex(Tile tile)
        {
            if (!tileIndex.TryGetValue((tile.X, tile.Y), out var cell))
            {
                cell = new List<Tile>();
                tileIndex[(tile.X, tile.Y)] = cell;
            }

            // Insert after any tile with the same Z to keep the order stable
            int i = cell.Count;
            while (i > 0 && cell[i - 1].Z > tile.Z)
                i--;
            cell.Insert(i, tile);
        }

        // === GET ALL LAYERS (sorted by Z) ===
        public IReadOnlyList<Tile> GetTilesAt(int x, int y)
        {
            return GetTileIndex().TryGetValue((x, y), out var cell) ? cell : NoTiles;
        }

        // === GET FIRST TILE ON Z=0 ===
        public int GetTileIdAt(int x, int y)
        {
            return GetTileAt(x, y)?.Id ?? 0;
        }

        public Tile GetTileAt(int x, int y)
        {
            return GetTileAt(x, y, 0);
        }

        public Tile GetTileAt(int x, int y, int z)
        {
            foreach (var tile in GetTilesAt(x, y))
            {
                if (tile.Z == z)
                    return tile;
            }
            return null;
        }

        // === SET TILE AT Z=0 ===
        public void SetTile(int x, int y, int id)
        {
            SetTile(x, y, 0, id);
        }

        // === SET TILE AT ANY Z ===
        public void SetTile(int x, int y, int z, int id)
        {
            var tile = GetTileAt(x, y, z);
            if (tile != null)
                tile.Id = id;
            else
                AddTile(new Tile { X = x, Y = y, Z = z, Id = id });
        }

        public void AddTile(Tile tile)
        {
            GetTileIndex();
            tiles.Add(tile);
            AddToIndex(tile);
            indexedTileCount = tiles.Count;
        }

        public void AddTiles(IEnumerable<Tile> newTiles)
        {
            foreach (var tile in newTiles)
                AddTile(tile);
        }

        public void RemoveTile(Tile tile)
        {
            if (GetTileIndex().TryGetValue((tile.X, tile.Y), out var cell))
                cell.Remove(tile);

            tiles.Remove(tile);
            indexedTileCount = tiles.Count;
        }

        public void RemoveTileAt(int x, int y, int z = 0)
        {
            if (!GetTileIndex().TryGetValue((x, y), out var cell))
                return;

            var removed = cell.Where(t => t.Z == z).ToList();
            if (removed.Count == 0)
                return;

            foreach (var tile in removed)
            {
                cell.Remove(tile);
                tiles.Remove(tile);
            }
            indexedTileCount = tiles.Count;
        }
    }
}
