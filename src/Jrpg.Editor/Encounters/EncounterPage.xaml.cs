using System.Windows.Controls;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// The Encounters page (decisions 905, 906): the encounters left, the chosen one's id, its enemies numbered by slot,
/// and its Background and Music boxes right. Its host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class EncounterPage : UserControl
{
    public EncounterPage(EncounterCatalogViewModel encounters)
    {
        InitializeComponent();
        DataContext = encounters;
    }
}
