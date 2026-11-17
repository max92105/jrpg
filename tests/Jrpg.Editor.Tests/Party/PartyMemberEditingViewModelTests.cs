using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.PartyMemberFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Party members page starts from the draft as it is now, and its whole session is one undo step (decision 954).
public sealed class PartyMemberEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(World);

    private readonly PartyMemberEditingViewModel _editing;

    public PartyMemberEditingViewModelTests() => _editing = new PartyMemberEditingViewModel(_draft);

    [Fact]
    public void Apply_EditedName_ChangesTheDraft()
    {
        var edited = _editing.Start();
        edited.Members.Single(member => member.Id == Scholar.Id).Name = "Elsa";

        _editing.Apply(edited);

        Assert.Equal("Elsa", _draft.Current.PartyMembers[Scholar.Id].Name);
    }
}
