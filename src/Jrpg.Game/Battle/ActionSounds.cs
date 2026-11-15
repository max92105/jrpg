using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// Which sound an action makes at each of its two moments (decision 647): its opening, as its wind-up starts with the
/// plaque, and its impact's, as it lands. Per-action opening and impact files, when content gains them, fill their
/// moment here and nowhere else.
/// </summary>
internal static class ActionSounds
{
    /// <returns>
    /// A skill's own sound when it has one, the item-use sound for an item, the Defend sound; none for an Attack, a
    /// Move or Run, whose sounds come from what they do.
    /// </returns>
    public static string? OpeningOf(BattleCommand command) => command switch
    {
        SkillCommand { Skill.SoundFile: { Length: > 0 } skillSound } => skillSound,
        ItemCommand => UiSoundFiles.Battle.ItemUse,
        DefendCommand => UiSoundFiles.Battle.Defend,
        _ => null,
    };

    /// <param name="battleEvent">One event of the turn.</param>
    /// <param name="events">The whole turn: one critical hit makes every hit of it sound critical.</param>
    /// <returns>What the event sounds like at the impact; null when it isn't shown there.</returns>
    public static string? ImpactOf(BattleEvent battleEvent, IReadOnlyList<BattleEvent> events) => battleEvent switch
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
}
