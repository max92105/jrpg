using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>Tilesheet images and the tiles cut from them, each loaded once and kept for the editor's lifetime.</summary>
internal sealed class TileImages(CatalogDraft catalog, string artRoot)
{
    private readonly Dictionary<string, BitmapSource> _sheets = [];
    private readonly Dictionary<int, ImageSource> _tiles = [];

    public ImageSource TileImageOf(int tileId)
    {
        if (!_tiles.TryGetValue(tileId, out var image))
        {
            var tile = catalog.Current.Tiles[tileId];
            var sheet = catalog.Current.Tilesheets[tile.SheetId];
            var cell = new Int32Rect(sheet.CellLeft(tile.Column), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight);
            image = new CroppedBitmap(SheetImageOf(sheet.File), cell);
            image.Freeze();
            _tiles.Add(tileId, image);
        }

        return image;
    }

    /// <param name="file">The sheet's file name in the tilesheet art folder.</param>
    public BitmapSource SheetImageOf(string file)
    {
        if (!_sheets.TryGetValue(file, out var sheet))
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Path.Combine(artRoot, ArtFolders.Tilesheets, file));
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            sheet = bitmap;
            _sheets.Add(file, sheet);
        }

        return sheet;
    }
}
