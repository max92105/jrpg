using Jrpg.Data.Art;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.PartyMemberFixtures;

namespace Jrpg.Editor.Tests.Party;

// One member as the Party members page types it (decision 953): its boxes, the record they make, its problem and the
// Changed event the list's problem line listens to.
public sealed class PartyMemberViewModelTests
{
    private readonly PartyMemberViewModel _scholar = PartyMemberViewModel.From(Scholar);

    [Fact]
    public void From_Member_ShowsEveryField()
    {
        string[] shown =
        [
            _scholar.Id, _scholar.Name, _scholar.ClassId, _scholar.Summary, _scholar.PortraitFile,
            _scholar.BattleSpriteFile, _scholar.AttackEffectFile,
        ];

        Assert.Equal(
            [
                Scholar.Id, Scholar.Name, Scholar.ClassId, Scholar.Summary, Scholar.PortraitFile,
                Scholar.BattleSpriteFile, Scholar.AttackEffectFile,
            ],
            shown);
    }

    [Fact]
    public void ToMember_Untouched_EqualsTheSource() => Assert.Equal(Scholar, _scholar.ToMember());

    [Fact]
    public void ToMember_TypedSummary_SetsIt()
    {
        _scholar.Summary = "Reads in the dark.";

        Assert.Equal(Scholar with { Summary = "Reads in the dark." }, _scholar.ToMember());
    }

    [Fact]
    public void ToMember_TypedNameWithSpaces_TrimsIt()
    {
        _scholar.Name = "  Isla ";

        Assert.Equal("Isla", _scholar.ToMember().Name);
    }

    [Fact]
    public void ToMember_TypedArtNameWithSpaces_TrimsIt()
    {
        _scholar.PortraitFile = " A.png  ";
        _scholar.BattleSpriteFile = "  B.png";
        _scholar.AttackEffectFile = "C.png ";

        var written = _scholar.ToMember();

        Assert.Equal(("A.png", "B.png", "C.png"), (written.PortraitFile, written.BattleSpriteFile, written.AttackEffectFile));
    }

    [Fact]
    public void ToMember_TypedArtNames_SetsAllThree()
    {
        _scholar.PortraitFile = "A.png";
        _scholar.BattleSpriteFile = "B.png";
        _scholar.AttackEffectFile = "C.png";

        var written = _scholar.ToMember();

        Assert.Equal(("A.png", "B.png", "C.png"), (written.PortraitFile, written.BattleSpriteFile, written.AttackEffectFile));
    }

    [Fact]
    public void Problem_NoClass_SaysClassMustBeChosen()
    {
        _scholar.ClassId = "";

        Assert.Equal("Class must be chosen.", _scholar.Problem);
    }

    [Fact]
    public void Problem_NewMemberWithUppercaseId_SaysTheIdRule()
    {
        var added = PartyMemberViewModel.New();
        added.ClassId = "scholar";

        added.Id = "Bram";

        Assert.Equal("The id may only use lowercase letters, digits and _ (like tree_4).", added.Problem);
    }

    // The repository's ids predate the rule; a saved member's id is fixed, so only a new one is checked.
    [Fact]
    public void Problem_SavedMember_IgnoresIdRule() =>
        Assert.Null(PartyMemberViewModel.From(Scholar with { Id = "Ilsa-1" }).Problem);

    [Fact]
    public void Changed_SummaryTyped_IsRaised()
    {
        var raised = 0;
        _scholar.Changed += (_, _) => raised++;

        _scholar.Summary = "Reads in the dark.";

        Assert.Equal(1, raised);
    }

    // The id raises the label's and the problem's notices too; only the typed field counts.
    [Fact]
    public void Changed_IdTyped_IsRaisedOnce()
    {
        var added = PartyMemberViewModel.New();
        var raised = 0;
        added.Changed += (_, _) => raised++;

        added.Id = "bram";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Pick_FolderOfNoMemberPicture_Throws() =>
        Assert.Throws<ArgumentException>(() => _scholar.Pick(ArtFolders.Tilesheets, "Grass.png"));
}
