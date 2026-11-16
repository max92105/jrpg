using Jrpg.Editor.Quests;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Database > Quests...: the Quests window edits the draft's quests, and OK applies the whole session as one undo step
/// (decision 394).
/// </summary>
internal sealed partial class MainWindow
{
    private void OpenQuests()
    {
        if (_viewModel.QuestEditing is not { } questEditing)
        {
            return;
        }

        var edited = questEditing.Start();
        var window = new QuestCatalogWindow(edited) { Owner = this };
        DialogBounds.FitTo(window, this);
        if (window.ShowDialog() == true)
        {
            questEditing.Apply(edited);
        }
    }
}
