using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Data.Art;

/// <summary>
/// The battle backgrounds the startup check reads (decision 895): every encounter's own and every map's, by exact name
/// and at most <see cref="TextureLimit.MaxSide"/> a side; and every animated background's sheet (ADR 0059), which must
/// also hold its record's grid.
/// </summary>
public static partial class ArtReferenceCheck
{
    /// <summary>
    /// Why an image this size can't hold <paramref name="record"/>'s frames: as many columns as whole frames fit its
    /// width (at least one, so an image narrower than a frame needs one), and every frame's row inside its height. By
    /// counts in <c>long</c>, so a frame count near the int limit never wraps. The startup check and the editor's
    /// Battle backgrounds page read the same rule.
    /// </summary>
    /// <param name="record">The animated background's frame size and count.</param>
    /// <param name="size">The sheet's width and height in pixels.</param>
    /// <returns>The grid the record needs and the one the image holds; null when the image holds every frame.</returns>
    public static string? GridProblemOf(AnimatedBackgroundDefinition record, (int Width, int Height) size)
    {
        long columnsHeld = size.Width / record.FrameWidth;
        long rowsHeld = size.Height / record.FrameHeight;
        var columns = Math.Max(columnsHeld, 1);
        var rowsNeeded = (record.Animation.FrameCount + columns - 1) / columns;
        if (columnsHeld >= 1 && rowsHeld >= rowsNeeded)
        {
            return null;
        }

        return $"{record.Animation.FrameCount} frames of {record.FrameWidth} x {record.FrameHeight} px need "
            + $"{columns} x {rowsNeeded}; the image holds {columnsHeld} x {rowsHeld}";
    }

    private static void AddBackgroundProblems(List<string> problems, ContentCatalog catalog, string artRoot)
    {
        foreach (var (file, namedBy) in NamedBackgrounds(catalog))
        {
            if (BackgroundProblem(artRoot, file) is { } problem)
            {
                problems.Add($"{problem} Named by {namedBy}.");
            }
        }

        var records = catalog.AnimatedBackgrounds.Values.OrderBy(record => record.File, StringComparer.Ordinal);
        foreach (var record in records)
        {
            if (AnimatedBackgroundProblem(artRoot, record) is { } problem)
            {
                problems.Add($"{problem} Named by {ContentCatalogLoader.AnimatedBackgroundsFile} '{record.File}'.");
            }
        }
    }

    // Every encounter's own, then every map's, each in id order.
    private static IEnumerable<(string File, string NamedBy)> NamedBackgrounds(ContentCatalog catalog)
    {
        var encounters = catalog.Encounters.Values
            .Where(encounter => encounter.BackgroundFile is not null)
            .OrderBy(encounter => encounter.Id, StringComparer.Ordinal);
        var encounterBackgrounds = encounters.Select(encounter => (encounter.BackgroundFile!, $"encounter '{encounter.Id}'"));
        var maps = catalog.Maps.Values
            .Select(map => map.Settings)
            .Where(settings => settings.BattleBackgroundFile is not null)
            .OrderBy(settings => settings.Id, StringComparer.Ordinal);
        var mapBackgrounds = maps.Select(settings => (settings.BattleBackgroundFile!, $"map '{settings.Id}'"));
        return encounterBackgrounds.Concat(mapBackgrounds);
    }

    private static string? BackgroundProblem(string artRoot, string file) =>
        ReadBackgroundSize(artRoot, file) switch
        {
            ({ } size, null) => WithShownPath(file, TextureLimit.ProblemOf(size)),
            (_, var problem) => problem,
        };

    // Too large is said before the grid: the game refuses to load the sheet whatever its grid.
    private static string? AnimatedBackgroundProblem(string artRoot, AnimatedBackgroundDefinition record) =>
        ReadBackgroundSize(artRoot, record.File) switch
        {
            ({ } size, null) => WithShownPath(record.File, TextureLimit.ProblemOf(size) ?? GridProblemOf(record, size)),
            (_, var problem) => problem,
        };

    // By exact name first (see ExactFileName): Windows would read a file named in another case.
    private static ((int Width, int Height)? Size, string? Problem) ReadBackgroundSize(string artRoot, string file) =>
        ExactFileName.FindCaseMismatch(artRoot, ArtFolders.Backgrounds, file) is { } mismatch
            ? (null, mismatch)
            : ReadSize(artRoot, ArtFolders.Backgrounds, file);

    private static string? WithShownPath(string file, string? problem) =>
        problem is null ? null : $"{ExactFileName.ShownPath(ArtFolders.Backgrounds, file)}: {problem}.";
}
