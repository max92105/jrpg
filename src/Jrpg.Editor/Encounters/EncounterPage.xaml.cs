using System.Windows;
using System.Windows.Controls;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// The Encounters page (decisions 905, 906): the encounters left, the chosen one's id, its enemies numbered by slot,
/// its Background and Music boxes and the maps rolling it right. Its host shows the problem line.
/// </summary>
internal sealed partial class EncounterPage : UserControl
{
    private readonly EncounterCatalogViewModel _encounters;

    public EncounterPage(EncounterCatalogViewModel encounters)
    {
        InitializeComponent();
        DataContext = _encounters = encounters;
    }

    // The button is greyed while the chosen encounter can't be linked, so the dialog always has one to link.
    private void OnLinkMapClicked(object sender, RoutedEventArgs e)
    {
        var link = _encounters.StartLinkingMap();
        if (new MapLinkWindow(link) { Owner = Window.GetWindow(this) }.ShowDialog() == true)
        {
            _encounters.LinkMap(link);
        }
    }
}
