using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Statuses;

/// <summary>
/// Editing the draft's statuses through the Statuses page (ADR 0045): the page starts from the draft as it is and the
/// whole session is applied as one undo step, every skill and item granting a status following it (decision 975), as
/// <see cref="Effects.TieredEffectEditingViewModel"/> does for the tiered effects.
/// </summary>
/// <param name="catalog">The draft the page edits.</param>
internal sealed class StatusEditingViewModel(CatalogDraft catalog) : ICatalogEditing<StatusCatalogViewModel>
{
    /// <summary>The page's state from the draft as it is now, so a status undone since shows on the page.</summary>
    public StatusCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's statuses; nothing when they didn't change.</summary>
    public void Apply(StatusCatalogViewModel edited) => catalog.SetStatuses(edited.ToStatuses());
}
