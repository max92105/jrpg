using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Quests;

/// <summary>
/// Editing the draft's quests through the Quests page (decision 394): the page starts from the draft as it is and the
/// whole session is applied as one undo step, as <see cref="Events.EventEditingViewModel"/> applies a dialog.
/// </summary>
internal sealed class QuestEditingViewModel(CatalogDraft catalog)
{
    /// <summary>
    /// The page's state from the draft as it is now, so a quest undone or saved since is read right.
    /// </summary>
    public QuestCatalogViewModel Start() => new(catalog.Current.Quests.Values);

    /// <summary>Gives the draft the page's quests; nothing when they didn't change.</summary>
    public void Apply(QuestCatalogViewModel edited) => catalog.SetQuests(edited.ToQuests());
}
