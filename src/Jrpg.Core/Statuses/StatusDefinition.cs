using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>
/// One status as content describes it (ADR 0055): its words, whether it harms its holder, when in its holder's turn it
/// ticks, the family a holder keeps one of, and the parts it applies while held. Compares its parts in order (decision
/// 795), so two records built apart from the same parts are equal, as a drop table compares its entries.
/// </summary>
public sealed record StatusDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    /// <summary>A short line about what it does; may be empty.</summary>
    public required string Description { get; init; }

    /// <summary>Whether it hurts its holder, which the game's mark shows; false for a boon.</summary>
    public bool IsHarmful { get; init; }

    /// <summary>
    /// When its tick acts in its holder's turn (decision 695); moves only the tick, never the countdown.
    /// </summary>
    public StatusTiming ActsAt { get; init; } = StatusTiming.EndOfTurn;

    /// <summary>The statuses a holder keeps one of (decision 800), such as <c>poison</c>; its id when absent.</summary>
    public string? Family { get; init; }

    public required IReadOnlyList<StatusPart> Parts { get; init; }

    /// <summary>The family a holder keeps one of: <see cref="Family"/>, else the record's id.</summary>
    public string FamilyKey => Family ?? Id;

    /// <summary>Its one part that takes or restores HP each turn (decision 797), or null when it has none.</summary>
    public StatusPart? TickingPart =>
        Parts.FirstOrDefault(part => part.Kind is StatusPartKind.DamageEachTurn or StatusPartKind.HealEachTurn);

    public bool Has(StatusPartKind kind) => Parts.Any(part => part.Kind == kind);

    /// <summary>
    /// The record with every part's amount raised by <paramref name="permille"/> (decision 799), rounded toward zero as
    /// <see cref="Stats.StatFormula.Scaled"/> rounds, so -15 at 150 % is -22; an amountless part stays at 0.
    /// </summary>
    public StatusDefinition Strengthened(int permille) => this with
    {
        Parts = [.. Parts.Select(part => part with { Amount = StatFormula.Scaled(part.Amount, permille) })],
    };

    public bool Equals(StatusDefinition? other) =>
        other is not null
        && Id == other.Id
        && Name == other.Name
        && Description == other.Description
        && IsHarmful == other.IsHarmful
        && ActsAt == other.ActsAt
        && Family == other.Family
        && Parts.SequenceEqual(other.Parts);

    public override int GetHashCode() => HashCode.Combine(Id, Parts.Count);
}
