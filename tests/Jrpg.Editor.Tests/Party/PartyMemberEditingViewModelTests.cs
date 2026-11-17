using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.PartyMemberFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Party members page starts from the draft as it is now, and its whole session is one undo step (decision 954).
public sealed class PartyMemberEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(World);

    private readonly PartyMemberEditingViewModel _editing;

    public PartyMemberEditingViewModelTests() => _editing = new PartyMemberEditingViewModel(_draft, ImagesIn);

    [Fact]
    public void Apply_EditedName_ChangesTheDraft()
    {
        var edited = _editing.Start();
        edited.Members.Single(member => member.Id == Scholar.Id).Name = "Elsa";

        _editing.Apply(edited);

        Assert.Equal("Elsa", _draft.Current.PartyMembers[Scholar.Id].Name);
    }

    // A member applied restarts as saved, so Remove can't take away one a map may recruit since: only the undo can,
    // which refuses while a map names it (decision 954).
    [Fact]
    public void Start_AfterApplyingAnAddedMember_CannotRemoveIt()
    {
        var edited = _editing.Start();
        edited.AddMemberCommand.Execute(null);
        var added = edited.SelectedMember!;
        (added.Id, added.Name, added.ClassId) = ("bram", "Bram", Scholar.ClassId);
        (added.Summary, added.PortraitFile) = ("A guard.", "IlsaPortrait.png");
        (added.BattleSpriteFile, added.AttackEffectFile) = ("IlsaBattler.png", "InkSplash.png");
        _editing.Apply(edited);

        var restarted = _editing.Start();
        restarted.Choose("bram");

        Assert.False(restarted.RemoveMemberCommand.CanExecute(null));
    }
}
