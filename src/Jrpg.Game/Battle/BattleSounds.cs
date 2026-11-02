using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The sounds a turn's events make, each at the start of the beat that shows it (decision 423). An event it doesn't
/// know throws, so a new mechanic can't be silent by accident, as in <see cref="ActionPlayback"/>.
/// </summary>
internal static class BattleSounds
{
    /// <param name="events">One turn's events, as <see cref="BattleState.Execute"/> returned them.</param>
    /// <param name="startOf">When a beat of that kind starts, from the playback's start.</param>
    /// <returns>Sorted by time, each file once at each time.</returns>
    /// <exception cref="InvalidOperationException">An event the battle's sounds don't know yet.</exception>
    public static IReadOnlyList<TimedSound> Of(IReadOnlyList<BattleEvent> events, Func<PlaybackBeatKind, TimeSpan> startOf)
    {
        var sounds = new List<TimedSound>();
        foreach (var battleEvent in events)
        {
            var sound = ImpactFileOf(battleEvent, events) is { } file ? new TimedSound(file, startOf(PlaybackBeatKind.Impact)) : OtherSoundOf(battleEvent);
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

    // Silent until the plan's next slice gives each its sound; an outcome sounds on its own screen.
    private static TimedSound? OtherSoundOf(BattleEvent battleEvent) => battleEvent switch
    {
        ActionStarted or CombatantDefeated or StatusTicked or Backfired or TurnSkipped or ResourceChanged => null,
        StatusEnded or PlacesSwapped or ExtraTurnReadied or EnemyPhaseEntered or BattleEnded => null,
        _ => throw new InvalidOperationException($"The battle has no sound for a {battleEvent.GetType().Name} yet."),
    };
}
