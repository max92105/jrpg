using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Flow;

/// <summary>
/// The track the flow's state calls for (ADR 0035): on the field, the playing cinematic's when it has one, else the
/// map's; null is silence, as on a map without one (decision 419).
/// </summary>
internal static class MusicSelection
{
    /// <param name="session">The playthrough; null while no game is on, on the title and the leader choice.</param>
    /// <param name="battleCue">What the battle screen on top calls for; null when none is.</param>
    public static string? TrackFor(GameSession? session, BattleMusicCue? battleCue)
    {
        if (session is null)
        {
            return UiSoundFiles.Music.Title;
        }

        if (battleCue == BattleMusicCue.Silence)
        {
            return null;
        }

        if (battleCue == BattleMusicCue.Field)
        {
            return FieldTrack(session);
        }

        // Pending from the encounter's flash until the session finishes the battle (decision 416).
        return session.Exploration.PendingEncounter is { } encounter
            ? encounter.MusicFile ?? UiSoundFiles.Music.Battle
            : FieldTrack(session);
    }

    // A cinematic step with a track plays it to its last still (decision 614); a map without a track is silent.
    private static string? FieldTrack(GameSession session) =>
        session.Exploration.CurrentCinematic?.MusicFile ?? session.Exploration.CurrentMap.Settings.MusicFile;
}
