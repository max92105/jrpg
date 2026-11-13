using System.Windows;

namespace Jrpg.Editor.Trade;

/// <summary>
/// The Shops window (ADR 0045): OK keeps the session's shops when nothing would be refused, Cancel drops them. The
/// problem line shows live and greys OK while it shows.
/// </summary>
internal sealed partial class ShopCatalogWindow : Window
{
    public ShopCatalogWindow(ShopCatalogViewModel shops)
    {
        InitializeComponent();
        DataContext = shops;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
