using Jrpg.Editor.Catalogs;
using static Jrpg.Editor.Tests.Party.PartyMemberFixtures;

namespace Jrpg.Editor.Tests.Catalogs;

// The party members are replaced whole as one undo step (decision 954), compared by content so a page applied unchanged
// records nothing; what an undo would take away is listed so it can be refused while a map names it.
public sealed class CatalogDraftPartyMemberTests
{
    private readonly CatalogDraft _draft = new(World);

    [Fact]
    public void SetPartyMembers_ChangedSummary_RecordsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _draft.SetPartyMembers([Scholar with { Summary = "A reader at large." }]);

        Assert.Equal(1, steps);
        Assert.Equal("A reader at large.", _draft.Current.PartyMembers["ilsa"].Summary);
    }

    // A copy equal by value, as the page's ToMembers rebuilds every record.
    [Fact]
    public void SetPartyMembers_SameMembers_RecordsNothing()
    {
        _draft.SetPartyMembers([Scholar with { }]);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetPartyMembers_Undo_RestoresTheMembers()
    {
        _draft.SetPartyMembers([Scholar with { Name = "Elsa" }, Scholar with { Id = "bram" }]);

        _draft.Undo();

        Assert.Equal(Scholar, Assert.Single(_draft.Current.PartyMembers.Values));
    }

    // Added out of order: the ids come back sorted.
    [Fact]
    public void PartyMembersRemovedByUndo_AfterAdding_NamesTheAddedMember()
    {
        _draft.SetPartyMembers([Scholar, Scholar with { Id = "zora" }, Scholar with { Id = "bram" }]);

        Assert.Equal(["bram", "zora"], _draft.PartyMembersRemovedByUndo());
    }
}
