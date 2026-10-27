namespace Jrpg.Core.Resources;

/// <summary>One combatant's current resource amounts in one battle, each kept within 0..max (ADR 0019).</summary>
public sealed class ResourceMeters
{
    private readonly Dictionary<string, int> _amounts;
    private readonly Dictionary<string, int> _maxima;
    private readonly Dictionary<string, int> _gainLowerings;
    private readonly HashSet<string> _dominant = [];

    /// <param name="starts">The holder's resources, each with its max and starting amount for this battle.</param>
    public ResourceMeters(IReadOnlyList<ResourceStart> starts)
    {
        Definitions = [.. starts.Select(start => start.Definition)];
        _maxima = starts.ToDictionary(start => start.Definition.Id, start => start.Max);
        _amounts = starts.ToDictionary(start => start.Definition.Id, start => start.Amount);
        _gainLowerings = starts.ToDictionary(start => start.Definition.Id, start => start.GainLowering);
    }

    public IReadOnlyList<ResourceDefinition> Definitions { get; }

    /// <summary>
    /// Whether this force leads its rival (decision 204): it went strictly above, and the rival hasn't since. At the
    /// start neither leads, whatever the amounts (question 227).
    /// </summary>
    /// <exception cref="ArgumentException">The holder has no resource <paramref name="resourceId"/>.</exception>
    public bool IsDominant(string resourceId) =>
        _amounts.ContainsKey(resourceId)
            ? _dominant.Contains(resourceId)
            : throw new ArgumentException($"This combatant has no resource '{resourceId}'.", nameof(resourceId));

    /// <exception cref="ArgumentException">The holder has no resource <paramref name="resourceId"/>.</exception>
    public int AmountOf(string resourceId) =>
        _amounts.TryGetValue(resourceId, out var amount)
            ? amount
            : throw new ArgumentException($"This combatant has no resource '{resourceId}'.", nameof(resourceId));

    /// <summary>The points every gain to the resource loses to its holder's Will (decision 319); 0 for most.</summary>
    /// <exception cref="ArgumentException">The holder has no resource <paramref name="resourceId"/>.</exception>
    public int GainLoweringOf(string resourceId) =>
        _gainLowerings.TryGetValue(resourceId, out var lowering)
            ? lowering
            : throw new ArgumentException($"This combatant has no resource '{resourceId}'.", nameof(resourceId));

    /// <summary>The max for this battle, which a speed-scaled resource takes from its holder.</summary>
    /// <exception cref="ArgumentException">The holder has no resource <paramref name="resourceId"/>.</exception>
    public int MaxOf(string resourceId) =>
        _maxima.TryGetValue(resourceId, out var max)
            ? max
            : throw new ArgumentException($"This combatant has no resource '{resourceId}'.", nameof(resourceId));

    /// <returns>The change applied after clamping to 0..max; 0 when the amount was already at that bound.</returns>
    /// <exception cref="ArgumentException">The holder has no resource <paramref name="resourceId"/>.</exception>
    internal int Change(string resourceId, int amount)
    {
        var before = AmountOf(resourceId);
        var after = Math.Clamp(before + amount, 0, MaxOf(resourceId));
        _amounts[resourceId] = after;
        UpdateDominance(resourceId);
        return after - before;
    }

    // Each force of the pair names the other, so both sides are judged; a tie leaves the leader as it was.
    private void UpdateDominance(string changedId)
    {
        foreach (var resource in Definitions)
        {
            if (resource.Dominance is not { } rule || (resource.Id != changedId && rule.Over != changedId))
            {
                continue;
            }

            var lead = AmountOf(resource.Id) - AmountOf(rule.Over);
            if (lead > 0)
            {
                _dominant.Add(resource.Id);
            }
            else if (lead < 0)
            {
                _dominant.Remove(resource.Id);
            }
        }
    }
}
