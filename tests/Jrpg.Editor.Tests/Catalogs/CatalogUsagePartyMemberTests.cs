using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The maps whose events recruit a party member or run only while it leads (decision 954), saved or open as they are
// now, on the test world's start map.
public sealed class CatalogUsagePartyMemberTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly MapDefinition _start = _catalog.Maps["start"];

    [Fact]
    public void UsersOfPartyMember_RecruitStep_NamesTheMap()
    {
        var recruiting = new MapDefinition(WithEvent(_start.Layout, Recruiting("ilsa")), _start.Settings);
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["start"] = recruiting } };

        Assert.Equal(["map start"], CatalogUsage.UsersOfPartyMember(saved, "ilsa", []));
    }

    [Fact]
    public void UsersOfPartyMember_LeaderGate_NamesTheMap()
    {
        var gated = new MapDefinition(WithEvent(_start.Layout, GatedOn("ilsa")), _start.Settings);
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["start"] = gated } };

        Assert.Equal(["map start"], CatalogUsage.UsersOfPartyMember(saved, "ilsa", []));
    }

    // Both ways of naming a member, each naming another.
    [Fact]
    public void UsersOfPartyMember_OtherMember_IsEmpty()
    {
        var layout = WithEvent(WithEvent(_start.Layout, Recruiting("bram")), GatedOn("bram"));

        Assert.Empty(CatalogUsage.UsersOfPartyMember(_catalog, "ilsa", [layout]));
    }

    // The saved start map doesn't recruit her; the open one, unsaved, does.
    [Fact]
    public void UsersOfPartyMember_OpenMapUnsavedStep_NamesIt()
    {
        var open = WithEvent(_start.Layout, Recruiting("ilsa"));

        Assert.Equal(["map start"], CatalogUsage.UsersOfPartyMember(_catalog, "ilsa", [open]));
    }

    private static MapEvent Recruiting(string memberId) =>
        new() { Id = "recruits", Activation = EventActivation.Touch, Steps = [new RecruitStep(memberId)] };

    private static MapEvent GatedOn(string leaderId) =>
        new() { Id = "leader_intro", Activation = EventActivation.Touch, OnlyWhenLeaderId = leaderId, Steps = [] };

    private static MapLayout WithEvent(MapLayout layout, MapEvent mapEvent) =>
        layout with { CellEvents = [.. layout.CellEvents, new CellEvent(new GridCell(0, layout.CellEvents.Count), mapEvent)] };
}
