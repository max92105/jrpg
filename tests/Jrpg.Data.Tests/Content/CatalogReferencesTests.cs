using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A record holds the effects and statuses it names (decision 789), so an edited one reaches them only by rebinding
// (decision 975): Spark and the Fire Flask, built here over the test world, name fire_1 and burning_1.
public sealed class CatalogReferencesTests
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly SkillDefinition _spark = new()
    {
        Id = "spark",
        Name = "Spark",
        Description = "",
        IconFile = "Attack.png",
        Effect = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Fire)
        {
            Tier = _testWorld.TieredEffects["fire_1"],
            Statuses = [new StatusGrant(_testWorld.Statuses["burning_1"], 2)],
        },
    };

    private static readonly ItemDefinition _fireFlask = new()
    {
        Id = "fire_flask",
        Name = "Fire Flask",
        Description = "",
        IconFile = "Potion.png",
        MaxStack = 9,
        IsUsableFromMenu = false,
        Effect = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy)
        {
            Statuses = [new StatusGrant(_testWorld.Statuses["burning_1"], 3)],
        },
    };

    private static readonly ContentCatalog _catalog = _testWorld with
    {
        Skills = Replaced(_testWorld.Skills, _spark.Id, _spark),
        Items = Replaced(_testWorld.Items, _fireFlask.Id, _fireFlask),
    };

    [Fact]
    public void Rebound_TierEdited_SkillHoldsTheNewRecord()
    {
        var fire1 = _catalog.TieredEffects["fire_1"] with { PerPower = 7 };

        var rebound = CatalogReferences.Rebound(
            _catalog with { TieredEffects = Replaced(_catalog.TieredEffects, fire1.Id, fire1) });

        Assert.Same(fire1, rebound.Skills["spark"].Effect.Tier);
    }

    [Fact]
    public void Rebound_StatusEdited_SkillGrantHoldsTheNewRecord()
    {
        var burning = _catalog.Statuses["burning_1"] with { Name = "Scorch" };

        var rebound = CatalogReferences.Rebound(_catalog with { Statuses = Replaced(_catalog.Statuses, burning.Id, burning) });

        Assert.Same(burning, Assert.Single(rebound.Skills["spark"].Effect.Statuses).Status);
    }

    [Fact]
    public void Rebound_StatusEdited_ItemGrantHoldsTheNewRecord()
    {
        var burning = _catalog.Statuses["burning_1"] with { Name = "Scorch" };

        var rebound = CatalogReferences.Rebound(_catalog with { Statuses = Replaced(_catalog.Statuses, burning.Id, burning) });

        Assert.Same(burning, Assert.Single(rebound.Items["fire_flask"].Effect!.Statuses).Status);
    }

    [Fact]
    public void Rebound_NothingToRebind_KeepsTheSameInstances()
    {
        var rebound = CatalogReferences.Rebound(_catalog);

        Assert.All(_catalog.Skills.Values, skill => Assert.Same(skill, rebound.Skills[skill.Id]));
        Assert.All(_catalog.Items.Values, item => Assert.Same(item, rebound.Items[item.Id]));
    }

    [Fact]
    public void Rebound_MissingId_ThrowsNamingTheRecordAndId()
    {
        var spark1 = new TieredEffect("spark_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };
        var spark = _spark with { Effect = _spark.Effect with { Tier = spark1 } };

        var exception = Assert.Throws<InvalidOperationException>(
            () => CatalogReferences.Rebound(_catalog with { Skills = Replaced(_catalog.Skills, spark.Id, spark) }));

        Assert.Equal("skill 'spark' names the effect 'spark_1', which the catalog has no record of.", exception.Message);
    }

    private static Dictionary<string, T> Replaced<T>(IReadOnlyDictionary<string, T> records, string id, T record) =>
        new(records, StringComparer.Ordinal) { [id] = record };
}
