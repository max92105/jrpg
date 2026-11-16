using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Trade;

/// <summary>
/// Every shop as the Shops page edits it (ADR 0045): the draft's shops in id order, the ones added this session after
/// them; only an added one can be removed, since a map's event may open a saved one (decision 581). The picker offers
/// the priced items by name (decision 584). <see cref="Problem"/> is a shop's own problem first, then Data's message
/// for the whole list (decision 580). A shop is chosen by its id.
/// </summary>
internal sealed partial class ShopCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    /// <param name="content">The draft as it is: the shops edited and the items they may sell.</param>
    public ShopCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        Shops = [.. content.Shops.Values.OrderBy(shop => shop.Id, StringComparer.Ordinal).Select(ShopViewModel.From)];
        foreach (var shop in Shops)
        {
            Watch(shop);
        }

        OfferedItems = [.. content.Items.Values
            .Where(item => item.Price is not null)
            .OrderBy(item => item.Name, StringComparer.Ordinal)
            .ThenBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => new PricedItemChoice(item.Id, item.Name, item.Price!.Value))];
        SelectedShop = Shops.FirstOrDefault();
    }

    public ObservableCollection<ShopViewModel> Shops { get; }

    /// <summary>The items a shop may sell: the priced ones, by name.</summary>
    public IReadOnlyList<PricedItemChoice> OfferedItems { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveShopCommand))]
    public partial ShopViewModel? SelectedShop { get; set; }

    /// <summary>
    /// Why the game would refuse the shops as typed, naming the shop by id (by position while it has none); null when
    /// the game would take them.
    /// </summary>
    public string? Problem =>
        RecordListProblem.FirstOf(Shops, shop => shop.Id, shop => shop.Problem, "Shop")
        ?? CatalogProblems.OfShops([.. Shops.Select(shop => shop.ToShop())], _content);

    public string? ChosenKey => SelectedShop?.Id;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<ShopDefinition> ToShops() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Shops.Select(shop => shop.ToShop())];

    public void Choose(string key) => SelectedShop = Shops.FirstOrDefault(shop => shop.Id == key) ?? SelectedShop;

    /// <summary>Adds a shop with nothing typed yet and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddShop()
    {
        var shop = ShopViewModel.New();
        Watch(shop);
        Shops.Add(shop);
        SelectedShop = shop;
        RaiseProblemChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveShop))]
    private void RemoveShop()
    {
        var removed = SelectedShop!;
        removed.PropertyChanged -= OnShopChanged;
        removed.ItemIds.CollectionChanged -= OnShopItemsChanged;
        SelectedShop = ListOrder.Remove(Shops, removed);
        RaiseProblemChanged();
    }

    private bool CanRemoveShop() => SelectedShop is { IsNew: true };

    // A shop's typed fields and its item list both feed the problem line.
    private void Watch(ShopViewModel shop)
    {
        shop.PropertyChanged += OnShopChanged;
        shop.ItemIds.CollectionChanged += OnShopItemsChanged;
    }

    // Only the typed fields feed the problem; the selection and the picker's choice don't.
    private void OnShopChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ShopViewModel.Id) or nameof(ShopViewModel.Name))
        {
            RaiseProblemChanged();
        }
    }

    private void OnShopItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RaiseProblemChanged();

    private void RaiseProblemChanged() => OnPropertyChanged(nameof(Problem));
}
