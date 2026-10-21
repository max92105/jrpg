using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>A placeholder cue shown over one target (decision 101).</summary>
/// <param name="Target">Who the effect landed on.</param>
/// <param name="Text">The action's name.</param>
internal sealed record CueOnTarget(Combatant Target, string Text);
