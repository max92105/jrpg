using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;

namespace JRPGPlayerDemo
{
    public class MapManager
    {
        public Map CurrentMap { get; private set; } = new Map();
        private double mapTimeElapsed => (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;

        public int Width => CurrentMap.Width;
        public int Height => CurrentMap.Height;

        public void LoadFromFile(string filePath)
        {
            CurrentMap = MapSerializer.Load(filePath);
        }

        public bool IsWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= CurrentMap.Width || y >= CurrentMap.Height)
                return false;

            int tileId = CurrentMap.GetTileIdAt(x, y);
            return CurrentMap.TileDefinitions.TryGetValue(tileId, out var def) ? def.IsWalkable : false;
        }

        public TileTrigger GetTileTrigger(int x, int y)
        {
            return CurrentMap.Triggers.FirstOrDefault(t => t.X == x && t.Y == y);
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
