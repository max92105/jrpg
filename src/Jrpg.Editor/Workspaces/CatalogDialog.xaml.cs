using System.Windows;

namespace Jrpg.Editor.Workspaces;

/// <summary>
/// Hosts one Database page modally (stages 1 and 2 of the mode bar plan): OK keeps the page's session when nothing
/// would be refused, Cancel drops it (ADR 0045). The page knows nothing of its host, so the problem line and the
/// buttons are the dialog's.
/// </summary>
internal sealed partial class CatalogDialog : Window
{
    public CatalogDialog(string title, FrameworkElement page, IEditedCatalog edited)
    {
        InitializeComponent();
        Title = title;
        PageHost.Content = page;
        DataContext = edited;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
