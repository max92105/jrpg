namespace Jrpg.Core.Loot;

/// <summary>What an enemy drops when defeated; several enemies may name one table (decision 343).</summary>
public sealed record DropTable(string Id, IReadOnlyList<DropEntry> Entries);
