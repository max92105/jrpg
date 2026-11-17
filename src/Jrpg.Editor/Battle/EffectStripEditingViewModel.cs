using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Battle;

/// <summary>
/// Editing the draft's effect strip records through the Effect strips page (decision 659, ADR 0045): the page starts
/// from the draft and the effects folder as they are, and the whole session is applied as one undo step.
/// </summary>
/// <param name="catalog">The draft the records come from and go back to.</param>
/// <param name="listImages">The effects folder's readable images, read when the page opens.</param>
/// <param name="sizeOf">An effects image's size by file name; null when the folder lacks it.</param>
internal sealed class EffectStripEditingViewModel(
    CatalogDraft catalog, Func<IReadOnlyList<string>> listImages, Func<string, (int Width, int Height)?> sizeOf)
    : ICatalogEditing<EffectStripCatalogViewModel>
{
    /// <summary>The page's state from the draft and the folder as they are now.</summary>
    public EffectStripCatalogViewModel Start() => new(catalog.Current, listImages(), sizeOf);

    /// <summary>Gives the draft the page's records; nothing when they didn't change.</summary>
    public void Apply(EffectStripCatalogViewModel edited) => catalog.SetEffectStrips(edited.ToStrips());
}
