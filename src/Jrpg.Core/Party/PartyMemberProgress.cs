using Jrpg.Core.Items;

namespace Jrpg.Core.Party;

/// <summary>
/// What changes about a party member during play, as a save keeps it: worn items by slot and item id, an empty slot
/// absent (ADR 0028), and each lasting resource's amount by resource id (ADR 0024). Everything else comes from content:
/// skills from their class, max HP and base stats from it at their level (ADR 0007, ADR 0017, ADR 0020).
/// </summary>
public sealed record PartyMemberProgress(
    string Id,
    int Level,
    int Experience,
    int Hp,
    IReadOnlyDictionary<EquipmentSlot, string> Worn,
    IReadOnlyDictionary<string, int> LastingAmounts)
{
    // A record compares a dictionary by reference; two progresses wearing the same and holding the same amounts are the
    // same progress.
    public bool Equals(PartyMemberProgress? other) =>
        other is not null
        && (Id, Level, Experience, Hp) == (other.Id, other.Level, other.Experience, other.Hp)
        && SameEntries(Worn, other.Worn)
        && SameEntries(LastingAmounts, other.LastingAmounts);

    public override int GetHashCode() => HashCode.Combine(Id, Level, Experience, Hp, Worn.Count, LastingAmounts.Count);

    private static bool SameEntries<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> first, IReadOnlyDictionary<TKey, TValue> second)
        where TValue : IEquatable<TValue> =>
        first.Count == second.Count
        && first.All(entry => second.TryGetValue(entry.Key, out var value) && value.Equals(entry.Value));
}
