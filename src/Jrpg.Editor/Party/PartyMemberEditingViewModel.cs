using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Party;

/// <summary>
/// Editing the draft's party members through the Party members page (decision 954): the page starts from the draft as
/// it is and the whole session is applied as one undo step, as <see cref="ClassEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the members are read from and applied to.</param>
internal sealed class PartyMemberEditingViewModel(CatalogDraft catalog) : ICatalogEditing<PartyMemberCatalogViewModel>
{
    public PartyMemberCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's members; nothing when they didn't change.</summary>
    public void Apply(PartyMemberCatalogViewModel edited) => catalog.SetPartyMembers(edited.ToMembers());
}
