using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JRPG.Core.Utils
{
    public static class TileGroupObjectCreator
    {
        public static bool TryCreateObjectFromSelection(string tilesheet, List<Point> selectedPoints, out ObjectDefinition newObj,
            string name = null, string category = null)
        {
            newObj = null;

            if (string.IsNullOrEmpty(tilesheet) || selectedPoints == null || selectedPoints.Count == 0)
                return false;

            var objectTiles = new List<TileGroupEntry>();

            foreach (var point in selectedPoints)
            {
                int sheetX = (int)point.X;
                int sheetY = (int)point.Y;

                var match = TileDatabase.Definitions.FirstOrDefault(kvp =>
                    kvp.Value.SheetX == sheetX &&
                    kvp.Value.SheetY == sheetY &&
                    Path.GetFileName(kvp.Value.Tilesheet ?? "") == Path.GetFileName(tilesheet ?? "")
                );

                int tileId;
                if (!match.Equals(default(KeyValuePair<int, TileDefinition>)))
                {
                    tileId = match.Key;
                }
                else
                {
                    var newDef = new TileDefinition
                    {
                        Id = TileDatabase.GetNextId(),
                        Name = $"{TilesheetDatabase.Get(tilesheet).Name} {sheetX},{sheetY}",
                        IsWalkable = true,
                        Tilesheet = Path.GetFileName(tilesheet),
                        SheetX = sheetX,
                        SheetY = sheetY
                    };

                    TileDatabase.AddOrUpdate(newDef);
                    tileId = newDef.Id;
                }

                objectTiles.Add(new TileGroupEntry
                {
                    X = sheetX,
                    Y = sheetY,
                    TileId = tileId,
                    Z = 0
                });
            }

            TileDatabase.Save();

            int newId = ObjectDatabase.GetNextId();
            newObj = new ObjectDefinition
            {
                Id = newId,
                Name = string.IsNullOrWhiteSpace(name) ? $"Object {newId}" : name,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category,
                Width = 1,
                Height = 1,
                Tiles = objectTiles
            };

            ObjectDatabase.AddOrUpdate(newObj);
            ObjectDatabase.Save();

            return true;
        }
    }
}
