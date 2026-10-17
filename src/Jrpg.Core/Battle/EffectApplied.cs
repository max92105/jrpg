using Jrpg.Core.Effects;

namespace Jrpg.Core.Battle;

/// <summary>An effect landed on <c>Target</c>. <c>Amount</c> is what was actually applied, e.g. the HP really lost, not the raw power.</summary>
public sealed record EffectApplied(Combatant Target, EffectKind Kind, int Amount) : BattleEvent;
