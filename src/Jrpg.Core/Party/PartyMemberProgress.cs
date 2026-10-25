namespace Jrpg.Core.Party;

/// <summary>
/// What changes about a party member during play, as a save keeps it; worn items by item id, null when the slot is
/// empty, and each lasting resource's amount by resource id (ADR 0024). Everything else comes from content: skills from
/// their class, max HP and base stats from it at their level (ADR 0007, ADR 0017, ADR 0020).
/// </summary>
public sealed record PartyMemberProgress(
    string Id, int Level, int Experience, int Hp, string? WeaponId, string? ArmorId, IReadOnlyDictionary<string, int> LastingAmounts)
{
    // A record compares a dictionary by reference; two progresses with the same amounts are the same progress.
    public bool Equals(PartyMemberProgress? other) =>
        other is not null
        && (Id, Level, Experience, Hp, WeaponId, ArmorId) == (other.Id, other.Level, other.Experience, other.Hp, other.WeaponId, other.ArmorId)
        && LastingAmounts.Count == other.LastingAmounts.Count
        && LastingAmounts.All(entry => other.LastingAmounts.TryGetValue(entry.Key, out var amount) && amount == entry.Value);

    public override int GetHashCode() => HashCode.Combine(Id, Level, Experience, Hp, WeaponId, ArmorId, LastingAmounts.Count);
}
