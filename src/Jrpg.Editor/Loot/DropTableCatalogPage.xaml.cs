using System.Windows.Controls;
using System.Windows.Input;

namespace Jrpg.Editor.Loot;

/// <summary>
/// The Drop tables page (ADR 0045, decision 596): the tables left, the chosen table and its entries right. Its host
/// shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class DropTableCatalogPage : UserControl
{
    public DropTableCatalogPage(DropTableCatalogViewModel tables)
    {
        InitializeComponent();
        DataContext = tables;
    }

    // Selected on focus rather than through an IsKeyboardFocusWithin trigger: the trigger lets go as soon as a button
    // takes the focus, and a move rebuilds the row's container, so Up, Down and Remove would act on another row.
    private void OnEntryRowFocused(object sender, KeyboardFocusChangedEventArgs e) => ((ListBoxItem)sender).IsSelected = true;
}
