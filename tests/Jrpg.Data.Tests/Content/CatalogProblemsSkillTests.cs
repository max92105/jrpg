using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The Skills page's problem line (decision 974): the skills' own rules, then the classes and enemies that name them,
// checked with the edited skills, since the editor writes neither file. The Effects and Statuses pages' lines (decision
// 975): the records' own rules, then the skills over the catalog rebound to the edited records.
public sealed class CatalogProblemsSkillTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly SkillDefinition _spark = new()
    {
        Id = "spark",
        Name = "Spark",
        Description = "",
        IconFile = "Attack.png",
        SoundFile = "PlaceholderSkillSlash.wav",
        Effect = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Fire)
        {
            Tier = _catalog.TieredEffects["fire_1"],
        },
    };

    private static readonly TieredEffect _spark1 = new("spark_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };

    [Fact]
    public void OfSkills_TestWorldSkills_IsNull() => Assert.Null(CatalogProblems.OfSkills([.. _catalog.Skills.Values], _catalog));

    [Fact]
    public void OfSkills_EmptyName_NamesTheFileAndSkill()
    {
        var problem = CatalogProblems.OfSkills([.. _catalog.Skills.Values, _spark with { Name = " " }], _catalog);

        Assert.Equal("skills.json: skill 'spark': name must not be empty.", problem);
    }

    [Fact]
    public void OfSkills_ClassSkillWithAForeignResource_NamesClassesFile()
    {
        var mage = _catalog.Classes["mage"] with { SkillIds = ["spark"] };
        var content = _catalog with { Classes = Replaced(_catalog.Classes, "mage", mage) };
        var spark = _spark with { ResourceChanges = [new ResourceChange("stamina", -1)] };

        var problem = CatalogProblems.OfSkills([.. _catalog.Skills.Values, spark], content);

        Assert.Equal(
            "classes.json: class 'mage': skillIds lists 'spark', which uses 'stamina', a resource mage doesn't have.", problem);
    }

    // The enemy names a skill only the edited list holds: checked against the content's skills, it would be unknown.
    [Fact]
    public void OfSkills_EnemySkillWithACost_NamesEnemiesFile()
    {
        var goblin = _catalog.Enemies["goblin"] with { Tactics = [new EnemyAction { SkillId = "spark", Weight = 1 }] };
        var content = _catalog with { Enemies = Replaced(_catalog.Enemies, "goblin", goblin) };
        var spark = _spark with { ResourceChanges = [new ResourceChange("mana", -1)] };

        var problem = CatalogProblems.OfSkills([.. _catalog.Skills.Values, spark], content);

        Assert.Equal(
            "enemies.json: enemy 'goblin': tactics[0].skillId 'spark' uses a class resource, and enemies have none.", problem);
    }

    [Fact]
    public void OfTieredEffects_TestWorldEffects_IsNull() =>
        Assert.Null(CatalogProblems.OfTieredEffects([.. _catalog.TieredEffects.Values], _catalog));

    [Fact]
    public void OfTieredEffects_PerPowerZero_NamesTheEffectsFile()
    {
        var effects = _catalog.TieredEffects.Values.Append(_spark1 with { PerPower = 0 }).ToList();

        var problem = CatalogProblems.OfTieredEffects(effects, _catalog);

        Assert.Equal("effects.json: effect 'spark_1': perPower must be within 1..1000 (was 0).", problem);
    }

    // Only Spark names spark_1, so the skill named is the one the rebinding reached.
    [Fact]
    public void OfTieredEffects_KindChangedUnderADamageSkill_NamesTheSkillsFileAndSkill()
    {
        var spark = _spark with { Effect = _spark.Effect with { Tier = _spark1 } };
        var content = _catalog with
        {
            Skills = Replaced(_catalog.Skills, spark.Id, spark),
            TieredEffects = Replaced(_catalog.TieredEffects, _spark1.Id, _spark1),
        };
        var heal = _spark1 with { Kind = EffectKind.RestoreHp, DamageType = null };

        var problem = CatalogProblems.OfTieredEffects([.. _catalog.TieredEffects.Values, heal], content);

        Assert.Equal("skills.json: skill 'spark': effect.kind is damage but spark_1 is restoreHp.", problem);
    }

    [Fact]
    public void OfStatuses_TestWorldStatuses_IsNull() =>
        Assert.Null(CatalogProblems.OfStatuses([.. _catalog.Statuses.Values], _catalog));

    [Fact]
    public void OfStatuses_DamageTickZero_NamesTheFileAndStatus()
    {
        var statuses = StatusesWithPoison1Ticking(0);

        var problem = CatalogProblems.OfStatuses(statuses, _catalog);

        Assert.Equal("statuses.json: status 'poison_1': parts[0]: damageEachTurn's amount must be at least 1 (was 0).", problem);
    }

    // The review's C7: no Data rule reads a granted status's parts yet, so the skill breaks a rule of its own (chance
    // 0); the message proves the statuses' check runs the skills, over the catalog rebound to the edited poison_1.
    [Fact]
    public void OfStatuses_SkillGrantingTheEditedStatusBreaksARule_IsTheSkillsMessage()
    {
        var spark = _spark with
        {
            Effect = _spark.Effect with { Statuses = [new StatusGrant(_catalog.Statuses["poison_1"], 2) { Chance = 0 }] },
        };
        var content = _catalog with { Skills = Replaced(_catalog.Skills, spark.Id, spark) };

        var problem = CatalogProblems.OfStatuses(StatusesWithPoison1Ticking(12), content);

        Assert.Equal("skills.json: skill 'spark': effect.statuses[0].chance must be within 1..100 (was 0).", problem);
    }

    private static List<StatusDefinition> StatusesWithPoison1Ticking(int amount)
    {
        var poison1 = _catalog.Statuses["poison_1"];
        var retuned = poison1 with { Parts = [poison1.Parts[0] with { Amount = amount }] };
        return [.. _catalog.Statuses.Values.Select(status => status.Id == retuned.Id ? retuned : status)];
    }

    private static Dictionary<string, T> Replaced<T>(IReadOnlyDictionary<string, T> records, string id, T record) =>
        new(records, StringComparer.Ordinal) { [id] = record };
}
