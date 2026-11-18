namespace Jrpg.Editor.Catalogs;

/// <summary>
/// One record of a catalog as a box or a row offers it (decision 967), shown by name with the id that tells two records
/// named alike apart; <see cref="Missing"/> stands for an id the draft no longer holds, so a row naming it is still
/// shown, never thrown (<see cref="Of"/>); <see cref="ByName"/> orders an add box's entries. The earlier one-catalog
/// copies stay until next touched: <c>Encounters/EnemyChoice</c> and <c>Loot/DropItemChoice</c> have this shape,
/// <c>Trade/PricedItemChoice</c>, <c>Skills/StatusChoice</c> and <c>Battle/TacticSkillChoice</c> each carry an extra (a
/// price, a line, the attack entry).
/// </summary>
/// <param name="Id">The record's id, which the content names.</param>
/// <param name="Name">The name the game shows; the id for a missing record.</param>
internal sealed record RecordChoice(string Id, string Name)
{
    /// <summary>True for an id the draft lacks (<see cref="Missing"/>).</summary>
    public bool IsMissing { get; private init; }

    /// <summary>"Fireball (fireball)"; "(missing) fireball" for an id the draft lacks.</summary>
    public string Label => IsMissing ? $"(missing) {Id}" : $"{Name} ({Id})";

    public static RecordChoice Missing(string id) => new(id, id) { IsMissing = true };

    /// <summary>
    /// A row's choice for the id it names: by its record's name, or missing when the draft has no record.
    /// </summary>
    /// <param name="id">The id the row names.</param>
    /// <param name="name">The record's name; null when the draft lacks the id.</param>
    public static RecordChoice Of(string id, string? name) => name is null ? Missing(id) : new(id, name);

    /// <summary>
    /// An add box's entries in the order it lists them: by name, then by the id that tells two named alike apart.
    /// </summary>
    public static IReadOnlyList<RecordChoice> ByName(IEnumerable<RecordChoice> choices) =>
    [
        .. choices
            .OrderBy(choice => choice.Name, StringComparer.Ordinal)
            .ThenBy(choice => choice.Id, StringComparer.Ordinal),
    ];
}
