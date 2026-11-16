namespace Jrpg.Core.Statuses;

/// <summary>A status an effect grants: the record it names and how long it lasts (ADR 0055, decision 798).</summary>
/// <param name="Status">The record granted, as content names it; its amounts are raised as it lands.</param>
/// <param name="Turns">How many of the holder's turns it lasts, at least 1.</param>
public sealed record StatusGrant(StatusDefinition Status, int Turns)
{
    /// <summary>The percent it lands with before Intelligence and resistance (decision 798), 1 to 100.</summary>
    public int Chance { get; init; } = 100;
}
