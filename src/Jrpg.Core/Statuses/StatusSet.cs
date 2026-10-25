namespace Jrpg.Core.Statuses;

/// <summary>The statuses on one combatant in one battle: at most one of each kind, in the order applied.</summary>
public sealed class StatusSet
{
    private readonly List<ActiveStatus> _active = [];

    public IReadOnlyList<ActiveStatus> Active => _active;

    public bool Has(StatusKind kind) => _active.Exists(status => status.Kind == kind);

    /// <summary>
    /// Adds the status, or refreshes one of the same kind: its turns become the grant's and it keeps the stronger
    /// power, never stacking (ADR 0021). Either way it is fresh until the turn now playing ends.
    /// </summary>
    internal void Apply(StatusGrant grant)
    {
        var index = _active.FindIndex(status => status.Kind == grant.Kind);
        if (index < 0)
        {
            _active.Add(new ActiveStatus(grant.Kind, grant.Power, grant.Turns, IsFresh: true));
            return;
        }

        var power = Math.Max(_active[index].Power, grant.Power);
        _active[index] = new ActiveStatus(grant.Kind, power, grant.Turns, IsFresh: true);
    }

    /// <summary>The end of the holder's own turn: a fresh status only loses its mark; the rest lose a turn.</summary>
    /// <returns>The kinds that ran out, in the order they were applied.</returns>
    internal IReadOnlyList<StatusKind> CountDown()
    {
        for (var index = 0; index < _active.Count; index++)
        {
            var status = _active[index];
            _active[index] = status.IsFresh ? status with { IsFresh = false } : status with { TurnsLeft = status.TurnsLeft - 1 };
        }

        var ended = _active.Where(status => status.TurnsLeft == 0).Select(status => status.Kind).ToList();
        _active.RemoveAll(status => status.TurnsLeft == 0);
        return ended;
    }

    /// <summary>The end of another's turn: what was granted during it counts from the holder's next turn.</summary>
    internal void ClearFreshMarks()
    {
        for (var index = 0; index < _active.Count; index++)
        {
            _active[index] = _active[index] with { IsFresh = false };
        }
    }

    /// <returns>What was active, in the order it was applied.</returns>
    internal IReadOnlyList<StatusKind> Clear()
    {
        var cleared = _active.Select(status => status.Kind).ToList();
        _active.Clear();
        return cleared;
    }
}
