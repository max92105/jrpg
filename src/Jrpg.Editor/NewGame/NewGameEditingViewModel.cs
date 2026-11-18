using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.NewGame;

/// <summary>
/// Editing the draft's new game through the New game page (decision 968): the page starts from the draft as it is and
/// the whole session is applied as one undo step, as <see cref="Party.PartyMemberEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the new game is read from and applied to.</param>
internal sealed class NewGameEditingViewModel(CatalogDraft catalog) : ICatalogEditing<NewGameViewModel>
{
    public NewGameViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's new game; nothing when it didn't change.</summary>
    public void Apply(NewGameViewModel edited) => catalog.SetNewGame(edited.ToNewGame());
}
