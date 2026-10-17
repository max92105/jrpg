using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentCatalogLoaderTests
{
    [Fact]
    public void Load_RepositoryContent_Succeeds()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        // Assets/Maps/start.json already refers to "potion", and ids never change once used.
        Assert.Contains("potion", catalog.Items.Keys);
    }

    [Fact]
    public void Load_RepositoryContent_EveryPartyMemberCanBeRecruitedAndGainExperience()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var stepsPerMember = catalog.PartyMembers.Values
            .Select(definition => PartyMember.Recruit(definition, catalog.Skills)
                .GainExperience(1, catalog.Rules.Experience))
            .ToList();

        Assert.Equal(7, stepsPerMember.Count);
        Assert.All(stepsPerMember, steps => Assert.Single(steps));
    }

    [Fact]
    public void Load_RepositoryContent_SarxianKnowsTheLegacySkills()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var skillIds = catalog.PartyMembers["sarxian"].SkillIds;

        Assert.Equal(["fire_blast", "fireball", "heal", "recovery", "heal_all"], skillIds);
    }

    [Fact]
    public void Load_RepositoryContent_RecoveryHealsItsUser()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var recovery = catalog.Skills["recovery"].Effect;

        // Developer decision 22: Recovery heals 30 HP on its user, as in legacy; it does not restore MP.
        Assert.Equal(new Effect(EffectKind.RestoreHp, Power: 30, Targeting.Self), recovery);
    }
}
