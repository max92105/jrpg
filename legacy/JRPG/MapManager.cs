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

        public void LoadFromFile(string file) => SetMap(file, ReadMap(file));

        /// <summary>
        /// Reads and checks a map without switching to it. Throws if the file is missing or broken,
        /// so a bad warp never leaves the game half on one map, half on another.
        /// </summary>
        public static Map ReadMap(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentException("No map file given.");

            string path = AssetPathResolver.GetPath(Path.Combine("Maps", file));
            if (!File.Exists(path))
                throw new FileNotFoundException($"Map \"{file}\" not found.", path);

            var map = MapSerializer.Load(path);
            if (map == null || map.Width <= 0 || map.Height <= 0)
                throw new InvalidDataException($"Map \"{file}\" is empty or has no size.");

            if (map.Objects == null)
                map.Objects = new List<PlacedObject>();

            return map;
        }

        /// <summary>Switches to a map that was already read with ReadMap.</summary>
        public void SetMap(string file, Map map)
        {
            // Opened chests etc. stay as the player left them
            GameSystems.World.ApplyTo(map, file);

            CurrentMap = map;
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
