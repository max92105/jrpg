namespace Jrpg.Core.Loot;

/// <summary>What an enemy drops when defeated; several enemies may name one table (decision 343).</summary>
public sealed record DropTable(string Id, IReadOnlyList<DropEntry> Entries)
{
    // A record compares a list by reference; two tables read from the same content are one table (decision 604). The
    // entries' order counts: it is the order the roll draws them in.
    public bool Equals(DropTable? other) =>
        other is not null && Id == other.Id && Entries.SequenceEqual(other.Entries);

    public override int GetHashCode() => HashCode.Combine(Id, Entries.Count);
}
