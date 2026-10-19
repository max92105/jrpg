using JRPG.Core.Maps.Models;
using JRPG.Core.Models;
using JRPG.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace JRPG.Core.Maps.IO
{
    /// <summary>
    /// Tilesheet settings (Assets/Tiles/Tilesheets.json): how each sheet image is cut.
    /// A sheet without an entry is cut in MapConstant.TileSize squares (the old behavior).
    /// </summary>
    public static class TilesheetDatabase
    {
        private static readonly Dictionary<string, TilesheetDefinition> sheets =
            new Dictionary<string, TilesheetDefinition>(StringComparer.OrdinalIgnoreCase);

        private static string FilePath => AssetPathResolver.GetTilePath("Tilesheets.json");

        public static IEnumerable<TilesheetDefinition> All => sheets.Values.OrderBy(s => s.Name ?? s.File);

        public static void Load()
        {
            sheets.Clear();

            if (!File.Exists(FilePath))
                return;

            var list = JsonConvert.DeserializeObject<List<TilesheetDefinition>>(File.ReadAllText(FilePath));
            foreach (var sheet in list ?? new List<TilesheetDefinition>())
            {
                if (!string.IsNullOrEmpty(sheet.File))
                    sheets[Path.GetFileName(sheet.File)] = sheet;
            }
        }

        public static void Save()
        {
            var list = sheets.Values.OrderBy(s => s.File).ToList();
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        public static bool Contains(string file) => file != null && sheets.ContainsKey(Path.GetFileName(file));

        public static void AddOrUpdate(TilesheetDefinition sheet)
        {
            sheet.File = Path.GetFileName(sheet.File);
            sheets[sheet.File] = sheet;
        }

        /// <summary>The sheet's settings, or the default 48px grid if it was never configured.</summary>
        public static TilesheetDefinition Get(string file)
        {
            string key = Path.GetFileName(file ?? "");
            if (sheets.TryGetValue(key, out var sheet))
                return sheet;

            return new TilesheetDefinition { File = key, Name = Path.GetFileNameWithoutExtension(key) };
        }

        /// <summary>
        /// Pixel rectangle of a tile inside its sheet, or null if the tile is not from a sheet
        /// or falls outside the image (e.g. after changing the sheet's grid).
        /// Used by the game and the editor so tiles are cut the same way everywhere.
        /// </summary>
        public static Int32Rect? GetSourceRect(TileDefinition tile, int imageWidth, int imageHeight)
        {
            if (string.IsNullOrEmpty(tile.Tilesheet) || !tile.SheetX.HasValue || !tile.SheetY.HasValue)
                return null;

            var sheet = Get(tile.Tilesheet);
            var rect = new Int32Rect(sheet.CellLeft(tile.SheetX.Value), sheet.CellTop(tile.SheetY.Value), sheet.TileWidth, sheet.TileHeight);

            if (rect.X < 0 || rect.Y < 0 || rect.Width <= 0 || rect.Height <= 0 ||
                rect.X + rect.Width > imageWidth || rect.Y + rect.Height > imageHeight)
                return null;

            return rect;
        }
    }
}
