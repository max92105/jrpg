using System.Windows;

namespace Jrpg.Editor.Quests;

/// <summary>
/// The Quests window (decision 394): OK keeps the session's quests when nothing would be refused, Cancel drops them.
/// </summary>
internal sealed partial class QuestCatalogWindow : Window
{
    private readonly QuestCatalogViewModel _quests;

    public QuestCatalogWindow(QuestCatalogViewModel quests)
    {
        InitializeComponent();
        DataContext = _quests = quests;
    }

    // A problem keeps the window open and says which quest and stage it is in.
    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        ProblemText.Text = _quests.Problem;
        if (_quests.Problem is null)
        {
            DialogResult = true;
        }
    }
}
