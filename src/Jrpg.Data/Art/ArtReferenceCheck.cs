using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Art;

/// <summary>
/// Checks that the art content names exists (ADR 0009). It runs at startup, apart from the content loader, so content
/// tests don't need art.
/// </summary>
public static class ArtReferenceCheck
{
    /// <summary>
    /// Every tilesheet, the player's and every NPC object's field sprite, every dialogue portrait, every cutscene
    /// still, every party member's portrait, every item's icon, every skill icon named (a skill may have none), every
    /// battle sprite, every enemy's portrait, every attack effect and every encounter's background exist as PNGs, every
    /// tile's cell and its animation's last frame lie inside its sheet, every effect strip is whole square frames, and
    /// every field sprite holds the walk frames of <see cref="FieldSpriteLayout"/>. The names of skill icons, NPC
    /// sprites, stills and battle art must match their files exactly, case included.
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
        AddSpritesWithoutWalkFrames(problems, artRoot, npcSprites.Prepend((catalog.NewGame.FieldSpriteFile, "the new game")));
        var dialoguePortraits = from named in MapEventSteps.Of<DialogueStep>(catalog)
                                where named.Step.Speech.PortraitFile is not null
                                select (named.Step.Speech.PortraitFile!, named.NamedBy);
        AddMissing(problems, artRoot, ArtFolders.Portraits, dialoguePortraits);
        var stills = MapEventSteps.Of<StillStep>(catalog).Select(named => (named.Step.ImageFile, named.NamedBy));
        AddMissingByExactName(problems, artRoot, ArtFolders.Cinematics, stills);
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
        var memberBattlers = members.Select(member => (member.BattleSpriteFile, $"party member '{member.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, memberBattlers);
        var enemies = catalog.Enemies.Values.OrderBy(enemy => enemy.Id, StringComparer.Ordinal);
        var enemyBattlers = enemies.Select(enemy => (enemy.BattleSpriteFile, $"enemy '{enemy.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, enemyBattlers);
        var enemyPortraits = enemies.Select(enemy => (enemy.PortraitFile, $"enemy '{enemy.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Portraits, enemyPortraits);
        var attackEffects = members.Select(member => (File: member.AttackEffectFile, NamedBy: $"party member '{member.Id}'"))
            .Concat(enemies.Select(enemy => (File: enemy.AttackEffectFile, NamedBy: $"enemy '{enemy.Id}'")))
            .ToList();
        AddMissingByExactName(problems, artRoot, ArtFolders.Effects, attackEffects);
        AddPartialFrameStrips(problems, artRoot, attackEffects.Select(effect => effect.File));
        var encounters = catalog.Encounters.Values.OrderBy(encounter => encounter.Id, StringComparer.Ordinal);
        var backgrounds = encounters.Select(encounter => (encounter.BackgroundFile, $"encounter '{encounter.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Backgrounds, backgrounds);
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

    // The walk animation reads every frame of the grid, so a smaller sprite would be cropped past its edge.
    private static void AddSpritesWithoutWalkFrames(List<string> problems, string artRoot, IEnumerable<(string File, string NamedBy)> sprites)
    {
        const int neededWidth = FieldSpriteLayout.WalkFrames * FieldSpriteLayout.FramePixels;
        const int neededHeight = FieldSpriteLayout.Rows * FieldSpriteLayout.FramePixels;
        foreach (var (file, namedBy) in sprites)
        {
            if (ReadSize(artRoot, ArtFolders.FieldSprites, file).Size is { } size && !FieldSpriteLayout.Holds(size))
            {
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.FieldSprites, file)}: a {size.Width}x{size.Height} sprite doesn't hold "
                    + $"{FieldSpriteLayout.WalkFrames} x {FieldSpriteLayout.Rows} frames of {FieldSpriteLayout.FramePixels} px "
                    + $"({neededWidth}x{neededHeight}). Named by {namedBy}.");
            }
        }
    }

    // An effect strip is square frames side by side (decision 57), so its width must be a whole number of its heights.
    private static void AddPartialFrameStrips(List<string> problems, string artRoot, IEnumerable<string> files)
    {
        foreach (var file in files.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (ReadSize(artRoot, ArtFolders.Effects, file).Size is not { } size)
            {
                continue;
            }

            var isRowOfSquares = size.Height > 0 && size.Width % size.Height == 0;
            if (!isRowOfSquares)
            {
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.Effects, file)}: a {size.Width}x{size.Height} strip "
                    + "isn't a row of whole square frames.");
            }
        }
    }

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
