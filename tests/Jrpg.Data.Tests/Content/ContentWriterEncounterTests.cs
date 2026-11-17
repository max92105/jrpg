using System.Text.Json.Nodes;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// encounters.json is written with the other catalogs (decision 908): checked against the enemies as the loader checks
// it, sorted by id, an encounter's own background and track left out while it has none, on a copy of the test world.
public sealed class ContentWriterEncounterTests : IDisposable
{
    private const string FileName = "encounters.json";

    private readonly CopiedContentRoot _content = new("writer-encounters", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterEncounterTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    // Added after the test world's, the later id first: unsorted, the file would end on "zz_ambush", "aa_ambush".
    [Fact]
    public void WriteMapCatalogsAndMaps_EncountersAddedOutOfOrder_WritesThemSortedById()
    {
        _writer.WriteMapCatalogsAndMaps(With(Ambush("zz_ambush"), Ambush("aa_ambush")), []);

        var ids = Written().Select(encounter => (string?)encounter!["id"]).ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.Contains("aa_ambush", ids);
        Assert.Contains("zz_ambush", ids);
    }

    [Fact]
    public void WriteMapCatalogsAndMaps_EncounterWithoutBackgroundOrMusic_LeavesBothFieldsOut()
    {
        _writer.WriteMapCatalogsAndMaps(With(Ambush("ambush")), []);

        Assert.Equal(["id", "enemyIds"], WrittenEncounter("ambush").Select(field => field.Key));
    }

    [Fact]
    public void WriteMapCatalogsAndMaps_EncounterWithBoth_WritesBoth()
    {
        var arena = Ambush("ambush") with { BackgroundFile = "Arena.png", MusicFile = "boss/loop.wav" };

        _writer.WriteMapCatalogsAndMaps(With(arena), []);

        var written = WrittenEncounter("ambush");
        Assert.Equal("Arena.png", (string?)written["backgroundFile"]);
        Assert.Equal("boss/loop.wav", (string?)written["musicFile"]);
    }

    [Fact]
    public void WriteMapCatalogsAndMaps_UnknownEnemy_IsRefusedAndNothingWritten()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogsAndMaps(With(Ambush("ambush") with { EnemyIds = ["goblin", "dragon"] }), []));

        Assert.Equal("encounters.json: encounter 'ambush': enemyIds has unknown enemy id 'dragon'.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    // Slot order is screen order, so the file keeps it: neither sorted (goblin, goblin, stone_beetle) nor merged.
    [Fact]
    public void WriteMapCatalogsAndMaps_SameEnemyTwice_WritesBothSlotsInOrder()
    {
        var ambush = Ambush("ambush") with { EnemyIds = ["goblin", "stone_beetle", "goblin"] };

        _writer.WriteMapCatalogsAndMaps(With(ambush), []);

        var enemyIds = WrittenEncounter("ambush")["enemyIds"]!.AsArray().Select(id => (string?)id);
        Assert.Equal(["goblin", "stone_beetle", "goblin"], enemyIds);
    }

    private static EncounterDefinition Ambush(string id) => new() { Id = id, EnemyIds = ["goblin"] };

    // The test world's encounters stay: its maps' random battles name them.
    private ContentCatalog With(params EncounterDefinition[] encounters)
    {
        var all = new Dictionary<string, EncounterDefinition>(_catalog.Encounters);
        foreach (var encounter in encounters)
        {
            all.Add(encounter.Id, encounter);
        }

        return _catalog with { Encounters = all };
    }

    private JsonArray Written() => JsonNode.Parse(File.ReadAllText(_content.PathOf(FileName)))!.AsArray();

    private JsonObject WrittenEncounter(string id) =>
        Written().Single(encounter => (string?)encounter!["id"] == id)!.AsObject();
}
