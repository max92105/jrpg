using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Items;

/// <summary>
/// Every item as the Items page edits it (decision 877): the draft's items by category then id, the ones added this
/// session after them; Add starts an item of the category its button names, Duplicate copies the chosen one with its
/// category (decision 878); only an added one can be removed, since shops, tables and maps name a saved one (decision
/// 581). <see cref="Problem"/> is the chosen item's own first, then any item's, then Data's message for the items and
/// everything that reads them (decision 863).
/// </summary>
internal sealed partial class ItemCatalogViewModel : ObservableObject
{
    private readonly ContentCatalog _content;

    // rules.json's one sale share (decision 864), which every item's sale line reads.
    private readonly int _sellPercent;

    /// <param name="content">
    /// The draft as it is: the items edited, the affix pool the uniques name, and the rules, shops, drop tables and new
    /// game that name the items.
    /// </param>
    public ItemCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        _sellPercent = content.Rules.Trade.SellPercent;
        Items =
        [
            .. content.Items.Values
                .OrderBy(ItemCategories.Of)
                .ThenBy(item => item.Id, StringComparer.Ordinal)
                .Select(item => ItemViewModel.From(item, _sellPercent)),
        ];
        foreach (var item in Items)
        {
            item.Changed += OnItemChanged;
        }

        SelectedItem = Items.FirstOrDefault();
    }

    public ObservableCollection<ItemViewModel> Items { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateCommand), nameof(RemoveItemCommand))]
    public partial ItemViewModel? SelectedItem { get; set; }

    /// <summary>
    /// Why the game would refuse the items as typed: the chosen item's boxes first, as the page shows them, then
    /// another item's named by id (by position while it has none), then Data's; null when the game would take them.
    /// </summary>
    public string? Problem =>
        SelectedItem?.Problem
        ?? RecordListProblem.FirstOf(Items, item => item.Id, item => item.Problem, "Item")
        ?? CatalogProblems.OfItems([.. Items.Select(item => item.ToItem())], _content);

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<ItemDefinition> ToItems() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Items.Select(item => item.ToItem())];

    /// <summary>Adds an item of the button's category last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddItem(ItemCategory category) =>
        Insert(ItemViewModel.New(ItemCategories.StartingRecord(category), _sellPercent));

    /// <summary>Adds a copy of the chosen item last, as typed, its id left to type (decision 585).</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    private void Duplicate() => Insert(SelectedItem!.Duplicate());

    [RelayCommand(CanExecute = nameof(CanRemoveItem))]
    private void RemoveItem()
    {
        var removed = SelectedItem!;
        removed.Changed -= OnItemChanged;
        SelectedItem = ListOrder.Remove(Items, removed);
    }

    private bool HasSelectedItem() => SelectedItem is not null;

    private bool CanRemoveItem() => SelectedItem is { IsNew: true };

    private void Insert(ItemViewModel item)
    {
        item.Changed += OnItemChanged;
        Items.Add(item);
        SelectedItem = item;
    }

    // Only the typed boxes reach here, never an item's label or problem notices, so Data's check runs once per change.
    private void OnItemChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Problem));
}
