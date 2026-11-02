using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Audio;

/// <summary>
/// Checks that the sound files the content names exist, at startup beside the art check (ADR 0018). The encoding isn't
/// read here: a file that won't play is the player's to report, and plays silent.
/// </summary>
public static class AudioReferenceCheck
{
    /// <summary>
    /// Every voice step's file is in the voices folder, and every map's track and encounter's theme in the music
    /// folder, under exactly the name given.
    /// </summary>
    /// <returns>One line per problem, naming the file and what names it; empty when the sound is complete.</returns>
    public static IReadOnlyList<string> FindProblems(ContentCatalog catalog, string audioRoot) =>
    [
        .. MissingByExactName(audioRoot, AudioFolders.Voices, VoiceFiles(catalog)),
        .. MissingByExactName(audioRoot, AudioFolders.Music, MapTracks(catalog)),
        .. MissingByExactName(audioRoot, AudioFolders.Music, EncounterThemes(catalog)),
    ];

    private static IEnumerable<(string File, string NamedBy)> VoiceFiles(ContentCatalog catalog) =>
        MapEventSteps.Of<VoiceStep>(catalog).Select(found => (found.Step.File, found.NamedBy));

    private static IEnumerable<(string File, string NamedBy)> MapTracks(ContentCatalog catalog) =>
        catalog.Maps.Values
            .Where(map => map.Settings.MusicFile is not null)
            .OrderBy(map => map.Settings.Id, StringComparer.Ordinal)
            .Select(map => (map.Settings.MusicFile!, $"map '{map.Settings.Id}'"));

    private static IEnumerable<(string File, string NamedBy)> EncounterThemes(ContentCatalog catalog) =>
        catalog.Encounters.Values
            .Where(encounter => encounter.MusicFile is not null)
            .OrderBy(encounter => encounter.Id, StringComparer.Ordinal)
            .Select(encounter => (encounter.MusicFile!, $"encounter '{encounter.Id}'"));

    private static IEnumerable<string> MissingByExactName(string audioRoot, string folder, IEnumerable<(string File, string NamedBy)> references)
    {
        foreach (var (file, namedBy) in references)
        {
            if ((ExactFileName.FindCaseMismatch(audioRoot, folder, file) ?? FindMissing(audioRoot, folder, file)) is { } problem)
            {
                yield return $"{problem} Named by {namedBy}.";
            }
        }
    }

    private static string? FindMissing(string audioRoot, string folder, string file) =>
        File.Exists(Path.Combine(audioRoot, folder, file))
            ? null
            : $"{ExactFileName.ShownPath(folder, file)}: not found in {audioRoot}.";
}
