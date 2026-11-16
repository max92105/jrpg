using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Loot;

/// <summary>
/// Every drop table as the Drop tables page edits it (ADR 0045): the draft's tables in id order, the ones added this
/// session after them; only an added one can be removed (decisions 581, 607); the entry box offers every item by name;
/// <see cref="RolledBy"/> says which enemies share the chosen table (decision 603). <see cref="Problem"/> is a table's
/// own problem first, then Data's message for the whole list (decision 580). A table is chosen by its id.
/// </summary>
internal sealed partial class DropTableCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    /// <param name="content">
    /// The draft as it is: the tables edited, the items they name, the enemies that roll them.
    /// </param>
    public DropTableCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        OfferedItems = [.. content.Items.Values
            .OrderBy(item => item.Name, StringComparer.Ordinal)
            .ThenBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => new DropItemChoice(item.Id, item.Name))];
        Tables = [.. content.DropTables.Values
            .OrderBy(table => table.Id, StringComparer.Ordinal)
            .Select(table => DropTableViewModel.From(table, content.Items))];
        foreach (var table in Tables)
        {
            Watch(table);
        }

        SelectedTable = Tables.FirstOrDefault();
    }

    public ObservableCollection<DropTableViewModel> Tables { get; }

    /// <summary>The items an entry may give: every item, by name then id.</summary>
    public IReadOnlyList<DropItemChoice> OfferedItems { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveTableCommand))]
    [NotifyPropertyChangedFor(nameof(RolledBy))]
    public partial DropTableViewModel? SelectedTable { get; set; }

    /// <summary>
    /// "Rolled by: goblin, goblin_chief", the draft's enemies naming the chosen table by id, or "Rolled by no enemy";
    /// empty for a table added this session (no enemy can name it yet) or while none is chosen.
    /// </summary>
    public string RolledBy
    {
        get
        {
            if (SelectedTable is not { IsNew: false } table)
            {
                return "";
            }

            var enemies = _content.Enemies.Values
                .Where(enemy => enemy.DropTableId == table.Id)
                .Select(enemy => enemy.Id)
                .Order(StringComparer.Ordinal)
                .ToList();
            return enemies.Count == 0 ? "Rolled by no enemy" : $"Rolled by: {string.Join(", ", enemies)}";
        }
    }

    /// <summary>
    /// Why the game would refuse the tables as typed, naming the table by id (by position while it has none); null when
    /// the game would take them.
    /// </summary>
    public string? Problem =>
        RecordListProblem.FirstOf(Tables, table => table.Id, table => table.Problem, "Drop table")
        ?? CatalogProblems.OfDropTables([.. Tables.Select(table => table.ToTable())], _content);

    public string? ChosenKey => SelectedTable?.Id;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<DropTable> ToTables() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Tables.Select(table => table.ToTable())];

    /// <summary>
    /// The enemy's table when the page opens from the Enemies page's Edit...; an enemy with none passes "", which keeps
    /// the first chosen.
    /// </summary>
    public void Choose(string key) => SelectedTable = Tables.FirstOrDefault(table => table.Id == key) ?? SelectedTable;

    /// <summary>Adds a table with nothing typed yet, last, and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddTable()
    {
        var table = DropTableViewModel.New();
        Watch(table);
        Tables.Add(table);
        SelectedTable = table;
        RaiseProblemChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveTable))]
    private void RemoveTable()
    {
        var removed = SelectedTable!;
        removed.PropertyChanged -= OnTableChanged;
        removed.EntriesChanged -= OnTableEntriesChanged;
        SelectedTable = ListOrder.Remove(Tables, removed);
        RaiseProblemChanged();
    }

    private bool CanRemoveTable() => SelectedTable is { IsNew: true };

    // A table's typed id and its entries feed the problem line. Its Label and Problem notices arrive here too and are
    // dropped: the line rereads every table.
    private void Watch(DropTableViewModel table)
    {
        table.PropertyChanged += OnTableChanged;
        table.EntriesChanged += OnTableEntriesChanged;
    }

    private void OnTableChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DropTableViewModel.Id))
        {
            RaiseProblemChanged();
        }
    }

    private void OnTableEntriesChanged(object? sender, EventArgs e) => RaiseProblemChanged();

    private void RaiseProblemChanged() => OnPropertyChanged(nameof(Problem));
}
