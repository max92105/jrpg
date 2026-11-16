using System.Windows.Controls;

namespace Jrpg.Editor.Trade;

/// <summary>
/// The Shops page (ADR 0045): the shops left, the chosen shop and its items right. Its host shows the problem line and
/// keeps or drops the session.
/// </summary>
internal sealed partial class ShopCatalogPage : UserControl
{
    public ShopCatalogPage(ShopCatalogViewModel shops)
    {
        InitializeComponent();
        DataContext = shops;
    }
}
