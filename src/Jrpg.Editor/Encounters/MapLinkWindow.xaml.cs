using System.Windows;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// The Encounters page's Link dialog: a map picked, and its odds typed when it had no random battles. Link closes it
/// with a true result; the page then links the map.
/// </summary>
internal sealed partial class MapLinkWindow : Window
{
    public MapLinkWindow(MapLinkViewModel link)
    {
        InitializeComponent();
        DataContext = link;
        Loaded += (_, _) => MapBox.Focus();
    }

    private void OnLinkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
