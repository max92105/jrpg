using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>An effect strip played over one target.</summary>
/// <param name="Target">Who the effect landed on.</param>
/// <param name="File">The strip, in the effects folder.</param>
internal sealed record EffectOnTarget(Combatant Target, string File);
