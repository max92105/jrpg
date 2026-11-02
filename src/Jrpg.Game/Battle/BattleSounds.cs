using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The sounds a turn's events make, each at the start of the beat that shows it (decision 423). An event it doesn't
/// know throws, so a new mechanic can't be silent by accident, as in <see cref="ActionPlayback"/>.
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
            var sound = ImpactFileOf(battleEvent, events) is { } file ? new TimedSound(file, StartOfBeat(PlaybackBeatKind.Impact)) : SoundAtItsBeat(battleEvent, StartOfBeat);
            if (sound is { } found && !sounds.Contains(found))
            {
                sounds.Add(found);
            }
        }

        return [.. sounds.OrderBy(sound => sound.At)];
    }

    private static string? ImpactFileOf(BattleEvent battleEvent, IReadOnlyList<BattleEvent> events) => battleEvent switch
    {
        EffectApplied { Kind: EffectKind.Damage } => HasCritical(events) ? UiSoundFiles.Battle.Critical : UiSoundFiles.Battle.Hit,
        EffectApplied { Kind: EffectKind.RestoreHp or EffectKind.Revive } or LifeStolen => UiSoundFiles.Battle.Heal,
        HitMissed => UiSoundFiles.Battle.Miss,
        HitBlocked => UiSoundFiles.Battle.Block,
        StatusResisted => UiSoundFiles.Battle.Resisted,
        StatusApplied => UiSoundFiles.Battle.StatusApplied,
        _ => null,
    };

    private static bool HasCritical(IReadOnlyList<BattleEvent> events) =>
        events.Any(battleEvent => battleEvent is EffectApplied { IsCritical: true });

    private static TimedSound? SoundAtItsBeat(BattleEvent battleEvent, Func<PlaybackBeatKind, TimeSpan> startOf) => battleEvent switch
    {
        CombatantDefeated => new TimedSound(UiSoundFiles.Battle.Fall, startOf(PlaybackBeatKind.Defeat)),
        StatusTicked ticked => new TimedSound(TickFileOf(ticked.Status), startOf(PlaybackBeatKind.Tick)),
        Backfired => new TimedSound(UiSoundFiles.Battle.Backfire, startOf(PlaybackBeatKind.Backfire)),
        TurnSkipped => new TimedSound(UiSoundFiles.Battle.Stunned, startOf(PlaybackBeatKind.Skip)),
        EnemyPhaseEntered => new TimedSound(UiSoundFiles.Battle.Enraged, startOf(PlaybackBeatKind.Phase)),
        _ => SoundAtTheStartOf(battleEvent),
    };

    // These have no beat of their own: the action starting, and what shows by live state. A resource's cost is silent,
    // as the skill's own sound covers spending; an outcome sounds on its own screen.
    private static TimedSound? SoundAtTheStartOf(BattleEvent battleEvent)
    {
        var file = battleEvent switch
        {
            ActionStarted { Command: ItemCommand } => UiSoundFiles.Battle.ItemUse,
            ActionStarted { Command: DefendCommand } => UiSoundFiles.Battle.Defend,
            ResourceChanged { Change: > 0 } => UiSoundFiles.Battle.ResourceGain,
            StatusEnded => UiSoundFiles.Battle.StatusEnded,
            PlacesSwapped => UiSoundFiles.Battle.Move,
            ExtraTurnReadied => UiSoundFiles.Battle.ExtraTurn,
            ActionStarted or ResourceChanged or BattleEnded => null,
            _ => throw new InvalidOperationException($"The battle has no sound for a {battleEvent.GetType().Name} yet."),
        };
        return file is null ? null : new TimedSound(file, TimeSpan.Zero);
    }

    private static string TickFileOf(StatusKind status) => status switch
    {
        StatusKind.Poison => UiSoundFiles.Battle.PoisonTick,
        StatusKind.Regen => UiSoundFiles.Battle.RegenTick,
        _ => throw new InvalidOperationException($"A ticking {status} has no sound yet."),
    };
}
