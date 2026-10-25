namespace Jrpg.Core.Battle;

/// <summary>
/// The party's line in one battle (ADR 0023): it starts from the active order, and distance is the difference in
/// places. A fallen ally keeps their place. The line is battle state: the party order after the battle doesn't change.
/// </summary>
public sealed class Formation
{
    private readonly List<Combatant> _line;

    /// <param name="party">The party's combatants, in active order.</param>
    internal Formation(IReadOnlyList<Combatant> party) => _line = [.. party];

    public IReadOnlyList<Combatant> Line => _line;

    /// <returns>The place in the line, from 0.</returns>
    /// <exception cref="ArgumentException"><paramref name="member"/> isn't in the line.</exception>
    public int PositionOf(Combatant member)
    {
        var position = _line.IndexOf(member);
        return position >= 0 ? position : throw new ArgumentException($"{member.Name} isn't in the party's line.", nameof(member));
    }

    /// <returns>The allies just before and after <paramref name="member"/>, fallen or not, in line order.</returns>
    public IReadOnlyList<Combatant> Neighbors(Combatant member) => WithinRange(member, 1).Where(ally => ally != member).ToList();

    /// <returns>Everyone within <paramref name="range"/> places, the center included, in line order.</returns>
    public IReadOnlyList<Combatant> WithinRange(Combatant center, int range)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(range);

        var position = PositionOf(center);
        var first = Math.Max(0, position - range);
        var last = Math.Min(_line.Count - 1, position + range);
        return _line.GetRange(first, last - first + 1);
    }

    /// <exception cref="ArgumentException">The two aren't next to each other.</exception>
    internal void Swap(Combatant mover, Combatant other)
    {
        var (from, to) = (PositionOf(mover), PositionOf(other));
        if (Math.Abs(from - to) != 1)
        {
            throw new ArgumentException($"{mover.Name} can only trade places with an ally right beside them.", nameof(other));
        }

        (_line[from], _line[to]) = (other, mover);
    }
}
