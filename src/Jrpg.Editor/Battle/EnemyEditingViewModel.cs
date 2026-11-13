using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Battle;

/// <summary>
/// Editing the draft's enemies through the Enemies window (ADR 0045): the window starts from the draft as it is and the
/// whole session is applied as one undo step (decision 591), as <see cref="Trade.ShopEditingViewModel"/> does; a Drop
/// tables window opened over it applies its own step first, and the window then takes the draft's tables again.
/// </summary>
/// <param name="catalog">The draft the window edits.</param>
/// <param name="artImagesIn">The PNG names of an art folder by name, built by the shell over the art root.</param>
internal sealed class EnemyEditingViewModel(CatalogDraft catalog, Func<string, IReadOnlyList<string>> artImagesIn)
{
    /// <summary>
    /// The window's state from the draft as it is now, so an enemy undone or saved since is read right.
    /// </summary>
    public EnemyCatalogViewModel Start() => new(catalog.Current, artImagesIn);

    /// <summary>Gives the draft the window's enemies; nothing when they didn't change.</summary>
    public void Apply(EnemyCatalogViewModel edited) => catalog.SetEnemies(edited.ToEnemies());

    /// <summary>
    /// After a Drop tables window opened over the Enemies window applied its own step (decision 602): the window's Drop
    /// table box and problem line take the draft's tables.
    /// </summary>
    public void RefreshDropTables(EnemyCatalogViewModel edited) => edited.TakeDropTables(catalog.Current);
}
