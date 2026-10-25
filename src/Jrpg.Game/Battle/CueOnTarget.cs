using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>A placeholder cue shown over one target (decision 101).</summary>
/// <param name="Target">Who the effect landed on.</param>
/// <param name="Text">The action's or the status's name.</param>
/// <param name="ShowsAt">When it shows, from the impact: 0, or a later beat's start, such as a skipped turn.</param>
internal sealed record CueOnTarget(Combatant Target, string Text, TimeSpan ShowsAt = default);
