using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.NewGame;

/// <summary>
/// The new game's starting items as the New game page types them (decision 968): rows of an item and its quantity, each
/// added after the chosen row from the draft's items not already listed (Data refuses one listed twice), removed and
/// moved. A quantity typed and every change of the list raise <see cref="Problem"/>'s change once.
/// </summary>
internal sealed partial class NewGameViewModel
{
    /// <summary>
    /// The items the party starts with, each with its quantity as typed; an item the draft lacks is shown as missing.
    /// The rows are compared by reference, so the moves find the chosen one even if two named the same item.
    /// </summary>
    public ObservableCollection<StartingItemRow> StartingItems { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveItemCommand), nameof(MoveItemUpCommand), nameof(MoveItemDownCommand))]
    public partial StartingItemRow? SelectedItem { get; set; }

    /// <summary>The add box's choice, added by Add; null while nothing is picked.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemCommand))]
    public partial RecordChoice? ItemToAdd { get; set; }

    /// <summary>
    /// The add box's entries: the draft's items by name, then id, the listed ones left out, since Data refuses an item
    /// the list already holds.
    /// </summary>
    public IReadOnlyList<RecordChoice> ItemsToAdd =>
        RecordChoice.ByName(
            _content.Items.Values
                .Where(item => !StartingItems.Any(row => row.Item.Id == item.Id))
                .Select(item => new RecordChoice(item.Id, item.Name)));

    // The first row's problem, in the list's order; the row's own message names the item.
    private string? StartingItemsProblem =>
        StartingItems.Select(row => row.Problem).FirstOrDefault(problem => problem is not null);

    private static ObservableCollection<StartingItemRow> StartingItemRowsOf(NewGameDefinition newGame, ContentCatalog content) =>
    [
        .. newGame.StartingItems.Select(entry => new StartingItemRow(
            RecordChoice.Of(entry.ItemId, content.Items.GetValueOrDefault(entry.ItemId)?.Name),
            TypedNumber.Text(entry.Quantity))),
    ];

    private IReadOnlyList<InventoryEntry> StartingItemsAsTyped() =>
        ListOrder.KeptIfEqual([.. StartingItems.Select(row => row.ToEntry())], _source.StartingItems);

    /// <summary>Puts the add box's item after the chosen row (last when none) as one copy, and chooses it.</summary>
    [RelayCommand(CanExecute = nameof(HasItemToAdd))]
    private void AddItem()
    {
        var row = StartingItemRow.New(ItemToAdd!);
        ListOrder.InsertAfter(StartingItems, row, SelectedItem);
        SelectedItem = row;
        ItemToAdd = null;
    }

    private bool HasItemToAdd() => ItemToAdd is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    private void RemoveItem() => SelectedItem = ListOrder.Remove(StartingItems, SelectedItem!);

    [RelayCommand(CanExecute = nameof(CanMoveItemUp))]
    private void MoveItemUp() => ListOrder.MoveBy(StartingItems, SelectedItem!, -1);

    [RelayCommand(CanExecute = nameof(CanMoveItemDown))]
    private void MoveItemDown() => ListOrder.MoveBy(StartingItems, SelectedItem!, 1);

    private bool HasSelectedItem() => SelectedItem is not null;

    private bool CanMoveItemUp() => ListOrder.CanMoveUp(StartingItems, SelectedItem);

    private bool CanMoveItemDown() => ListOrder.CanMoveDown(StartingItems, SelectedItem);

    private void WatchStartingItemRows() =>
        ListItemWatch.WatchRowsOf(
            StartingItems,
            row => row.PropertyChanged += OnStartingItemTyped,
            row => row.PropertyChanged -= OnStartingItemTyped,
            OnStartingItemsChanged);

    private void OnStartingItemsChanged()
    {
        MoveItemUpCommand.NotifyCanExecuteChanged();
        MoveItemDownCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(ItemsToAdd));
        OnPropertyChanged(nameof(Problem));
    }

    // Only the typed quantity, never the row's problem notice, so the list runs Data's check once per change.
    private void OnStartingItemTyped(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StartingItemRow.QuantityText))
        {
            OnPropertyChanged(nameof(Problem));
        }
    }
}
