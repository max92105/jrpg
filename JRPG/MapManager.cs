using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Systems;
using JRPG.Core.Triggers.Models;
using JRPG.Core.Utils;

namespace JRPG
{
    public class MapManager
    {
        public Map CurrentMap { get; private set; } = new Map();
        private double mapTimeElapsed => (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;

        public int Width => CurrentMap.Width;
        public int Height => CurrentMap.Height;

        public void LoadFromFile(string file)
        {
            CurrentMap = MapSerializer.Load(AssetPathResolver.GetPath(Path.Combine("Maps//", file)));

            GameSystems.System.UpdateCurrentMap(file);
        }

        public bool IsWalkable(int x, int y) => IsCellWalkable(CurrentMap, x, y);

        // A cell is walkable when it has at least one real tile (on any layer) and every
        // tile in it is walkable. Id 0 tiles are only placeholders holding a trigger.
        public static bool IsCellWalkable(Map map, int x, int y)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return false;

            bool hasTile = false;
            foreach (var tile in map.GetTilesAt(x, y))
            {
                if (tile.Id == 0) continue;

                if (!TileDatabase.Definitions.TryGetValue(tile.Id, out var def) || !def.IsWalkable)
                    return false;

                hasTile = true;
            }

            return hasTile;
        }

        // Trigger on the cell, from any layer (top-most first)
        public Trigger GetTileTrigger(int x, int y)
        {
            var tiles = CurrentMap.GetTilesAt(x, y);
            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                if (tiles[i].Trigger != null)
                    return tiles[i].Trigger;
            }
            return null;
        }

        public string GetTileSprite(int tileId)
        {
            return TileDatabase.Definitions.TryGetValue(tileId, out var def)
                ? def.Sprite ?? def.Frames?.FirstOrDefault()
                : null;
        }

        public bool IsAnimatedTile(int tileId)
        {
            return TileDatabase.Definitions.TryGetValue(tileId, out var def)
                && def.Frames != null && def.Frames.Length > 0;
        }

        public string GetAnimatedFrame(int tileId)
        {
            if (!TileDatabase.Definitions.TryGetValue(tileId, out var def) || def.Frames == null)
                return null;

            int frameIndex = (int)(mapTimeElapsed / def.FrameDuration) % def.Frames.Length;
            return def.Frames[frameIndex];
        }

        public List<PlacedObject> GetObjects() => CurrentMap.Objects;

        public ObjectDefinition GetObjectDefinition(int id)
        {
            return ObjectDatabase.Definitions.TryGetValue(id, out var def) ? def : null;
        }
    }
}
