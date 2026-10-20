using Jrpg.Core.Randomness;

namespace Jrpg.Core.Battle;

/// <summary>
/// Who acts next: a fixed cycle shuffled once at battle start, skipping defeated combatants.
/// Placeholder for a future speed-based order (plan 2026-10-17, guiding rule); ported from the demo's shuffled queue.
/// </summary>
internal sealed class TurnOrder
{
    private readonly List<Combatant> _cycle;
    private int _currentIndex = -1;

    /// <exception cref="ArgumentException">No combatant is alive.</exception>
    public TurnOrder(IReadOnlyList<Combatant> combatants, IRandom random)
    {
        if (!combatants.Any(combatant => combatant.IsAlive))
        {
            throw new ArgumentException("A turn order needs at least one living combatant.", nameof(combatants));
        }

        _cycle = [.. combatants];
        Shuffle(_cycle, random);
        Advance();
    }

    public Combatant Current => _cycle[_currentIndex];

    /// <summary>
    /// The next <paramref name="count"/> turns after the current one as the order stands now: the cycle walked on from
    /// the current combatant, wrapping, skipping the fallen. A combatant appears again once the cycle comes back to them.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    /// <exception cref="InvalidOperationException">Everyone is defeated; the battle should have ended.</exception>
    public IReadOnlyList<Combatant> Upcoming(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (!_cycle.Any(combatant => combatant.IsAlive))
        {
            throw new InvalidOperationException("No combatant is alive.");
        }

        var upcoming = new List<Combatant>(count);
        for (var index = _currentIndex; upcoming.Count < count;)
        {
            index = (index + 1) % _cycle.Count;
            if (_cycle[index].IsAlive)
            {
                upcoming.Add(_cycle[index]);
            }
        }

        return upcoming;
    }

    /// <summary>Moves to the next living combatant, wrapping around the cycle.</summary>
    /// <exception cref="InvalidOperationException">Everyone is defeated; the battle should have ended.</exception>
    public void Advance()
    {
        for (var step = 1; step <= _cycle.Count; step++)
        {
            var index = (_currentIndex + step) % _cycle.Count;
            if (_cycle[index].IsAlive)
            {
                _currentIndex = index;
                return;
            }
        }

        throw new InvalidOperationException("No combatant is alive.");
    }

    // Fisher-Yates. Seeded results depend on this exact loop and its order of IRandom calls.
    private static void Shuffle(List<Combatant> combatants, IRandom random)
    {
        for (var last = combatants.Count - 1; last > 0; last--)
        {
            var swapWith = random.NextBelow(last + 1);
            (combatants[last], combatants[swapWith]) = (combatants[swapWith], combatants[last]);
        }
    }
}
