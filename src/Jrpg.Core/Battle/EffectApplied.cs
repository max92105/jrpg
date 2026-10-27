using Jrpg.Core.Effects;

namespace Jrpg.Core.Battle;

/// <summary>An effect landed on <c>Target</c>.</summary>
/// <param name="Target">Who it landed on.</param>
/// <param name="Kind">What it did.</param>
/// <param name="Amount">What was actually applied, e.g. the HP really lost, not the raw power.</param>
/// <param name="Affinity">What the target's resistance did to a hit; neutral for anything but damage.</param>
/// <param name="IsCritical">Whether a damage hit crit (decision 300); never for anything but damage.</param>
public sealed record EffectApplied(
    Combatant Target,
    EffectKind Kind,
    int Amount,
    HitAffinity Affinity = HitAffinity.Neutral,
    bool IsCritical = false) : BattleEvent;
