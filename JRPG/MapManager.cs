using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Triggers;
using JRPG.Core.Triggers.Models;
using JRPG.Core.Utils;

namespace JRPGPlayerDemo
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
        }

        public bool IsWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= CurrentMap.Width || y >= CurrentMap.Height)
                return false;

            int tileId = CurrentMap.GetTileIdAt(x, y);
            return CurrentMap.TileDefinitions.TryGetValue(tileId, out var def) ? def.IsWalkable : false;
        }

        public Trigger GetTileTrigger(int x, int y)
        {
            Tile tile = CurrentMap.GetTileAt(x, y);
            return tile?.Trigger;
        }

        public string GetTileSprite(int tileId)
        {
            return CurrentMap.TileDefinitions.TryGetValue(tileId, out var def)
                ? def.Sprite ?? def.Frames?.FirstOrDefault()
                : null;
        }

        public bool IsAnimatedTile(int tileId)
        {
            return CurrentMap.TileDefinitions.TryGetValue(tileId, out var def)
                && def.Frames != null && def.Frames.Length > 0;
        }

        public string GetAnimatedFrame(int tileId)
        {
            if (!CurrentMap.TileDefinitions.TryGetValue(tileId, out var def) || def.Frames == null)
                return null;

            int frameIndex = (int)(mapTimeElapsed / def.FrameDuration) % def.Frames.Length;
            return def.Frames[frameIndex];
        }

        public List<PlacedObject> GetObjects() => CurrentMap.Objects;

        public ObjectDefinition GetObjectDefinition(int id)
        {
            return CurrentMap.ObjectDefinitions.TryGetValue(id, out var def) ? def : null;
        }
    }
}
