using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The repository's skills deal and heal in tiers (ADR 0056), and the enemy using them has a Power for them: rules over
// whatever the content holds, never its numbers, which are the developer's to tune.
public sealed class ContentCatalogLoaderSkillTierTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Load_RepositorySkills_EveryDamageOrHealSkillNamesATieredEffect()
    {
        var damagingOrHealing =
            _catalog.Skills.Values.Where(skill => skill.Effect.Kind is EffectKind.Damage or EffectKind.RestoreHp);

        Assert.All(damagingOrHealing, skill => Assert.NotNull(skill.Effect.Tier));
    }

    [Fact]
    public void Load_RepositoryGoblinChief_HasPowerForItsTieredSkills()
    {
        Assert.True(_catalog.Enemies["goblin_chief"].Power >= 1);
    }
}
