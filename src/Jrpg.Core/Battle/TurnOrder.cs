using Jrpg.Core.Randomness;

namespace Jrpg.Core.Battle;

/// <summary>
/// Who acts next: a fixed cycle shuffled once at battle start, skipping defeated combatants.
/// Placeholder for a future speed-based order (plan 2026-10-17, guiding rule); ported from the demo's shuffled queue.
/// A readied extra turn (decision 202) plays right after its anchor's next turn, or after the next turn anyone takes
/// once the anchor has fallen, without moving the cycle on.
/// </summary>
internal sealed class TurnOrder
{
    private readonly List<Combatant> _cycle;
    private readonly List<(Combatant Actor, Combatant After)> _readied = [];
    private int _currentIndex = -1;
    private Combatant? _extraTurnActor;

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

    public Combatant Current => _extraTurnActor ?? _cycle[_currentIndex];

    /// <summary>Whether the current turn is a readied extra one, which has no upkeep.</summary>
    public bool IsExtraTurn => _extraTurnActor is not null;

    public bool HasReadiedTurn(Combatant actor) => _readied.Exists(readied => readied.Actor == actor);

    /// <summary>
    /// The next <paramref name="count"/> turns after the current one as the order stands now: the cycle walked on from
    /// the current combatant, wrapping, skipping the fallen, with each readied extra turn in its place.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    /// <exception cref="InvalidOperationException">Everyone is defeated; the battle should have ended.</exception>
    public IReadOnlyList<UpcomingTurn> Upcoming(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (!_cycle.Any(combatant => combatant.IsAlive))
        {
            throw new InvalidOperationException("No combatant is alive.");
        }

        var readied = new List<(Combatant Actor, Combatant After)>(_readied);
        var upcoming = new List<UpcomingTurn>(count);
        var lastActor = Current;
        for (var index = _currentIndex; upcoming.Count < count;)
        {
            if (TakeReadiedAfter(readied, _cycle[index], lastActor) is { } extra)
            {
                upcoming.Add(new UpcomingTurn(extra, IsExtra: true));
                lastActor = extra;
                continue;
            }

            index = (index + 1) % _cycle.Count;
            if (_cycle[index].IsAlive)
            {
                upcoming.Add(new UpcomingTurn(_cycle[index], IsExtra: false));
                lastActor = _cycle[index];
            }
        }

        return upcoming;
    }

    /// <summary>
    /// Moves to the next turn: a readied extra turn whose anchor just acted, or else the next living combatant in the
    /// cycle, wrapping.
    /// </summary>
    /// <exception cref="InvalidOperationException">Everyone is defeated; the battle should have ended.</exception>
    public void Advance()
    {
        var lastActor = _currentIndex >= 0 ? Current : null;
        _extraTurnActor = null;
        if (lastActor is not null && TakeReadiedAfter(_readied, _cycle[_currentIndex], lastActor) is { } extra)
        {
            _extraTurnActor = extra;
            return;
        }

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

    /// <summary>Gives <paramref name="actor"/> an extra turn after <paramref name="after"/>'s next turn.</summary>
    internal void ReadyExtraTurn(Combatant actor, Combatant after) => _readied.Add((actor, after));

    // The first readied turn anchored on the cycle combatant whose turn just came, or on one who fell before acting,
    // whose actor still stands and didn't act last. Every readied turn on one anchor plays in turn, and no combatant
    // plays two readied turns in a row.
    private static Combatant? TakeReadiedAfter(List<(Combatant Actor, Combatant After)> readied, Combatant anchor, Combatant lastActor)
    {
        readied.RemoveAll(turn => !turn.Actor.IsAlive);
        var index = readied.FindIndex(turn => turn.Actor != lastActor && (turn.After == anchor || !turn.After.IsAlive));
        if (index < 0)
        {
            return null;
        }

        var actor = readied[index].Actor;
        readied.RemoveAt(index);
        return actor;
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
