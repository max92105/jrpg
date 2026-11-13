using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Battle;

/// <summary>
/// Editing the draft's enemies through the Enemies window (ADR 0045): the window starts from the draft as it is and the
/// whole session is applied as one undo step (decision 591), as <see cref="Trade.ShopEditingViewModel"/> does.
/// </summary>
internal sealed class EnemyEditingViewModel(CatalogDraft catalog)
{
    /// <summary>
    /// The window's state from the draft as it is now, so an enemy undone or saved since is read right.
    /// </summary>
    public EnemyCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the window's enemies; nothing when they didn't change.</summary>
    public void Apply(EnemyCatalogViewModel edited) => catalog.SetEnemies(edited.ToEnemies());
}
