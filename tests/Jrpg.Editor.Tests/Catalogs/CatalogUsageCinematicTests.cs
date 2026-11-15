using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The maps whose events play a cinematic (ADR 0046), saved or open as they are now, on the test world's start map.
public sealed class CatalogUsageCinematicTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly MapDefinition _start = _catalog.Maps["start"];

    [Fact]
    public void UsersOfCinematic_EventPlaysIt_NamesTheMap()
    {
        var playing = new MapDefinition(Playing(_start.Layout, "intro"), _start.Settings);
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["start"] = playing } };

        Assert.Equal(["map start"], CatalogUsage.UsersOfCinematic(saved, "intro", []));
    }

    // The saved start map doesn't play it; the open one, unsaved, does.
    [Fact]
    public void UsersOfCinematic_OpenMapUnsavedStep_NamesIt()
    {
        Assert.Equal(["map start"], CatalogUsage.UsersOfCinematic(_catalog, "intro", [Playing(_start.Layout, "intro")]));
    }

    [Fact]
    public void UsersOfCinematic_NoEventPlaysIt_IsEmpty()
    {
        Assert.Empty(CatalogUsage.UsersOfCinematic(_catalog, "intro", [Playing(_start.Layout, "outro")]));
    }

    private static MapLayout Playing(MapLayout layout, string cinematicId)
    {
        var mapEvent = new MapEvent
        {
            Id = "plays",
            Activation = EventActivation.Touch,
            Steps = [new CinematicStep(cinematicId)],
        };
        return layout with { CellEvents = [.. layout.CellEvents, new CellEvent(new GridCell(0, 0), mapEvent)] };
    }
}
