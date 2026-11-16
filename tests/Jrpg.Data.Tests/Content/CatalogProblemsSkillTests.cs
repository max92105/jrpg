using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The Skills page's problem line (decision 974): the skills' own rules, then the classes and enemies that name them,
// checked with the edited skills, since the editor writes neither file.
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

    private static Dictionary<string, T> Replaced<T>(IReadOnlyDictionary<string, T> records, string id, T record) =>
        new(records, StringComparer.Ordinal) { [id] = record };
}
