using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Art;

/// <summary>
/// Checks that the art content names exists (ADR 0009). It runs at startup, apart from the content loader, so content
/// tests don't need art.
/// </summary>
public static class ArtReferenceCheck
{
    /// <summary>
    /// Every tilesheet, the field sprite, every dialogue portrait, every party member's portrait, every item's icon,
    /// every skill icon named (a skill may have none), every battle sprite, every enemy's portrait, every attack effect
    /// and every encounter's background exist as PNGs, every tile's cell lies inside its sheet, and every effect strip is
    /// whole square frames. Skill icons and battle art names must match their files exactly, case included.
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
                problems.Add($"{ShownPath(ArtFolders.Tilesheets, catalog.Tilesheets[tile.SheetId].File)}: tile {tile.Id}'s cell ({tile.Column}, {tile.Row}) lies outside the {size.Width}x{size.Height} image.");
            }
        }

        TryReadSize(artRoot, ArtFolders.FieldSprites, catalog.NewGame.FieldSpriteFile, problems);
        var dialoguePortraits = DialoguePortraits(catalog).Select(named => (named.PortraitFile, $"event '{named.Event.Id}' on map '{named.MapId}'"));
        AddMissing(problems, artRoot, ArtFolders.Portraits, dialoguePortraits);
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

    // An effect strip is square frames side by side (decision 57), so its width must be a whole number of its heights.
    private static void AddPartialFrameStrips(List<string> problems, string artRoot, IEnumerable<string> files)
    {
        foreach (var file in files.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (ReadSize(artRoot, ArtFolders.Effects, file).Size is { } size && (size.Height == 0 || size.Width % size.Height != 0))
            {
                problems.Add($"{ShownPath(ArtFolders.Effects, file)}: a {size.Width}x{size.Height} strip isn't a row of whole square frames.");
            }
        }
    }

    // Battle art is matched by exact name, case included: Windows finds "goblin.png" for "Goblin.png", other systems don't.
    private static void AddMissingByExactName(List<string> problems, string artRoot, string folder, IEnumerable<(string File, string NamedBy)> references)
    {
        foreach (var (file, namedBy) in references)
        {
            if ((CaseMismatch(artRoot, folder, file) ?? ReadSize(artRoot, folder, file).Problem) is { } problem)
            {
                problems.Add($"{problem} Named by {namedBy}.");
            }
        }
    }

    private static string? CaseMismatch(string artRoot, string folder, string file)
    {
        var directory = Path.Combine(artRoot, folder);
        if (!Directory.Exists(directory))
        {
            return null;
        }

        var names = Directory.EnumerateFiles(directory).Select(Path.GetFileName).ToList();
        if (names.Contains(file, StringComparer.Ordinal))
        {
            return null;
        }

        var actual = names.FirstOrDefault(name => string.Equals(name, file, StringComparison.OrdinalIgnoreCase));
        return actual is null ? null : $"{ShownPath(folder, file)}: the file is named {actual}; names must match exactly.";
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

    // Every portrait a dialogue step names, with the map and event that name it, in a stable order for the report.
    private static IEnumerable<(string MapId, MapEvent Event, string PortraitFile)> DialoguePortraits(ContentCatalog catalog) =>
        from map in catalog.Maps.Values.OrderBy(map => map.Id, StringComparer.Ordinal)
        from mapEvent in map.Layout.Objects.SelectMany(placed => placed.Events).Concat(map.Layout.CellEvents.Select(cell => cell.Event))
        from step in mapEvent.Steps.OfType<DialogueStep>()
        where step.Speech.PortraitFile is not null
        select (map.Id, mapEvent, step.Speech.PortraitFile!);

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
            return (null, $"{ShownPath(folder, file)}: not found in {artRoot}.");
        }

        try
        {
            return (PngSize.Read(path), null);
        }
        catch (ContentValidationException)
        {
            return (null, $"{ShownPath(folder, file)}: not a PNG image.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return (null, $"{ShownPath(folder, file)}: {exception.Message}");
        }
    }

    private static string ShownPath(string folder, string file) => folder.Length == 0 ? file : $"{folder}/{file}";
}
