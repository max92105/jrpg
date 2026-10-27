using Jrpg.Core.Effects;

namespace Jrpg.Core.Battle;

/// <summary>A combatant's basic attack as an effect: their attack power, as their attack type, on one enemy.</summary>
internal static class AttackEffect
{
    /// <summary>
    /// Whom a basic attack can aim at: shared by <see cref="BattleState"/>'s check of the chosen target and the targets
    /// it offers, so the two can't drift.
    /// </summary>
    public const Targeting Targeting = Effects.Targeting.SingleEnemy;

    /// <summary>The attack from <paramref name="attacker"/>'s stats now, statuses included, raised by Power.</summary>
    public static Effect Of(Combatant attacker)
    {
        var stats = attacker.CurrentStats;
        return EffectScaling.ScaledFor(new Effect(EffectKind.Damage, stats.Attack, Targeting, DamageType: stats.AttackType), stats.DamagePermille);
    }
}
