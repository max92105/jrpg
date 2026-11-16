using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Trade;
using Jrpg.Editor.Events;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Trade;

/// <summary>
/// One shop as the Shops page edits it (ADR 0045): its id (typed while the shop is new, fixed once the draft holds it,
/// decision 582), its name and the items it sells in order, added after the chosen one from the picker's priced items.
/// <see cref="Problem"/> is only the typed id's: the name and the items are Data's rules, shown by the list.
/// </summary>
internal sealed partial class ShopViewModel : ObservableObject
{
    private ShopViewModel(string id, IEnumerable<string> itemIds)
    {
        Id = id;
        ItemIds = [.. itemIds];
        ItemIds.CollectionChanged += (_, _) => AddItemCommand.NotifyCanExecuteChanged();
        SelectedItem = ItemIds.FirstOrDefault();
    }

    public static ShopViewModel From(ShopDefinition shop) => new(shop.Id, shop.ItemIds) { Name = shop.Name };

    /// <summary>A shop with nothing typed yet.</summary>
    public static ShopViewModel New() => new("", []) { IsNew = true };

    /// <summary>True while the shop was added this session: the only time its id can be typed (decision 582).</summary>
    public bool IsNew { get; private init; }

    public ObservableCollection<string> ItemIds { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveItemCommand), nameof(MoveItemUpCommand), nameof(MoveItemDownCommand))]
    public partial string? SelectedItem { get; set; }

    /// <summary>The picker's choice, added by Add item; null while nothing is picked.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemCommand))]
    public partial PricedItemChoice? ItemToAdd { get; set; }

    /// <summary>The list's line: "general_store: General Store", or a placeholder while a new shop has no id.</summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Name}" : "(new shop)";

    /// <summary>Why the typed id can't be used; null for a saved shop or a usable id.</summary>
    public string? Problem => IsNew ? ObjectIds.ProblemOf(Id, []) : null;

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public ShopDefinition ToShop() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : new ShopDefinition(Id, Name.Trim(), [.. ItemIds]);

    // After the chosen item, or last when none is chosen; a shop lists an item once (Data refuses a doubled one).
    [RelayCommand(CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        var itemId = ItemToAdd!.Id;
        ItemIds.Insert(SelectedItem is null ? ItemIds.Count : ItemIds.IndexOf(SelectedItem) + 1, itemId);
        SelectedItem = itemId;
    }

    private bool CanAddItem() => ItemToAdd is { } item && !ItemIds.Contains(item.Id);

    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    private void RemoveItem() => SelectedItem = ListOrder.Remove(ItemIds, SelectedItem!);

    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    private void MoveItemUp() => ListOrder.MoveBy(ItemIds, SelectedItem!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    private void MoveItemDown() => ListOrder.MoveBy(ItemIds, SelectedItem!, 1);

    private bool HasSelectedItem() => SelectedItem is not null;
}
