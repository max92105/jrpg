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
    /// tile's cell lies inside its sheet, every effect strip is whole square frames, and every field sprite holds the
    /// walk frames of <see cref="FieldSpriteLayout"/>. The names of skill icons, NPC sprites, stills and battle art
    /// must match their files exactly, case included.
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

        foreach (var tile in catalog.Tiles.Values.OrderBy(tile => tile.Id))
        {
            if (sheetSizes.TryGetValue(tile.SheetId, out var size) && !IsCellInside(tile, catalog.Tilesheets[tile.SheetId], size))
            {
                problems.Add($"{ExactFileName.ShownPath(ArtFolders.Tilesheets, catalog.Tilesheets[tile.SheetId].File)}: tile {tile.Id}'s cell ({tile.Column}, {tile.Row}) lies outside the {size.Width}x{size.Height} image.");
            }
        }

        TryReadSize(artRoot, ArtFolders.FieldSprites, catalog.NewGame.FieldSpriteFile, problems);
        var npcSprites = catalog.Objects.Values.Where(definition => definition.FieldSpriteFile is not null).OrderBy(definition => definition.Id, StringComparer.Ordinal)
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
        AddMissing(problems, artRoot, ArtFolders.Portraits, members.Select(member => (member.PortraitFile, $"party member '{member.Id}'")));
        var items = catalog.Items.Values.OrderBy(item => item.Id, StringComparer.Ordinal);
        AddMissing(problems, artRoot, ArtFolders.Icons, items.Select(item => (item.IconFile, $"item '{item.Id}'")));
        var skillsWithIcons = catalog.Skills.Values.Where(skill => skill.IconFile.Length > 0).OrderBy(skill => skill.Id, StringComparer.Ordinal);
        AddMissingByExactName(problems, artRoot, ArtFolders.Icons, skillsWithIcons.Select(skill => (skill.IconFile, $"skill '{skill.Id}'")));
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, members.Select(member => (member.BattleSpriteFile, $"party member '{member.Id}'")));
        var enemies = catalog.Enemies.Values.OrderBy(enemy => enemy.Id, StringComparer.Ordinal);
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, enemies.Select(enemy => (enemy.BattleSpriteFile, $"enemy '{enemy.Id}'")));
        AddMissingByExactName(problems, artRoot, ArtFolders.Portraits, enemies.Select(enemy => (enemy.PortraitFile, $"enemy '{enemy.Id}'")));
        var attackEffects = members.Select(member => (File: member.AttackEffectFile, NamedBy: $"party member '{member.Id}'"))
            .Concat(enemies.Select(enemy => (File: enemy.AttackEffectFile, NamedBy: $"enemy '{enemy.Id}'")))
            .ToList();
        AddMissingByExactName(problems, artRoot, ArtFolders.Effects, attackEffects);
        AddPartialFrameStrips(problems, artRoot, attackEffects.Select(effect => effect.File));
        var encounters = catalog.Encounters.Values.OrderBy(encounter => encounter.Id, StringComparer.Ordinal);
        AddMissingByExactName(problems, artRoot, ArtFolders.Backgrounds, encounters.Select(encounter => (encounter.BackgroundFile, $"encounter '{encounter.Id}'")));
        return problems;
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
                    $"{ExactFileName.ShownPath(ArtFolders.FieldSprites, file)}: a {size.Width}x{size.Height} sprite doesn't hold {FieldSpriteLayout.WalkFrames} x {FieldSpriteLayout.Rows} frames of {FieldSpriteLayout.FramePixels} px ({neededWidth}x{neededHeight}). Named by {namedBy}.");
            }
        }
    }

    // An effect strip is square frames side by side (decision 57), so its width must be a whole number of its heights.
    private static void AddPartialFrameStrips(List<string> problems, string artRoot, IEnumerable<string> files)
    {
        foreach (var file in files.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (ReadSize(artRoot, ArtFolders.Effects, file).Size is { } size && (size.Height == 0 || size.Width % size.Height != 0))
            {
                problems.Add($"{ExactFileName.ShownPath(ArtFolders.Effects, file)}: a {size.Width}x{size.Height} strip isn't a row of whole square frames.");
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

    /// <summary>The lowest tile, cut from any sheet drawn from <paramref name="file"/>, whose cell an image of this size wouldn't hold.</summary>
    /// <param name="catalog">The sheets and tiles checked.</param>
    /// <param name="file">The image's file name in the tilesheet art folder; sheets match it ignoring case, as Windows does.</param>
    /// <param name="size">The new image's width and height in pixels.</param>
    internal static TileDefinition? FirstTileOutside(ContentCatalog catalog, string file, (int Width, int Height) size)
    {
        var sheets = catalog.Tilesheets.Values.Where(sheet => string.Equals(sheet.File, file, StringComparison.OrdinalIgnoreCase)).ToDictionary(sheet => sheet.Id);
        return catalog.Tiles.Values
            .Where(tile => sheets.TryGetValue(tile.SheetId, out var sheet) && !IsCellInside(tile, sheet, size))
            .MinBy(tile => tile.Id);
    }

    private static bool IsCellInside(TileDefinition tile, TilesheetDefinition sheet, (int Width, int Height) size) =>
        sheet.CellLeft(tile.Column) + sheet.TileWidth <= size.Width && sheet.CellTop(tile.Row) + sheet.TileHeight <= size.Height;

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
