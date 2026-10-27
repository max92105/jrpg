namespace Jrpg.Core.Effects;

/// <summary>
/// What stands between a hit and its target's HP. A class, not a struct, like <c>CombatStats</c>: a struct's
/// <c>default</c> would pass for a real "nothing in the way".
/// </summary>
/// <param name="Defense">Subtracted from the hit's power first (until the plan's M2b).</param>
/// <param name="ResistancePermille">The target's resistance to the hit's type: the share taken off.</param>
/// <param name="ExtraTakenPermille">The extra share the target's statuses make them take, such as Exposed's.</param>
internal sealed record Mitigation(int Defense, int ResistancePermille, int ExtraTakenPermille)
{
    public static Mitigation None { get; } = new(Defense: 0, ResistancePermille: 0, ExtraTakenPermille: 0);
}
