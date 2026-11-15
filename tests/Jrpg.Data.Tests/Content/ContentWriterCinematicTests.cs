using System.Text.Json.Nodes;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A cinematic step is written as its type and id and read back whole, and the cinematics are written sorted by id with
// their optional fields left out (ADR 0046), on a copy of the test world whose cinematics.json gains test_cinematic
// beside the frozen intros' cinematics, which its intro map plays.
public sealed class ContentWriterCinematicTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-cinematics", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterCinematicTests()
    {
        var frozen = JsonNode.Parse(File.ReadAllText(_content.PathOf("cinematics.json")))!.AsArray();
        frozen.Add(JsonNode.Parse("""{ "id": "test_cinematic", "stills": [ { "imageFile": "test/1.png", "seconds": 4 } ] }"""));
        File.WriteAllText(_content.PathOf("cinematics.json"), frozen.ToJsonString());
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMaps_CinematicStep_WritesTypeAndId()
    {
        var edited = WithCinematicCell("test_cinematic");

        _writer.WriteMaps(MapContent.With(_catalog, edited), [edited.Id]);

        var step = WrittenCellEvent()["steps"]!.AsArray().Single()!.AsObject();
        Assert.Equal(("cinematic", "test_cinematic"), ((string?)step["type"], (string?)step["cinematicId"]));
    }

    [Fact]
    public void WriteMaps_CinematicStep_LoadsBackEqual()
    {
        var edited = WithCinematicCell("test_cinematic");

        _writer.WriteMaps(MapContent.With(_catalog, edited), [edited.Id]);

        var cells = ContentCatalogLoader.Load(_content.Path).Maps["start"].Layout.CellEvents;
        var loaded = cells.Single(cell => cell.Event.Id == "cinematic_cell").Event;
        Assert.Equal(new CinematicStep("test_cinematic"), Assert.Single(loaded.Steps));
    }

    [Fact]
    public void WriteMaps_UnknownCinematic_ThrowsNamingIt()
    {
        var edited = WithCinematicCell("nope");
        var content = MapContent.With(_catalog, edited);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMaps(content, [edited.Id]));

        Assert.Contains("cinematic has unknown cinematic id 'nope'.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMapCatalogs_NewCinematics_WritesThemSortedWithStillsInOrder()
    {
        var zeta = Cinematic("zeta", Still("zeta/b.png"), Still("zeta/a.png"));
        var alpha = Cinematic("alpha", Still("alpha/b.png"), Still("alpha/a.png"));

        _writer.WriteMapCatalogs(WithCinematics(zeta, alpha));

        var ids = WrittenCinematics().Select(cinematic => (string?)cinematic!["id"]).ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.True(ids.IndexOf("alpha") < ids.IndexOf("zeta"));
        var alphaStills = Written("alpha")["stills"]!.AsArray();
        Assert.Equal(["alpha/b.png", "alpha/a.png"], alphaStills.Select(still => (string?)still!["imageFile"]));
    }

    [Fact]
    public void WriteMapCatalogs_StillWithoutOptionalFields_LeavesThemOut()
    {
        _writer.WriteMapCatalogs(WithCinematics(Cinematic("intro", Still("intro/1.png"))));

        var cinematic = Written("intro");
        var still = cinematic["stills"]!.AsArray().Single()!.AsObject();
        Assert.False(cinematic.ContainsKey("musicFile"));
        Assert.Equal(["imageFile", "seconds"], still.Select(field => field.Key));
    }

    [Fact]
    public void WriteMapCatalogs_StillWithEveryField_WritesThem()
    {
        var still = Still("intro/1.png") with
        {
            Caption = "Long ago.",
            VoiceFile = "Placeholder.wav",
            Zoom = new StillZoom { StartPercent = 100, EndPercent = 120 },
        };

        _writer.WriteMapCatalogs(WithCinematics(Cinematic("intro", still) with { MusicFile = "town/loop.wav" }));

        Assert.Equal(still, ContentCatalogLoader.Load(_content.Path).Cinematics["intro"].Stills.Single());
        Assert.Equal("town/loop.wav", (string?)Written("intro")["musicFile"]);
    }

    [Fact]
    public void WriteMapCatalogs_StillSecondsZero_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf("cinematics.json"));
        var catalog = WithCinematics(Cinematic("intro", Still("intro/1.png") with { Seconds = 0 }));

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(catalog));

        Assert.Equal("cinematics.json: cinematic 'intro': stills[0]: seconds must be above 0 (was 0).", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf("cinematics.json")));
    }

    private static CinematicStill Still(string imageFile) => new() { ImageFile = imageFile, Seconds = 4 };

    private static CinematicDefinition Cinematic(string id, params CinematicStill[] stills) => new() { Id = id, Stills = stills };

    // Beside the catalog's own: the intro map plays the frozen ones, and a save checks every map.
    private ContentCatalog WithCinematics(params CinematicDefinition[] cinematics)
    {
        var all = new Dictionary<string, CinematicDefinition>(_catalog.Cinematics);
        foreach (var cinematic in cinematics)
        {
            all[cinematic.Id] = cinematic;
        }

        return _catalog with { Cinematics = all };
    }

    private JsonObject Written(string id) => WrittenCinematics().Single(cinematic => (string?)cinematic!["id"] == id)!.AsObject();

    private JsonArray WrittenCinematics() => JsonNode.Parse(File.ReadAllText(_content.PathOf("cinematics.json")))!.AsArray();

    private JsonObject WrittenCellEvent()
    {
        var layout = JsonNode.Parse(File.ReadAllText(_content.PathOf(Path.Combine("map-layouts", "start.json"))))!;
        var written = layout["cellEvents"]!.AsArray().Single(cell => (string?)cell!["event"]!["id"] == "cinematic_cell")!;
        return written["event"]!.AsObject();
    }

    private MapDefinition WithCinematicCell(string cinematicId)
    {
        var start = _catalog.Maps["start"];
        var played = new MapEvent
        {
            Id = "cinematic_cell",
            Activation = EventActivation.Touch,
            Steps = [new CinematicStep(cinematicId)],
        };
        var cells = start.Layout.CellEvents.Append(new CellEvent(new GridCell(0, 0), played));
        return new MapDefinition(start.Layout with { CellEvents = [.. cells] }, start.Settings);
    }
}
