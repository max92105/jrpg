using System.Windows;
using Jrpg.Editor.Quests;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Quests menu: the Quests window edits the draft's quests, and OK applies the whole session as one undo step
/// (decision 394).
/// </summary>
internal sealed partial class MainWindow
{
    private void OnEditQuestsClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.QuestEditing is not { } questEditing)
        {
            return;
        }

        var edited = questEditing.Start();
        if (new QuestCatalogWindow(edited) { Owner = this }.ShowDialog() == true)
        {
            questEditing.Apply(edited);
        }
    }
}
