namespace Jrpg.Core.Battle;

/// <summary>
/// A landed hit <paramref name="Target"/> blocked (decision 348): it deals a share of what it would. Reported before
/// the hit's <see cref="EffectApplied"/>.
/// </summary>
public sealed record HitBlocked(Combatant Target) : BattleEvent;
