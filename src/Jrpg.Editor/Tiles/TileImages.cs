using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Tilesheet images, the tiles and cells cut from them and NPCs' standing frames, each loaded once and kept for the
/// editor's lifetime. A tile or cell of a sheet whose image is missing, one past its image's edge, and an NPC without a
/// sprite or a sheet record draw as <see cref="MissingArtMark"/> (decision 442), never cached, so an import that brings
/// the file back draws the art.
/// </summary>
/// <param name="catalog">The tiles, sheets and objects drawn.</param>
/// <param name="artRoot">The <c>art</c> folder.</param>
/// <param name="missingArt">What the art folder lacks right now, from the shell's art status.</param>
internal sealed class TileImages(CatalogDraft catalog, string artRoot, Func<MissingArt> missingArt)
{
    private readonly Dictionary<string, BitmapSource> _sheets = [];
    private readonly Dictionary<(FieldSpriteSheet Sheet, Facing Facing), ImageSource> _spriteFrames = [];

    // Keyed by the sheet's cut as well as the cell, so a re-cut sheet (another record) is cropped anew (design key decision 3).
    private readonly Dictionary<(TilesheetDefinition Sheet, int Column, int Row), ImageSource> _cells = [];

    // The animated tiles by their own cell, for the canvas's many cells per redraw; rebuilt when the catalog changes.
    private Dictionary<SheetCell, int> _animatedTileIds = [];
    private ContentCatalog? _animatedTilesOf;

    public ImageSource TileImageOf(int tileId)
    {
        var tile = catalog.Current.Tiles[tileId];
        return CellImageOf(SheetCell.Of(tile));
    }

    /// <summary>
    /// A sheet cell's image whether or not a tile is cut from it yet, as the object composer's canvas needs.
    /// </summary>
    public ImageSource CellImageOf(SheetCell cell)
    {
        var sheet = catalog.Current.Tilesheets[cell.SheetId];
        if (missingArt().IsSheetMissing(sheet.Id))
        {
            return MissingArtMark.Picture;
        }

        var key = (sheet, cell.Column, cell.Row);
        if (!_cells.TryGetValue(key, out var image))
        {
            var area = new Int32Rect(sheet.CellLeft(cell.Column), sheet.CellTop(cell.Row), sheet.TileWidth, sheet.TileHeight);
            var sheetImage = SheetImageOf(sheet.File);
            if (area.X + area.Width > sheetImage.PixelWidth || area.Y + area.Height > sheetImage.PixelHeight)
            {
                // A smaller image was imported over the sheet's; the cell lies past it.
                return MissingArtMark.Picture;
            }

            image = new CroppedBitmap(sheetImage, area);
            image.Freeze();
            _cells.Add(key, image);
        }

        return image;
    }

    /// <summary>
    /// A tile's frame <paramref name="elapsed"/> into the editor's animation time (ADR 0039); a still tile's own cell.
    /// </summary>
    public ImageSource TileImageAt(int tileId, TimeSpan elapsed)
    {
        var tile = catalog.Current.Tiles[tileId];
        return CellImageOf(new SheetCell(tile.SheetId, tile.ColumnShownAt(elapsed), tile.Row));
    }

    /// <summary>
    /// A sheet cell's picture <paramref name="elapsed"/> into the editor's animation time: an animated tile's own cell
    /// shows its frame, any other cell itself (a frame's cell isn't animated where it is painted).
    /// </summary>
    public ImageSource CellImageAt(SheetCell cell, TimeSpan elapsed) =>
        AnimatedTileIds().TryGetValue(cell, out var tileId) ? TileImageAt(tileId, elapsed) : CellImageOf(cell);

    // Two tiles may share a cell; the lowest id is the one drawn, as CatalogDraft.TileAt finds it.
    private Dictionary<SheetCell, int> AnimatedTileIds()
    {
        if (!ReferenceEquals(_animatedTilesOf, catalog.Current))
        {
            _animatedTilesOf = catalog.Current;
            _animatedTileIds = catalog.Current.Tiles.Values.OrderBy(tile => tile.Id)
                .GroupBy(SheetCell.Of)
                .Where(group => group.First().Animation is not null)
                .ToDictionary(group => group.Key, group => group.First().Id);
        }

        return _animatedTileIds;
    }

    /// <summary>
    /// The standing frame an NPC is drawn with: a facing's first walk frame as its sheet's record cuts it (ADR 0043),
    /// flipped left to right for a facing that mirrors another (ADR 0054). The mark for a sprite whose image is
    /// missing, one without a record (an object made before its sheet was set up) or one whose frame lies past the
    /// image's edge; cached by record and facing, so a changed setup crops anew.
    /// </summary>
    /// <param name="file">The file name in the field-sprite art folder.</param>
    /// <param name="facing">Which way the NPC stands (decision 572).</param>
    public ImageSource FieldSpriteFrameOf(string file, Facing facing)
    {
        if (missingArt().IsSpriteMissing(file) || !catalog.Current.FieldSprites.TryGetValue(file, out var sheet))
        {
            return MissingArtMark.Picture;
        }

        if (!_spriteFrames.TryGetValue((sheet, facing), out var frame))
        {
            var sprite = LoadImage(ArtFolders.FieldSprites, file);
            var standing = sheet.StandingFrame(facing);
            var cell = standing.Cell;
            var area = new Int32Rect(sheet.FrameLeft(cell), sheet.FrameTop(cell), sheet.FrameWidth, sheet.FrameHeight);
            if (area.X + area.Width > sprite.PixelWidth || area.Y + area.Height > sprite.PixelHeight)
            {
                return MissingArtMark.Picture;
            }

            var cropped = new CroppedBitmap(sprite, area);
            frame = standing.IsMirrored ? Mirrored(cropped) : cropped;
            frame.Freeze();
            _spriteFrames.Add((sheet, facing), frame);
        }

        return frame;
    }

    // A mirrored facing's frame, flipped left to right in place (decision 766).
    private static TransformedBitmap Mirrored(BitmapSource frame) => new(frame, new ScaleTransform(-1, 1));

    /// <summary>
    /// Drops an image and the tiles or sprite frames cropped from it, so a file replaced by an import is read again.
    /// </summary>
    /// <param name="file">The file name in the tilesheet or field-sprite art folder, in any case.</param>
    public void Forget(string file)
    {
        foreach (var cached in _sheets.Keys.Where(key => string.Equals(key, file, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            _sheets.Remove(cached);
        }

        foreach (var cropped in _cells.Keys.Where(key => string.Equals(key.Sheet.File, file, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            _cells.Remove(cropped);
        }

        var framesOfFile = _spriteFrames.Keys
            .Where(key => string.Equals(key.Sheet.File, file, StringComparison.OrdinalIgnoreCase));
        foreach (var key in framesOfFile.ToList())
        {
            _spriteFrames.Remove(key);
        }
    }

    /// <param name="file">The sheet's file name in the tilesheet art folder.</param>
    public BitmapSource SheetImageOf(string file)
    {
        if (!_sheets.TryGetValue(file, out var sheet))
        {
            sheet = LoadImage(ArtFolders.Tilesheets, file);
            _sheets.Add(file, sheet);
        }

        return sheet;
    }

    // Asked only for a file its caller found readable (the art status, the browser's missing mark), so an unreadable
    // one is a bug, never a missing image.
    private BitmapImage LoadImage(string folder, string file) =>
        SpriteImageFile.Read(Path.Combine(artRoot, folder, file))
            ?? throw new InvalidOperationException($"{file} isn't a readable PNG, though it was found readable.");
}
