// Map.cs - a dedicated class to manage map tiles and events
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using JRPG.Core.Enums;
using JRPG.Core.Maps.Models;
using JRPG.Core.Models;

namespace JRPGPlayerDemo
{
    public class Map
    {
        public int Width { get; private set; } = 32;
        public int Height { get; private set; } = 32;
        public int[,] Tiles { get; private set; }
        public TileType[,] TileTypes { get; private set; }
        public Dictionary<int, string> TileSprites { get; private set; }

        public List<PlacedObject> PlacedObjects { get; set; } = new List<PlacedObject>();
        public Dictionary<int, ObjectDefinition> ObjectDefinitions { get; set; } = new Dictionary<int, ObjectDefinition>();
        public Dictionary<int, AnimatedTileDefinition> AnimatedTiles = new Dictionary<int, AnimatedTileDefinition>();

        public Map()
        {
            Tiles = new int[Width, Height];
            TileTypes = new TileType[Width, Height];
            TileSprites = new Dictionary<int, string>
            {
                { 0, "Assets/floor.png" },
                { 1, "Assets/wall.png" }
            };

            ObjectDefinitions[1] = new ObjectDefinition
            {
                SpritePath = "C:\\git\\jrpg\\JRPG\\Assets\\tree.png",
                HasCollision = true,
                CollisionBox = new Rect(16, 8, 32, 48) // blocks bottom half
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
                if (line == "OBJECTS") { mode = 3; y = 0; continue; }

                if (mode == 1)
                {
                    for (int x = 0; x < line.Length; x++)
                        Tiles[x, y] = int.Parse(line[x].ToString());
                    y++;
                }
                else if (mode == 2) // TRIGGERS
                {
                    string[] parts = line.Split(':');
                    string[] coords = parts[0].Split(',');
                    int xCoord = int.Parse(coords[0]);
                    int yCoord = int.Parse(coords[1]);

                    Enum.TryParse(parts[1], out TileTriggerType trigger);
                    string data = parts[2];

                    if (parts.Length == 4)
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
                else if (mode == 3) // OBJECTS
                {
                    var parts = line.Split(':');
                    if (parts.Length >= 2 && int.TryParse(parts[0], out int id))
                    {
                        var coords = parts[1].Split('.');
                        if (coords.Length == 2 &&
                            double.TryParse(coords[0], out double objX) &&
                            double.TryParse(coords[1], out double objY))
                        {
                            bool hasCollision = true;
                            if (parts.Length == 3)
                                bool.TryParse(parts[2], out hasCollision);

                            PlacedObjects.Add(new PlacedObject
                            {
                                Id = id,
                                X = objX,
                                Y = objY,
                                HasCollision = hasCollision
                            });
                        }
                    }
                }
            }
        }
    }
}
