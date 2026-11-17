using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Backgrounds;

/// <summary>
/// Editing the draft's animated background records through the Battle backgrounds page (decision 901): the page starts
/// from the draft and the backgrounds folder as they are, and the whole session is applied as one undo step.
/// </summary>
/// <param name="catalog">The draft the records come from and go back to.</param>
/// <param name="listImages">The backgrounds folder's readable images, read when the page opens.</param>
/// <param name="sizeOf">A backgrounds image's size by file name; null when the folder lacks it.</param>
internal sealed class BattleBackgroundEditingViewModel(
    CatalogDraft catalog, Func<IReadOnlyList<string>> listImages, Func<string, (int Width, int Height)?> sizeOf)
    : ICatalogEditing<BattleBackgroundCatalogViewModel>
{
    /// <summary>The page's state from the draft and the folder as they are now.</summary>
    public BattleBackgroundCatalogViewModel Start() => new(catalog.Current, listImages(), sizeOf);

    /// <summary>Gives the draft the page's records; nothing when they didn't change.</summary>
    public void Apply(BattleBackgroundCatalogViewModel edited) => catalog.SetAnimatedBackgrounds(edited.ToRecords());
}
