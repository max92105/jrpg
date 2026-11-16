using System.Text.Json.Nodes;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// skills.json, effects.json and statuses.json are saved with the other catalogs (decision 972): a skill's tier and
// grants written as ids, every default-valued field left out (decision 973), the skills checked with the classes and
// enemies that name them (decision 974).
public sealed class ContentWriterSkillTests : IDisposable
{
    private static readonly string[] _defaultValuedFields =
    [
        "\"effectFile\": null",
        "\"resourceChanges\": []",
        "\"requirements\": []",
        "\"isQuick\": false",
        "\"range\": 0",
        "\"hits\": 1",
        "\"tier\": null",
        "\"statuses\": []",
        "\"damageType\": null",
    ];

    private readonly CopiedContentRoot _content = new("writer-skills", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterSkillTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_Skills_LoadTheSameAfterRelay()
    {
        _writer.WriteMapCatalogs(_catalog);

        var reloaded = ContentCatalogLoader.Load(_content.Path).Skills;
        Assert.Equal(_catalog.Skills.Keys.Order(StringComparer.Ordinal), reloaded.Keys.Order(StringComparer.Ordinal));
        Assert.All(_catalog.Skills.Values, skill => AssertSameSkill(skill, reloaded[skill.Id]));
    }

    [Fact]
    public void WriteMapCatalogs_RelaidSkills_HoldNoDefaultValuedField()
    {
        _writer.WriteMapCatalogs(_catalog);

        var text = File.ReadAllText(_content.PathOf("skills.json"));
        Assert.All(_defaultValuedFields, field => Assert.DoesNotContain(field, text, StringComparison.Ordinal));
    }

    [Fact]
    public void WriteMapCatalogs_TieredSkillWithAGrant_WritesTheIds()
    {
        var effect = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Fire)
        {
            Tier = _catalog.TieredEffects["fire_1"],
            Statuses = [new StatusGrant(_catalog.Statuses["poison_1"], 2)],
        };
        var skill = _catalog.Skills["fire_blast"] with { Id = "venom_flame", Effect = effect };

        _writer.WriteMapCatalogs(WithSkills(skill));

        var written = WrittenSkills()["venom_flame"]["effect"]!;
        Assert.Equal("fire_1", (string?)written["tier"]);
        Assert.Equal("poison_1", (string?)written["statuses"]![0]!["status"]);
    }

    [Fact]
    public void WriteMapCatalogs_SkillItsClassCantPay_WritesNothing()
    {
        var before = SkillFileBytes();
        var manaStrike = _catalog.Skills["power_strike"] with { ResourceChanges = [new ResourceChange("mana", -2)] };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithSkills(manaStrike)));

        Assert.StartsWith("classes.json", exception.Message, StringComparison.Ordinal);
        Assert.Contains("'mana'", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, SkillFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_EnemySkillGivenACost_WritesNothing()
    {
        var before = SkillFileBytes();
        var costlyCleave = _catalog.Skills["cleave"] with { ResourceChanges = [new ResourceChange("stamina", -1)] };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithSkills(costlyCleave)));

        Assert.StartsWith("enemies.json", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, SkillFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_InvalidSkill_WritesNothing()
    {
        var before = SkillFileBytes();
        var unnamed = _catalog.Skills["fire_blast"] with { Name = "" };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithSkills(unnamed)));

        Assert.Equal("skills.json: skill 'fire_blast': name must not be empty.", exception.Message);
        Assert.Equal(before, SkillFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_InvalidStatus_WritesNothing()
    {
        var before = SkillFileBytes();
        var statuses = new Dictionary<string, StatusDefinition>(_catalog.Statuses)
        {
            ["poison_1"] = _catalog.Statuses["poison_1"] with { Name = "" },
        };

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(_catalog with { Statuses = statuses }));

        Assert.Equal("statuses.json: status 'poison_1': name must not be empty.", exception.Message);
        Assert.Equal(before, SkillFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_InvalidTieredEffect_WritesNothing()
    {
        var before = SkillFileBytes();
        var effects = new Dictionary<string, TieredEffect>(_catalog.TieredEffects)
        {
            ["fire_1"] = _catalog.TieredEffects["fire_1"] with { PerPower = 0 },
        };

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(_catalog with { TieredEffects = effects }));

        Assert.Equal("effects.json: effect 'fire_1': perPower must be within 1..1000 (was 0).", exception.Message);
        Assert.Equal(before, SkillFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_ChangedStatus_WritesStatusesFile()
    {
        var poison = _catalog.Statuses["poison_1"];
        var stronger = poison with { Parts = [poison.Parts[0] with { Amount = poison.Parts[0].Amount + 1 }] };
        var statuses = new Dictionary<string, StatusDefinition>(_catalog.Statuses) { [poison.Id] = stronger };

        _writer.WriteMapCatalogs(_catalog with { Statuses = statuses });

        Assert.Equal(stronger, ContentCatalogLoader.Load(_content.Path).Statuses["poison_1"]);
    }

    [Fact]
    public void WriteMapCatalogs_ChangedTieredEffect_WritesEffectsFile()
    {
        var fire = _catalog.TieredEffects["fire_1"];
        var stronger = fire with { PerPower = fire.PerPower + 1 };
        var effects = new Dictionary<string, TieredEffect>(_catalog.TieredEffects) { [fire.Id] = stronger };

        _writer.WriteMapCatalogs(_catalog with { TieredEffects = effects });

        Assert.Equal(stronger, ContentCatalogLoader.Load(_content.Path).TieredEffects["fire_1"]);
    }

    private ContentCatalog WithSkills(params SkillDefinition[] skills)
    {
        var all = new Dictionary<string, SkillDefinition>(_catalog.Skills);
        foreach (var skill in skills)
        {
            all[skill.Id] = skill;
        }

        return _catalog with { Skills = all };
    }

    // The three files this writer takes over, one after the other: a refused save leaves them all as they were.
    private byte[] SkillFileBytes() =>
        [.. File.ReadAllBytes(_content.PathOf("skills.json")),
         .. File.ReadAllBytes(_content.PathOf("effects.json")),
         .. File.ReadAllBytes(_content.PathOf("statuses.json"))];

    private Dictionary<string, JsonNode> WrittenSkills() =>
        JsonNode.Parse(File.ReadAllText(_content.PathOf("skills.json")))!
            .AsArray()
            .ToDictionary(skill => (string)skill!["id"]!, skill => skill!);

    // Records compare their lists by reference, so the costs and minimums are compared by content.
    private static void AssertSameSkill(SkillDefinition expected, SkillDefinition actual)
    {
        Assert.Equal(expected with { ResourceChanges = actual.ResourceChanges, Requirements = actual.Requirements }, actual);
        Assert.Equal(expected.ResourceChanges, actual.ResourceChanges);
        Assert.Equal(expected.Requirements, actual.Requirements);
    }
}
