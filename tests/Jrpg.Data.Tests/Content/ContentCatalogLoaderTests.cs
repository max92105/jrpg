using Jrpg.Core.Effects;
using Jrpg.Core.Geometry;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentCatalogLoaderTests
{
    [Fact]
    public void Load_RepositoryContent_Succeeds()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        // content/map-layouts/start.json already refers to "potion", and ids never change once used.
        Assert.Contains("potion", catalog.Items.Keys);
    }

    [Fact]
    public void Load_RepositoryContent_HasTheNineClassesWithDescriptionsAndStats()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        string[] classIds = ["warrior", "mage", "cleric", "rogue", "barbarian", "paladin", "monk", "warlock", "druid"];

        Assert.All(classIds, id => Assert.NotEmpty(catalog.Classes[id].Description));
        Assert.All(classIds, id => Assert.True(catalog.Classes[id].Base.MaxHp > 0, $"{id} starts with no HP."));
    }

    [Fact]
    public void Load_RepositoryContent_TreeHasItsTileGroupSizeAndOutline()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.Equal((4, 4), (catalog.Objects["tree"].Width, catalog.Objects["tree"].Height));
        Assert.Equal(9, catalog.Objects["tree"].CollisionPolygon.Count);
        Assert.Equal(new Point2D(0.4, 0.94375), catalog.Objects["tree"].CollisionPolygon[1]);
    }

    [Fact]
    public void Load_RepositoryContent_VillagerUsesThePlaceholderSprite()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var villager = catalog.Objects["villager"];
        Assert.Equal("npc_placeholder.png", villager.FieldSpriteFile);
        Assert.Empty(villager.Tiles);
        Assert.Contains(catalog.Maps["start"].Layout.Objects, placed => placed.ObjectId == "villager");
    }

    [Fact]
    public void Load_RepositoryContent_ComposesTheThreeLiveMaps()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.All(["forest", "interior_1", "start"], id => Assert.Contains(id, catalog.Maps.Keys));
        var start = catalog.Maps["start"];
        Assert.Equal((45, 45), (start.Layout.Width, start.Layout.Height));
        Assert.Equal("Interior1", catalog.Maps["interior_1"].Settings.DisplayName);
    }

    [Fact]
    public void Load_RepositoryContent_OnlyTheForestHasRandomBattles()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var forestBattles = catalog.Maps["forest"].Settings.RandomEncounters;

        Assert.Equal(["goblin_trio"], forestBattles?.EncounterIds);
        Assert.Null(catalog.Maps["start"].Settings.RandomEncounters);
        Assert.Null(catalog.Maps["interior_1"].Settings.RandomEncounters);
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
    public void Load_RepositoryContent_SarxianKnowsTheStartingSkills()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var skillIds = catalog.PartyMembers["sarxian"].SkillIds;

        Assert.Equal(["fire_blast", "fireball", "heal", "recovery", "heal_all"], skillIds);
    }

    [Fact]
    public void Load_RepositoryContent_EverySkillHasDescription()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.All(catalog.Skills.Values, skill => Assert.False(string.IsNullOrWhiteSpace(skill.Description), skill.Id));
    }

    [Fact]
    public void Load_RepositoryContent_RecoveryHealsItsUser()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var recovery = catalog.Skills["recovery"].Effect;

        // Developer decision 22: Recovery heals 30 HP on its user; it does not restore MP.
        Assert.Equal(new Effect(EffectKind.RestoreHp, Power: 30, Targeting.Self), recovery);
    }
}
