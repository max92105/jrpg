using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Editor.Events;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Loot;

/// <summary>
/// One drop table as the Drop tables page edits it (ADR 0045): its id (typed while the table is new, fixed once the
/// draft holds it, decision 582) and its entry rows in the roll's order, numbered from 0 as Data numbers them, added
/// after the chosen one from the item box, removed and moved. <see cref="Problem"/> is the typed id's or a row's box
/// that isn't a number; the rest is Data's, shown by the list. <see cref="EntriesChanged"/> tells the list of each
/// change to the entries, typed or listed.
/// </summary>
internal sealed partial class DropTableViewModel : ObservableObject
{
    private DropTableViewModel(string id, IEnumerable<DropEntryRow> entries)
    {
        Id = id;
        Entries = [.. entries];
        foreach (var row in Entries)
        {
            row.PropertyChanged += OnRowChanged;
        }

        Entries.CollectionChanged += OnEntriesListChanged;
        Renumber();
        SelectedEntry = Entries.FirstOrDefault();
    }

    /// <summary>
    /// A row's chance or quantity typed, or a row added, removed or moved; never a row's own label, number or problem
    /// notice, so the list re-checks once per change (<c>.claude/rules/editor.md</c>).
    /// </summary>
    public event EventHandler? EntriesChanged;

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
    [NotifyCanExecuteChangedFor(nameof(RemoveEntryCommand), nameof(MoveEntryUpCommand), nameof(MoveEntryDownCommand))]
    public partial DropEntryRow? SelectedEntry { get; set; }

    /// <summary>The item box's choice, added by Add; null while nothing is picked.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddEntryCommand))]
    public partial DropItemChoice? ItemToAdd { get; set; }

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

    // After the chosen entry, or last when none is chosen; an item may be listed twice, as Data allows.
    [RelayCommand(CanExecute = nameof(HasItemToAdd))]
    private void AddEntry()
    {
        var row = DropEntryRow.New(ItemToAdd!);
        Entries.Insert(SelectedEntry is null ? Entries.Count : Entries.IndexOf(SelectedEntry) + 1, row);
        SelectedEntry = row;
    }

    private bool HasItemToAdd() => ItemToAdd is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedEntry))]
    private void RemoveEntry() => SelectedEntry = ListOrder.Remove(Entries, SelectedEntry!);

    [RelayCommand(CanExecute = nameof(HasSelectedEntry))]
    private void MoveEntryUp() => ListOrder.MoveBy(Entries, SelectedEntry!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedEntry))]
    private void MoveEntryDown() => ListOrder.MoveBy(Entries, SelectedEntry!, 1);

    private bool HasSelectedEntry() => SelectedEntry is not null;

    private string? FirstRowProblem() =>
        Entries.FirstOrDefault(row => row.Problem is not null) is { } row
            ? $"entries[{row.Index}] ({row.ItemId}): {row.Problem}"
            : null;

    private static string ItemNameOf(string itemId, IReadOnlyDictionary<string, ItemDefinition> items) =>
        items.TryGetValue(itemId, out var item) ? item.Name : itemId;

    // Each row's number is Data's entries[i], so any change to the list renumbers; a Move lists its row as both removed
    // and added, so the row ends up watched once.
    private void OnEntriesListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var row in e.OldItems?.OfType<DropEntryRow>() ?? [])
        {
            row.PropertyChanged -= OnRowChanged;
        }

        foreach (var row in e.NewItems?.OfType<DropEntryRow>() ?? [])
        {
            row.PropertyChanged += OnRowChanged;
        }

        Renumber();
        EntriesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DropEntryRow.ChanceText) or nameof(DropEntryRow.MinQuantityText)
            or nameof(DropEntryRow.MaxQuantityText))
        {
            EntriesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Each row shows its position, which Data's messages name.
    private void Renumber()
    {
        for (var index = 0; index < Entries.Count; index++)
        {
            Entries[index].Index = index;
        }
    }
}
