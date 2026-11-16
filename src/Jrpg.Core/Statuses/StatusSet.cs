namespace Jrpg.Core.Statuses;

/// <summary>
/// The statuses on one combatant in one battle: at most one of each family (decision 800), in the order applied.
/// </summary>
public sealed class StatusSet
{
    private readonly List<ActiveStatus> _active = [];

    public IReadOnlyList<ActiveStatus> Active => _active;

    /// <param name="familyKey">A status's <see cref="StatusDefinition.FamilyKey"/>.</param>
    public bool Has(string familyKey) => _active.Exists(status => status.Status.FamilyKey == familyKey);

    /// <returns>
    /// The first status held, in the order applied, with a part of <paramref name="kind"/>; else null.
    /// </returns>
    public StatusDefinition? FirstWith(StatusPartKind kind) => _active.Find(status => status.Status.Has(kind))?.Status;

    /// <summary>
    /// Adds the status, or refreshes the one of its family: it becomes the granted record with the grant's turns,
    /// keeping part by part the stronger amount of the two, never stacking (ADR 0021, decision 800). Either way it is
    /// fresh until the turn now playing ends.
    /// </summary>
    internal void Apply(StatusGrant grant)
    {
        var index = _active.FindIndex(status => status.Status.FamilyKey == grant.Status.FamilyKey);
        if (index < 0)
        {
            _active.Add(new ActiveStatus(grant.Status, grant.Turns, IsFresh: true));
            return;
        }

        _active[index] = new ActiveStatus(Stronger(_active[index].Status, grant.Status), grant.Turns, IsFresh: true);
    }

    /// <summary>The end of the holder's own turn: a fresh status only loses its mark; the rest lose a turn.</summary>
    /// <returns>The statuses that ran out, in the order they were applied.</returns>
    internal IReadOnlyList<StatusDefinition> CountDown()
    {
        for (var index = 0; index < _active.Count; index++)
        {
            var status = _active[index];
            _active[index] = status.IsFresh ? status with { IsFresh = false } : status with { TurnsLeft = status.TurnsLeft - 1 };
        }

        var ended = _active.Where(status => status.TurnsLeft == 0).Select(status => status.Status).ToList();
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
    internal IReadOnlyList<StatusDefinition> Clear()
    {
        var cleared = _active.Select(status => status.Status).ToList();
        _active.Clear();
        return cleared;
    }

    // The records of one family have the same parts in the same order (decision 797), so they pair up one to one.
    private static StatusDefinition Stronger(StatusDefinition held, StatusDefinition granted) => granted with
    {
        Parts = [.. granted.Parts.Zip(held.Parts, StrongerPart)],
    };

    // The amount farther from zero is the stronger, a negative one included (decision 800); a tie keeps the granted.
    private static StatusPart StrongerPart(StatusPart granted, StatusPart held) =>
        Math.Abs(held.Amount) > Math.Abs(granted.Amount) ? held : granted;
}
