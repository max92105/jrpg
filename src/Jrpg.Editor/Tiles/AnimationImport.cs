using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Tiles > Import animations (decision 469): a Tiled <c>.tsx</c> file matched to the set-up sheet with its image, its
/// animations set as one undo step, or why not. The file is refused whole rather than half imported.
/// </summary>
/// <param name="catalog">The sheets matched and the tiles animated.</param>
internal sealed partial class AnimationImport(CatalogDraft catalog) : ObservableObject
{
    /// <summary>What the last import did, or why it changed nothing.</summary>
    [ObservableProperty]
    public partial string Status { get; private set; } = "";

    public void Import(string tsxPath)
    {
        try
        {
            Status = Apply(TiledTilesetReader.Read(tsxPath), Path.GetFileName(tsxPath));
        }
        catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            Status = exception.Message;
        }
    }

    private string Apply(TiledTileset tileset, string fileName)
    {
        // Windows ignores case in file names, so the match does too. One image may be set up as several sheets: the one
        // with the .tsx's cut is meant, since another cut would put its tile ids on other cells.
        var sheets = catalog.Current.Tilesheets.Values
            .Where(sheet => string.Equals(sheet.File, tileset.ImageFile, StringComparison.OrdinalIgnoreCase))
            .OrderBy(sheet => sheet.Id, StringComparer.Ordinal)
            .ToList();
        if (sheets.Count == 0)
        {
            return $"{fileName}: no sheet has the image {tileset.ImageFile}; set it up first.";
        }

        var sheet = sheets.FirstOrDefault(sheet => (sheet.TileWidth, sheet.TileHeight, sheet.Margin, sheet.Spacing) == (tileset.TileWidth, tileset.TileHeight, tileset.Margin, tileset.Spacing));
        if (sheet is null)
        {
            var first = sheets[0];
            return $"{fileName}: its cut ({tileset.TileWidth}x{tileset.TileHeight} px, margin {tileset.Margin}, spacing {tileset.Spacing}) isn't sheet {first.Id}'s ({first.TileWidth}x{first.TileHeight} px, margin {first.Margin}, spacing {first.Spacing}).";
        }

        if (tileset.Refusal is { } refusal)
        {
            return $"{fileName}: {refusal}";
        }

        if (tileset.Animations.Count == 0)
        {
            return $"{fileName}: no tile is animated.";
        }

        var cells = tileset.Animations.ToDictionary(pair => new SheetCell(sheet.Id, pair.Key.Column, pair.Key.Row), pair => pair.Value);
        var tileIds = catalog.TileIdsOf(cells.Keys);
        catalog.SetTileAnimations(cells.ToDictionary(pair => tileIds[pair.Key], pair => (TileAnimation?)pair.Value));
        return cells.Count == 1 ? $"Animated 1 tile of {sheet.Id}." : $"Animated {cells.Count} tiles of {sheet.Id}.";
    }
}
