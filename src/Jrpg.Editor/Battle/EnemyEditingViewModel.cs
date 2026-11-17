using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Battle;

/// <summary>
/// Editing the draft's enemies through the Enemies page (ADR 0045): the page starts from the draft as it is and the
/// whole session is applied as one undo step (decision 591), as <see cref="Trade.ShopEditingViewModel"/> does; a Drop
/// tables page opened over it applies its own step first, and the Enemies page then takes the draft's tables again.
/// </summary>
/// <param name="catalog">The draft the page edits.</param>
/// <param name="artImagesIn">The PNG names of an art folder by name, built by the shell over the art root.</param>
internal sealed class EnemyEditingViewModel(CatalogDraft catalog, Func<string, IReadOnlyList<string>> artImagesIn)
    : ICatalogEditing<EnemyCatalogViewModel>
{
    /// <summary>
    /// The page's state from the draft as it is now, so an enemy undone or saved since is read right.
    /// </summary>
    public EnemyCatalogViewModel Start() => new(catalog.Current, artImagesIn);

    /// <summary>Gives the draft the page's enemies; nothing when they didn't change.</summary>
    public void Apply(EnemyCatalogViewModel edited) => catalog.SetEnemies(edited.ToEnemies());

    /// <summary>
    /// After a Drop tables page opened over the Enemies page applied its own step (decision 602): the page's Drop table
    /// box and problem line take the draft's tables.
    /// </summary>
    public void RefreshDropTables(EnemyCatalogViewModel edited) => edited.TakeDropTables(catalog.Current);
}
