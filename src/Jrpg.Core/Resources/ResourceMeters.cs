namespace Jrpg.Core.Resources;

/// <summary>One combatant's current resource amounts in one battle, each kept within 0..max (ADR 0019).</summary>
public sealed class ResourceMeters
{
    private readonly Dictionary<string, int> _amounts;
    private readonly Dictionary<string, ResourceDefinition> _definitions;

    /// <param name="definitions">The holder's resources; each starts at its starting amount.</param>
    public ResourceMeters(IReadOnlyList<ResourceDefinition> definitions)
    {
        Definitions = definitions;
        _definitions = definitions.ToDictionary(definition => definition.Id);
        _amounts = definitions.ToDictionary(definition => definition.Id, definition => definition.StartsAt);
    }

    public IReadOnlyList<ResourceDefinition> Definitions { get; }

    /// <exception cref="ArgumentException">The holder has no resource <paramref name="resourceId"/>.</exception>
    public int AmountOf(string resourceId) =>
        _amounts.TryGetValue(resourceId, out var amount)
            ? amount
            : throw new ArgumentException($"This combatant has no resource '{resourceId}'.", nameof(resourceId));

    /// <returns>The change applied after clamping to 0..max; 0 when the amount was already at that bound.</returns>
    /// <exception cref="ArgumentException">The holder has no resource <paramref name="resourceId"/>.</exception>
    internal int Change(string resourceId, int amount)
    {
        var before = AmountOf(resourceId);
        var after = Math.Clamp(before + amount, 0, _definitions[resourceId].Max);
        _amounts[resourceId] = after;
        return after - before;
    }
}
