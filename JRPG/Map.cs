// Map.cs - a dedicated class to manage map tiles and events
using System;
using System.Collections.Generic;
using System.IO;
using JRPG;
using JRPG.Enums;

namespace JRPGPlayerDemo
{
    public class Map
    {
        public int Width { get; private set; } = 16;
        public int Height { get; private set; } = 16;
        public int[,] Tiles { get; private set; }
        public TileType[,] TileTypes { get; private set; }
        public Dictionary<int, string> TileSprites { get; private set; }

        public Map()
        {
            Tiles = new int[Width, Height];
            TileTypes = new TileType[Width, Height];
            TileSprites = new Dictionary<int, string>
            {
                { 0, "Assets/floor.png" },
                { 1, "Assets/wall.png" }
            };

            GenerateTestMap();
        }

        private void GenerateTestMap()
        {
            // fill map with floor
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Tiles[x, y] = 0;

            // add walls around
            for (int i = 0; i < Width; i++)
            {
                Tiles[i, 0] = 1;
                Tiles[i, Height - 1] = 1;
                Tiles[0, i] = 1;
                Tiles[Width - 1, i] = 1;
            }

            // test triggers
            TileTypes[5, 5] = new TileType
            {
                TriggerType = TileTriggerType.Warp,
                TriggerData = "this:1,1"
            };

            TileTypes[7, 7] = new TileType
            {
                TriggerType = TileTriggerType.Battle,
                TriggerData = ""
            };
        }

        public bool IsWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return false;

            return Tiles[x, y] == 0; // floor only walkable for now
        }

        public TileType GetTileTrigger(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return null;

            return TileTypes[x, y];
        }


        public string GetTileSprite(int tileId)
        {
            return TileSprites.ContainsKey(tileId) ? TileSprites[tileId] : null;
        }

        public void LoadFromFile(string map)
        {
            string path = @"Maps/" + map;
            string[] lines = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), path));

            int mode = 0;
            int y = 0;

            foreach (string line in lines)
            {
                if (line == "MAP") { mode = 1; y = 0; continue; }
                if (line == "TRIGGERS") { mode = 2; continue; }

                if (mode == 1)
                {
                    for (int x = 0; x < line.Length; x++)
                        Tiles[x, y] = int.Parse(line[x].ToString());
                    y++;
                }
                else if (mode == 2)
                {
                    string[] parts = line.Split(':');
                    string[] coords = parts[0].Split(',');
                    int xCoord = int.Parse(coords[0]);
                    int yCoord = int.Parse(coords[1]);

                    Enum.TryParse(parts[1], out TileTriggerType trigger);
                    string data = parts[2];

                    if(parts.Length == 4)
                    {
                         data = data + ":" + parts[3];
                    }

                    TileTypes[xCoord, yCoord] = new TileType
                    {
                        Name = trigger.ToString(),
                        IsWalkable = true,
                        Sprite = GetTileSprite(0),
                        TriggerType = trigger,
                        TriggerData = data
                    };
                }
            }
        }
    }
}
