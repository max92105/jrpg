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
    /// <summary>Every voice step's file is in the voices folder under exactly the name the step gives.</summary>
    /// <returns>One line per problem, naming the file and what names it; empty when the sound is complete.</returns>
    public static IReadOnlyList<string> FindProblems(ContentCatalog catalog, string audioRoot)
    {
        var problems = new List<string>();
        foreach (var (voice, namedBy) in MapEventSteps.Of<VoiceStep>(catalog))
        {
            if ((ExactFileName.FindCaseMismatch(audioRoot, AudioFolders.Voices, voice.File) ?? FindMissing(audioRoot, voice.File)) is { } problem)
            {
                problems.Add($"{problem} Named by {namedBy}.");
            }
        }

        return problems;
    }

    private static string? FindMissing(string audioRoot, string file) =>
        File.Exists(Path.Combine(audioRoot, AudioFolders.Voices, file))
            ? null
            : $"{ExactFileName.ShownPath(AudioFolders.Voices, file)}: not found in {audioRoot}.";
}
