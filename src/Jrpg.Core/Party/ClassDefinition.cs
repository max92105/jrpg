namespace Jrpg.Core.Party;

/// <summary>
/// A class, like a D&amp;D class (decision 164): what a character can do. The description shows when choosing a leader;
/// the stats it gives by level come with ADR 0017.
/// </summary>
public sealed record ClassDefinition(string Id, string Name, string Description);
