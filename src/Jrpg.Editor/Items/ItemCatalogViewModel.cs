using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Items;

/// <summary>
/// Every item as the Items page edits it (decision 877): the draft's items by category then id, the ones added this
/// session after them; Add starts an item of the category its button names, Duplicate copies the chosen one with its
/// category (decision 878); only an added one can be removed, since shops, tables and maps name a saved one (decision
/// 581). <see cref="Problem"/> is the chosen item's own first, then any item's, then Data's message for the items and
/// everything that reads them (decision 863). The icon box offers the icons folder's images, a note naming the chosen
/// item's icon when the folder lacks it (decisions 879, 442).
/// </summary>
internal sealed partial class ItemCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    // rules.json's one sale share (decision 864), which every item's sale line reads.
    private readonly int _sellPercent;

    /// <param name="content">
    /// The draft as it is: the items edited, the affix pool the uniques name, and the rules, shops, drop tables and new
    /// game that name the items.
    /// </param>
    /// <param name="iconFiles">The PNGs in the icons folder as the session starts, sorted.</param>
    public ItemCatalogViewModel(ContentCatalog content, IReadOnlyList<string> iconFiles)
    {
        _content = content;
        IconFiles = iconFiles;
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

    public IReadOnlyList<string> IconFiles { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(MissingIconNote))]
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

    /// <summary>
    /// "Weapon.png isn't in art/icons: ...", the chosen item's icon when the folder lacks it: kept and saved (decision
    /// 442), refused by the game's start; empty when the folder has it or no item is chosen. The game reads the file by
    /// its path, which Windows matches ignoring case, so the folder's names are too.
    /// </summary>
    public string MissingIconNote =>
        SelectedItem is not { IconFile: var icon } || IconFiles.Contains(icon, StringComparer.OrdinalIgnoreCase) ? ""
        : icon.Length == 0 ? $"No icon: the game will refuse to start until one of art/{ArtFolders.Icons} is named."
        : $"{icon} isn't in art/{ArtFolders.Icons}: the game will refuse to start until it is.";

    /// <summary>The chosen item's id; null with none chosen or while a new one has no id yet.</summary>
    public string? ChosenKey => SelectedItem is { Id.Length: > 0 } item ? item.Id : null;

    public void Choose(string key) => SelectedItem = Items.FirstOrDefault(item => item.Id == key) ?? SelectedItem;

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
    // The chosen item's icon is among them, so its note is read again.
    private void OnItemChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Problem));
        OnPropertyChanged(nameof(MissingIconNote));
    }
}
