using Jrpg.Core.Session;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Flow;

/// <summary>
/// The track the flow's state calls for (ADR 0035); null is silence, as on a map without one (decision 419).
/// </summary>
internal static class MusicSelection
{
    /// <param name="session">The playthrough; null while no game is on, on the title and the leader choice.</param>
    public static string? TrackFor(GameSession? session)
    {
        if (session is null)
        {
            return UiSoundFiles.Music.Title;
        }

        // Pending from the encounter's flash until the session finishes the battle (decision 416).
        return session.Exploration.PendingEncounter is not null
            ? UiSoundFiles.Music.Battle
            : session.Exploration.CurrentMap.Settings.MusicFile;
    }
}
