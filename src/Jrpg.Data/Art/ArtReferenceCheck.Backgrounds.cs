using Jrpg.Core.Content;

namespace Jrpg.Data.Art;

/// <summary>
/// The battle backgrounds the startup check reads (decision 895): every encounter's own and every map's, by exact name.
/// </summary>
public static partial class ArtReferenceCheck
{
    private static void AddBackgroundProblems(List<string> problems, ContentCatalog catalog, string artRoot)
    {
        var encounters = catalog.Encounters.Values
            .Where(encounter => encounter.BackgroundFile is not null)
            .OrderBy(encounter => encounter.Id, StringComparer.Ordinal);
        var encounterBackgrounds = encounters.Select(encounter => (encounter.BackgroundFile!, $"encounter '{encounter.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Backgrounds, encounterBackgrounds);
        var maps = catalog.Maps.Values
            .Select(map => map.Settings)
            .Where(settings => settings.BattleBackgroundFile is not null)
            .OrderBy(settings => settings.Id, StringComparer.Ordinal);
        var mapBackgrounds = maps.Select(settings => (settings.BattleBackgroundFile!, $"map '{settings.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Backgrounds, mapBackgrounds);
    }
}
