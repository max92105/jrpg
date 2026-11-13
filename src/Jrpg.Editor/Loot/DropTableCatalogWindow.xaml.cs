using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jrpg.Editor.Loot;

/// <summary>
/// The Drop tables window (ADR 0045, decision 596): OK keeps the session's tables when nothing would be refused, Cancel
/// drops them. The problem line shows live and greys OK while it shows.
/// </summary>
internal sealed partial class DropTableCatalogWindow : Window
{
    public DropTableCatalogWindow(DropTableCatalogViewModel tables)
    {
        InitializeComponent();
        DataContext = tables;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;

    // Selected on focus rather than through an IsKeyboardFocusWithin trigger: the trigger lets go as soon as a button
    // takes the focus, and a move rebuilds the row's container, so Up, Down and Remove would act on another row.
    private void OnEntryRowFocused(object sender, KeyboardFocusChangedEventArgs e) => ((ListBoxItem)sender).IsSelected = true;
}
