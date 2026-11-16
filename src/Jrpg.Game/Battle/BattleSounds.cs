using Jrpg.Core.Battle;
using Jrpg.Core.Statuses;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The sounds a turn's events make, each at the start of the beat that shows it (decision 423); the action's opening
/// sound is its wind-up's (<see cref="ActionSounds"/>, decision 647). An event it doesn't know throws, so a new
/// mechanic can't be silent by accident, as in <see cref="ActionPlayback"/>.
/// </summary>
internal static class BattleSounds
{
    /// <param name="events">One turn's events, as <see cref="BattleState.Execute"/> returned them.</param>
    /// <param name="startOf">
    /// When the beat of that kind showing the event at that place starts, from the playback's start.
    /// </param>
    /// <returns>Sorted by time, each file once at each time.</returns>
    /// <exception cref="InvalidOperationException">An event the battle's sounds don't know yet.</exception>
    public static IReadOnlyList<TimedSound> Of(IReadOnlyList<BattleEvent> events, Func<int, PlaybackBeatKind, TimeSpan> startOf)
    {
        var sounds = new List<TimedSound>();
        for (var index = 0; index < events.Count; index++)
        {
            var place = index;
            TimeSpan StartOfBeat(PlaybackBeatKind kind) => startOf(place, kind);
            var battleEvent = events[index];
            var sound = ActionSounds.ImpactOf(battleEvent, events) is { } file
                ? new TimedSound(file, StartOfBeat(PlaybackBeatKind.Impact))
                : SoundAtItsBeat(battleEvent, StartOfBeat);
            if (sound is { } found && !sounds.Contains(found))
            {
                sounds.Add(found);
            }
        }

        return [.. sounds.OrderBy(sound => sound.At)];
    }

    private static TimedSound? SoundAtItsBeat(BattleEvent battleEvent, Func<PlaybackBeatKind, TimeSpan> startOf) => battleEvent switch
    {
        CombatantDefeated => new TimedSound(UiSoundFiles.Battle.Fall, startOf(PlaybackBeatKind.Defeat)),
        StatusTicked ticked => new TimedSound(TickFileOf(ticked.Status), startOf(PlaybackBeatKind.Tick)),
        Backfired => new TimedSound(UiSoundFiles.Battle.Backfire, startOf(PlaybackBeatKind.Backfire)),
        TurnSkipped => new TimedSound(UiSoundFiles.Battle.Stunned, startOf(PlaybackBeatKind.Skip)),
        EnemyPhaseEntered => new TimedSound(UiSoundFiles.Battle.Enraged, startOf(PlaybackBeatKind.Phase)),
        _ => SoundAtTheStartOf(battleEvent),
    };

    // These have no beat of their own: what shows by live state. The action starting is silent here: its opening sound
    // is its wind-up's (decision 647). A resource's cost is silent, as the skill's own sound covers spending; an
    // outcome sounds on its own screen.
    private static TimedSound? SoundAtTheStartOf(BattleEvent battleEvent)
    {
        var file = battleEvent switch
        {
            ResourceChanged { Change: > 0 } => UiSoundFiles.Battle.ResourceGain,
            StatusEnded => UiSoundFiles.Battle.StatusEnded,
            PlacesSwapped => UiSoundFiles.Battle.Move,
            ExtraTurnReadied => UiSoundFiles.Battle.ExtraTurn,
            ActionStarted or ResourceChanged or BattleEnded => null,
            _ => throw new InvalidOperationException($"The battle has no sound for a {battleEvent.GetType().Name} yet."),
        };
        return file is null ? null : new TimedSound(file, TimeSpan.Zero);
    }

    // A tick sounds by its part, whatever the record (decision 810): any damage as Poison's, any heal as Regen's.
    private static string TickFileOf(StatusDefinition status) => status.TickingPart?.Kind switch
    {
        StatusPartKind.DamageEachTurn => UiSoundFiles.Battle.PoisonTick,
        StatusPartKind.HealEachTurn => UiSoundFiles.Battle.RegenTick,
        _ => throw new InvalidOperationException($"{status.Id} ticked without a ticking part."),
    };
}
