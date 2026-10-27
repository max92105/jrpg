using System.Collections.Frozen;

namespace Jrpg.Core.Stats;

/// <summary>
/// What an item adds, by stat kind: whole points for a primary, max HP or speed, whole percents for the rest, accuracy
/// included (ADR 0026). A kind it doesn't name adds 0. Two bonuses are equal when they add the same to every kind.
/// </summary>
public sealed record StatBonuses
{
    private readonly FrozenDictionary<StatKind, int> _values;

    /// <param name="values">Each kind's bonus; copied, so the caller's dictionary can change afterwards.</param>
    public StatBonuses(IReadOnlyDictionary<StatKind, int> values)
    {
        _values = values.ToFrozenDictionary();
    }

    public static StatBonuses None { get; } = new(new Dictionary<StatKind, int>());

    /// <summary>The kinds this names, with their bonus.</summary>
    public IReadOnlyDictionary<StatKind, int> Values => _values;

    public int Of(StatKind kind) => _values.GetValueOrDefault(kind);

    /// <summary>
    /// These and <paramref name="other"/> added kind by kind: what two items add together. A kind they cancel out is
    /// left out, as it adds nothing.
    /// </summary>
    public StatBonuses Plus(StatBonuses other) => new(_values.Keys
        .Union(other._values.Keys)
        .Select(kind => (Kind: kind, Bonus: Of(kind) + other.Of(kind)))
        .Where(entry => entry.Bonus != 0)
        .ToDictionary(entry => entry.Kind, entry => entry.Bonus));

    public bool Equals(StatBonuses? other) =>
        other is not null
        && _values.Count == other._values.Count
        && _values.All(entry => other._values.TryGetValue(entry.Key, out var value) && value == entry.Value);

    // Order-independent, to match Equals.
    public override int GetHashCode() =>
        _values.Aggregate(0, (hash, entry) => hash ^ HashCode.Combine(entry.Key, entry.Value));
}
