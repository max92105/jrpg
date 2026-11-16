using System.Windows.Controls;

namespace Jrpg.Editor.Quests;

/// <summary>
/// The Quests page (decision 394): the quests left, the chosen quest and its stages right. Its host shows the problem
/// line and keeps or drops the session.
/// </summary>
internal sealed partial class QuestCatalogPage : UserControl
{
    public QuestCatalogPage(QuestCatalogViewModel quests)
    {
        InitializeComponent();
        DataContext = quests;
    }
}
