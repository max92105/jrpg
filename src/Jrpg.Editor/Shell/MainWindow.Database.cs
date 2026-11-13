using System.Windows;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Database > Shops...: the window edits the draft's shops, and OK applies the whole session as one undo step (ADR
/// 0045, decision 591), as the Quests window does.
/// </summary>
internal sealed partial class MainWindow
{
    private void OnEditShopsClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.ShopEditing is not { } shopEditing)
        {
            return;
        }

        var edited = shopEditing.Start();
        if (new ShopCatalogWindow(edited) { Owner = this }.ShowDialog() == true)
        {
            shopEditing.Apply(edited);
        }
    }
}
