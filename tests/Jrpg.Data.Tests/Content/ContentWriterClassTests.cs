using System.Text.Json.Nodes;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// classes.json is written with the other catalogs (decision 944), on a copy of the test world: checked against the
// skills as the loader checks it, sorted by id, a resource's absent rules, false lasting flag, zero amounts and zero
// gains left out of the file.
public sealed class ContentWriterClassTests : IDisposable
{
    private const string FileName = "classes.json";

    private static readonly string[] _defaultValuedFields =
    [
        "\"isLasting\": false",
        "\"maxFromSpeed\": null",
        "\"backfire\": null",
        "\"strain\": null",
        "\"dominance\": null",
        "\"willScaling\": null",
        "\"startsAt\": 0",
        "\"perTurn\": 0",
    ];

    private static readonly ResourceDefinition _focus =
        new("focus", "Focus", Max: 5) { Color = "#5B8CFF", Gains = new() { PerHitLanded = 1 } };

    private readonly CopiedContentRoot _content = new("writer-classes", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterClassTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    // Records compare their lists by reference, so the skill ids and resources are compared by content.
    [Fact]
    public void WriteMapCatalogs_Classes_LoadTheSameAfterRelay()
    {
        var written = With(Ranger("ranger") with { Resources = [_focus, Lasting()] });

        _writer.WriteMapCatalogs(written);

        var reloaded = ContentCatalogLoader.Load(_content.Path).Classes;
        Assert.Equal(written.Classes.Keys.Order(StringComparer.Ordinal), reloaded.Keys.Order(StringComparer.Ordinal));
        Assert.All(written.Classes.Values, classDefinition => AssertSameClass(classDefinition, reloaded[classDefinition.Id]));
    }

    [Fact]
    public void WriteMapCatalogs_RelaidClasses_HoldNoDefaultValuedField()
    {
        _writer.WriteMapCatalogs(With(Ranger("ranger")));

        var text = File.ReadAllText(_content.PathOf(FileName));
        Assert.All(_defaultValuedFields, field => Assert.DoesNotContain(field, text, StringComparison.Ordinal));
    }

    // Added after the test world's, the later id first: unsorted, the file would end on "zz_ranger", "aa_ranger".
    [Fact]
    public void WriteMapCatalogs_ClassesAddedOutOfOrder_WritesThemSortedById()
    {
        _writer.WriteMapCatalogs(With(Ranger("zz_ranger"), Ranger("aa_ranger")));

        var ids = Written().Select(classDefinition => (string?)classDefinition!["id"]).ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.Contains("aa_ranger", ids);
        Assert.Contains("zz_ranger", ids);
    }

    [Fact]
    public void WriteMapCatalogs_SpeedResource_LeavesMaxAndStartOut()
    {
        var readiness = new ResourceDefinition("readiness", "Readiness") { Color = "#9AD1C9", MaxFromSpeed = new(10) };

        _writer.WriteMapCatalogs(With(Ranger("ranger") with { Resources = [readiness] }));

        Assert.Equal(["id", "name", "maxFromSpeed", "color"], WrittenResource("ranger").Select(field => field.Key));
    }

    // Decision 944's one addition to the list: a resource only its skills change writes no "gains": {}.
    [Fact]
    public void WriteMapCatalogs_ResourceGainingNothing_LeavesGainsOut()
    {
        _writer.WriteMapCatalogs(With(Ranger("ranger") with { Resources = [_focus with { Gains = new() }] }));

        Assert.Equal(["id", "name", "max", "color"], WrittenResource("ranger").Select(field => field.Key));
    }

    [Fact]
    public void WriteMapCatalogs_LastingResourceWithStrain_WritesThem()
    {
        _writer.WriteMapCatalogs(With(Ranger("ranger") with { Resources = [_focus, Lasting()] }));

        var resolve = ContentCatalogLoader.Load(_content.Path).Classes["ranger"].Resources[1];
        Assert.True(resolve.IsLasting);
        Assert.Equal(new StrainRule("focus", 2), resolve.Strain);
    }

    [Fact]
    public void WriteMapCatalogs_ClassNamingUnknownSkill_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(With(Ranger("ranger") with { SkillIds = ["volley"] })));

        Assert.Equal("classes.json: class 'ranger': skillIds has unknown skill id 'volley'.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    [Fact]
    public void WriteMapCatalogs_SkillUsingForeignResource_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));
        var aimedShot = AimedShot();
        var ranger = Ranger("ranger") with { SkillIds = [aimedShot.Id], Resources = [] };

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(With(ranger) with { Skills = SkillsWith(aimedShot) }));

        Assert.Equal(
            "classes.json: class 'ranger': skillIds lists 'aimed_shot', which uses 'focus', a resource ranger doesn't have.",
            exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    // Two refused classes, the later id added first: the one named is the first in the file, as the loader names it.
    [Fact]
    public void WriteMapCatalogs_TwoClassesRefused_NamesTheFirstById()
    {
        var content = With(Ranger("zz_ranger") with { Name = "" }, Ranger("aa_ranger") with { Name = "" });

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(content));

        Assert.Equal("classes.json: class 'aa_ranger': name must not be empty.", exception.Message);
    }

    private static ClassDefinition Ranger(string id) =>
        new(id, "Ranger", "A hunter who waits for the shot.", new ClassStats(160, 14), new ClassStats(15, 1))
        {
            Resources = [_focus],
        };

    // Only a lasting resource may lower another's start (ADR 0024).
    private static ResourceDefinition Lasting() =>
        new("resolve", "Resolve", Max: 3, StartsAt: 3)
        {
            Color = "#B9C8FF",
            IsLasting = true,
            Gains = new() { PerBattleFought = -1 },
            Strain = new StrainRule("focus", 2),
        };

    private static void AssertSameClass(ClassDefinition expected, ClassDefinition actual)
    {
        Assert.Equal(expected.SkillIds, actual.SkillIds);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected with { SkillIds = actual.SkillIds, Resources = actual.Resources }, actual);
    }

    // Its only resource the focus, whatever power_strike costs today.
    private SkillDefinition AimedShot() =>
        _catalog.Skills["power_strike"] with
        {
            Id = "aimed_shot",
            ResourceChanges = [new ResourceChange("focus", -1)],
            Requirements = [],
        };

    private Dictionary<string, SkillDefinition> SkillsWith(SkillDefinition skill) =>
        new(_catalog.Skills) { [skill.Id] = skill };

    // The test world's classes stay: its members name them.
    private ContentCatalog With(params ClassDefinition[] classes)
    {
        var all = new Dictionary<string, ClassDefinition>(_catalog.Classes);
        foreach (var classDefinition in classes)
        {
            all.Add(classDefinition.Id, classDefinition);
        }

        return _catalog with { Classes = all };
    }

    private JsonArray Written() => JsonNode.Parse(File.ReadAllText(_content.PathOf(FileName)))!.AsArray();

    private JsonObject WrittenResource(string classId) =>
        Written().Single(classDefinition => (string?)classDefinition!["id"] == classId)!["resources"]![0]!.AsObject();
}
