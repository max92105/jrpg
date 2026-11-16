namespace Jrpg.Core.Statuses;

/// <summary>
/// One status as content describes it (ADR 0055): its words, whether it harms its holder, the family a holder keeps one
/// of, and the parts it applies while held. Compares its parts in order (decision 795), so two records built apart from
/// the same parts are equal, as a drop table compares its entries.
/// </summary>
public sealed record StatusDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    /// <summary>A short line about what it does; may be empty.</summary>
    public required string Description { get; init; }

    /// <summary>Whether it hurts its holder, which the game's mark shows; false for a boon.</summary>
    public bool IsHarmful { get; init; }

    /// <summary>The statuses a holder keeps one of (decision 800), such as <c>poison</c>; its id when absent.</summary>
    public string? Family { get; init; }

    public required IReadOnlyList<StatusPart> Parts { get; init; }

    /// <summary>The family a holder keeps one of: <see cref="Family"/>, else the record's id.</summary>
    public string FamilyKey => Family ?? Id;

    public bool Equals(StatusDefinition? other) =>
        other is not null
        && Id == other.Id
        && Name == other.Name
        && Description == other.Description
        && IsHarmful == other.IsHarmful
        && Family == other.Family
        && Parts.SequenceEqual(other.Parts);

    public override int GetHashCode() => HashCode.Combine(Id, Parts.Count);
}
