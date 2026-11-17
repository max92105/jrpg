using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps.Editing;
using static Jrpg.Editor.Tests.Party.PartyMemberFixtures;

namespace Jrpg.Editor.Tests.Shell;

// An undo that would take away a party member a map's event recruits is refused naming the map (decision 954, as
// decision 581 refuses an opened shop); the leader gate is the same lookup, tested in CatalogUsagePartyMemberTests.
public sealed class CombinedHistoryPartyMemberTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    // The open map's unsaved step, placed before the member, so the member's step is the next undo.
    [Fact]
    public void Undo_AddedMemberRecruitedOnOpenMap_IsRefusedNamingTheMap()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var map = viewModel.CurrentMap!;
        var recruitsIlsa = new MapEvent
        {
            Id = "recruit_check",
            Activation = EventActivation.Touch,
            Steps = [new RecruitStep(Scholar.Id)],
        };
        map.Do(SetCellEventEdit.For(map.Map.Layout.CellEvents, new GridCell(0, 0), recruitsIlsa));
        viewModel.Content!.SetPartyMembers([.. viewModel.Catalog!.PartyMembers.Values, Scholar]);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: party member ilsa is used on map {map.Map.Id}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.PartyMembers.ContainsKey(Scholar.Id));
    }

    [Fact]
    public void Undo_AddedMemberUnused_RemovesIt()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.Content!.SetPartyMembers([.. viewModel.Catalog!.PartyMembers.Values, Scholar]);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
        Assert.False(viewModel.Catalog.PartyMembers.ContainsKey(Scholar.Id));
    }
}
