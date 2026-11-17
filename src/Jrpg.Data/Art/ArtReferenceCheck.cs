using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Cinematics;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Art;

/// <summary>
/// Checks that the art content names exists (ADR 0009). It runs at startup, apart from the content loader, so content
/// tests don't need art.
/// </summary>
public static partial class ArtReferenceCheck
{
    /// <summary>
    /// Every tilesheet, the player's and every NPC object's field sprite, every dialogue portrait, every cinematic's
    /// still, every party member's portrait, every item's icon, every skill icon named (a skill may have none), every
    /// battle sprite, every enemy's portrait, every attack effect, every strip a skill or item names for itself
    /// (decision 709), every strip and swing <c>effect-strips.json</c> lists (ADR 0048), every encounter's own
    /// background, every map's battle background and every animated background's sheet exist as PNGs, every tile's cell
    /// and its animation's last frame lie inside its sheet, every effect strip is whole square frames and its listed
    /// hit frame one of them, every animated enemy's battle strip splits into its loop's frames (ADR 0047), every field
    /// sprite holds the frames its record needs (ADR 0043), every background is at most
    /// <see cref="TextureLimit.MaxSide"/> a side (decision 895) and every animated background's sheet holds its
    /// record's grid (ADR 0059). The names of skill icons, NPC sprites, stills, battle art and backgrounds must match
    /// their files exactly, case included.
    /// </summary>
    /// <returns>One line per problem, naming the file; empty when the art is complete.</returns>
    public static IReadOnlyList<string> FindProblems(ContentCatalog catalog, string artRoot)
    {
        var problems = new List<string>();
        var sheetSizes = new Dictionary<string, (int Width, int Height)>();
        foreach (var sheet in catalog.Tilesheets.Values.OrderBy(sheet => sheet.Id, StringComparer.Ordinal))
        {
            if (TryReadSize(artRoot, ArtFolders.Tilesheets, sheet.File, problems) is { } size)
            {
                sheetSizes.Add(sheet.Id, size);
            }
        }

        AddTilesOutside(problems, catalog, sheetSizes);
        TryReadSize(artRoot, ArtFolders.FieldSprites, catalog.NewGame.FieldSpriteFile, problems);
        var npcsWithSprites = catalog.Objects.Values
            .Where(definition => definition.FieldSpriteFile is not null)
            .OrderBy(definition => definition.Id, StringComparer.Ordinal);
        var npcSprites = npcsWithSprites
            .Select(definition => (File: definition.FieldSpriteFile!, NamedBy: $"object '{definition.Id}'"))
            .ToList();
        AddMissingByExactName(problems, artRoot, ArtFolders.FieldSprites, npcSprites);
        var allSprites = npcSprites.Prepend((catalog.NewGame.FieldSpriteFile, "the new game"));
        AddSpritesTooSmall(problems, artRoot, catalog.FieldSprites, allSprites);
        var dialoguePortraits = from named in MapEventSteps.Of<DialogueStep>(catalog)
                                where named.Step.Speech.PortraitFile is not null
                                select (named.Step.Speech.PortraitFile!, named.NamedBy);
        AddMissing(problems, artRoot, ArtFolders.Portraits, dialoguePortraits);
        var cinematicStills = CinematicStills.Of(catalog).Select(named => (named.Still.ImageFile, named.NamedBy));
        AddMissingByExactName(problems, artRoot, ArtFolders.Cinematics, cinematicStills);
        var members = catalog.PartyMembers.Values.OrderBy(member => member.Id, StringComparer.Ordinal);
        var memberPortraits = members.Select(member => (member.PortraitFile, $"party member '{member.Id}'"));
        AddMissing(problems, artRoot, ArtFolders.Portraits, memberPortraits);
        var items = catalog.Items.Values.OrderBy(item => item.Id, StringComparer.Ordinal);
        var itemIcons = items.Select(item => (item.IconFile, $"item '{item.Id}'"));
        AddMissing(problems, artRoot, ArtFolders.Icons, itemIcons);
        var skillsWithIcons = catalog.Skills.Values
            .Where(skill => skill.IconFile.Length > 0)
            .OrderBy(skill => skill.Id, StringComparer.Ordinal);
        var skillIcons = skillsWithIcons.Select(skill => (skill.IconFile, $"skill '{skill.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Icons, skillIcons);
        AddBattleArtProblems(problems, catalog, artRoot, members);
        AddBackgroundProblems(problems, catalog, artRoot);
        return problems;
    }

    // A tile's cell, then its animation's last frame (ADR 0039): either past the image would be cropped past its edge.
    private static void AddTilesOutside(List<string> problems, ContentCatalog catalog, Dictionary<string, (int Width, int Height)> sheetSizes)
    {
        foreach (var tile in catalog.Tiles.Values.OrderBy(tile => tile.Id))
        {
            if (!sheetSizes.TryGetValue(tile.SheetId, out var size))
            {
                continue;
            }

            var sheet = catalog.Tilesheets[tile.SheetId];
            var shownPath = ExactFileName.ShownPath(ArtFolders.Tilesheets, sheet.File);
            if (!IsCellInside(tile.Column, tile.Row, sheet, size))
            {
                problems.Add(
                    $"{shownPath}: tile {tile.Id}'s cell ({tile.Column}, {tile.Row}) "
                    + $"lies outside the {size.Width}x{size.Height} image.");
            }
            else if (!IsLastFrameInside(tile, sheet, size))
            {
                problems.Add(
                    $"{shownPath}: tile {tile.Id}'s last animation frame ({LastFrameColumn(tile)}, {tile.Row}) "
                    + $"lies outside the {size.Width}x{size.Height} image.");
            }
        }
    }

    // The walk reads every frame its record names, so a smaller sprite would be cropped past its edge. A sprite without
    // a record is the loader's refusal, not this check's. Counts, not pixel products: a hand-typed frame count near the
    // int limit would wrap back inside the image; the needed size is only said, in long.
    private static void AddSpritesTooSmall(
        List<string> problems,
        string artRoot,
        IReadOnlyDictionary<string, FieldSpriteSheet> sheets,
        IEnumerable<(string File, string NamedBy)> sprites)
    {
        foreach (var (file, namedBy) in sprites)
        {
            if (ReadSize(artRoot, ArtFolders.FieldSprites, file).Size is not { } size || !sheets.TryGetValue(file, out var sheet))
            {
                continue;
            }

            if (!HoldsFrames(sheet, size))
            {
                var neededWidth = sheet.ColumnsNeeded * sheet.FrameWidth;
                var neededHeight = sheet.RowsNeeded * sheet.FrameHeight;
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.FieldSprites, file)}: a {size.Width}x{size.Height} sprite doesn't hold "
                    + $"{sheet.ColumnsNeeded} x {sheet.RowsNeeded} frames of {sheet.FrameWidth} x {sheet.FrameHeight} px "
                    + $"({neededWidth}x{neededHeight}). Named by {namedBy}.");
            }
        }
    }

    /// <summary>
    /// Whether an image this size holds the frames a record needs, by counts (a frame count near the int limit never
    /// wraps); the startup check and the import share it.
    /// </summary>
    internal static bool HoldsFrames(FieldSpriteSheet sheet, (int Width, int Height) size) =>
        size.Width / sheet.FrameWidth >= sheet.ColumnsNeeded && size.Height / sheet.FrameHeight >= sheet.RowsNeeded;

    // Matched by exact name, case included (see ExactFileName).
    private static void AddMissingByExactName(List<string> problems, string artRoot, string folder, IEnumerable<(string File, string NamedBy)> references)
    {
        foreach (var (file, namedBy) in references)
        {
            if ((ExactFileName.FindCaseMismatch(artRoot, folder, file) ?? ReadSize(artRoot, folder, file).Problem) is { } problem)
            {
                problems.Add($"{problem} Named by {namedBy}.");
            }
        }
    }

    // One problem per named file that can't be read, saying what names it so the content can be fixed there.
    private static void AddMissing(List<string> problems, string artRoot, string folder, IEnumerable<(string File, string NamedBy)> references)
    {
        foreach (var (file, namedBy) in references)
        {
            if (ReadSize(artRoot, folder, file).Problem is { } problem)
            {
                problems.Add($"{problem} Named by {namedBy}.");
            }
        }
    }

    /// <summary>
    /// The lowest tile, cut from any sheet drawn from <paramref name="file"/>, whose cell or last animation frame an
    /// image of this size wouldn't hold.
    /// </summary>
    /// <param name="catalog">The sheets and tiles checked.</param>
    /// <param name="file">
    /// The image's file name in the tilesheet art folder; sheets match it ignoring case, as Windows does.
    /// </param>
    /// <param name="size">The new image's width and height in pixels.</param>
    internal static TileDefinition? FirstTileOutside(ContentCatalog catalog, string file, (int Width, int Height) size)
    {
        var sheetsFromFile = catalog.Tilesheets.Values
            .Where(sheet => string.Equals(sheet.File, file, StringComparison.OrdinalIgnoreCase));
        var sheets = sheetsFromFile.ToDictionary(sheet => sheet.Id);
        return catalog.Tiles.Values
            .Where(tile => sheets.TryGetValue(tile.SheetId, out var sheet) && !IsTileInside(tile, sheet, size))
            .MinBy(tile => tile.Id);
    }

    // Its cell and, animated, its last frame.
    private static bool IsTileInside(TileDefinition tile, TilesheetDefinition sheet, (int Width, int Height) size) =>
        IsCellInside(tile.Column, tile.Row, sheet, size) && IsLastFrameInside(tile, sheet, size);

    // Counts, not pixel sums: a hand-edited column or frame count near the int limit would wrap back inside the image
    // (the column and row are never negative, the tile validator refuses that).
    private static bool IsCellInside(int column, int row, TilesheetDefinition sheet, (int Width, int Height) size) =>
        column < sheet.ColumnsIn(size.Width) && row < sheet.RowsIn(size.Height);

    // Frames run rightward on the tile's row, so the last must fit in the columns left of its cell; a still tile is one
    // frame, its own cell.
    private static bool IsLastFrameInside(TileDefinition tile, TilesheetDefinition sheet, (int Width, int Height) size) =>
        FrameCount(tile) <= sheet.ColumnsIn(size.Width) - tile.Column;

    private static int FrameCount(TileDefinition tile) => tile.Animation?.FrameCount ?? 1;

    // For the message only, in long so the text names the frame's column however far it lies.
    private static long LastFrameColumn(TileDefinition tile) => tile.Column + (long)FrameCount(tile) - 1;

    private static (int Width, int Height)? TryReadSize(string artRoot, string folder, string file, List<string> problems)
    {
        var (size, problem) = ReadSize(artRoot, folder, file);
        if (problem is not null)
        {
            problems.Add(problem);
        }

        return size;
    }

    private static ((int Width, int Height)? Size, string? Problem) ReadSize(string artRoot, string folder, string file)
    {
        var path = Path.Combine(artRoot, folder, file);
        if (!File.Exists(path))
        {
            return (null, $"{ExactFileName.ShownPath(folder, file)}: not found in {artRoot}.");
        }

        try
        {
            return (PngSize.Read(path), null);
        }
        catch (ContentValidationException)
        {
            return (null, $"{ExactFileName.ShownPath(folder, file)}: not a PNG image.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return (null, $"{ExactFileName.ShownPath(folder, file)}: {exception.Message}");
        }
    }
}
