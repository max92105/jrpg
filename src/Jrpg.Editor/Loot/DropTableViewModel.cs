using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Loot;

/// <summary>
/// One drop table as the Drop tables window edits it (ADR 0045): its id (typed while the table is new, fixed once the
/// draft holds it, decision 582) and its entry rows in the roll's order, numbered from 0 as Data numbers them.
/// <see cref="Problem"/> is the typed id's or a row's box that isn't a number; the rest is Data's, shown by the list.
/// </summary>
internal sealed partial class DropTableViewModel : ObservableObject
{
    private DropTableViewModel(string id, IEnumerable<DropEntryRow> entries)
    {
        Id = id;
        Entries = [.. entries];
        Renumber();
        SelectedEntry = Entries.FirstOrDefault();
    }

    /// <param name="table">The table as the draft holds it.</param>
    /// <param name="items">Every item, for the rows' names; an id the content lacks shows as its own name.</param>
    public static DropTableViewModel From(DropTable table, IReadOnlyDictionary<string, ItemDefinition> items) =>
        new(table.Id, table.Entries.Select(entry => DropEntryRow.From(entry, ItemNameOf(entry.ItemId, items))));

    /// <summary>A table with nothing typed yet.</summary>
    public static DropTableViewModel New() => new("", []) { IsNew = true };

    /// <summary>
    /// True while the table was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    public ObservableCollection<DropEntryRow> Entries { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    public partial DropEntryRow? SelectedEntry { get; set; }

    /// <summary>The list's line: the id, or a placeholder while a new table has none.</summary>
    public string Label => Id.Length > 0 ? Id : "(new table)";

    /// <summary>
    /// Why the table can't be built: the typed id while new, then the first row's problem as "entries[2] (potion):
    /// Chance must be a whole number (was 'x')."; null when it can.
    /// </summary>
    public string? Problem => (IsNew ? ObjectIds.ProblemOf(Id, []) : null) ?? FirstRowProblem();

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public DropTable ToTable() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new DropTable(Id, [.. Entries.Select(row => row.ToEntry())]);

    private string? FirstRowProblem() =>
        Entries.FirstOrDefault(row => row.Problem is not null) is { } row
            ? $"entries[{row.Index}] ({row.ItemId}): {row.Problem}"
            : null;

    private static string ItemNameOf(string itemId, IReadOnlyDictionary<string, ItemDefinition> items) =>
        items.TryGetValue(itemId, out var item) ? item.Name : itemId;

    // Each row shows its position, which Data's messages name.
    private void Renumber()
    {
        for (var index = 0; index < Entries.Count; index++)
        {
            Entries[index].Index = index;
        }
    }
}
