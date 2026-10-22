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
    public void Load_RepositoryContent_LoadsEveryTile()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.Equal(283, catalog.Tiles.Count);
        Assert.True(catalog.Tiles[1].IsWalkable);
    }

    [Fact]
    public void Load_RepositoryContent_LoadsObjectsWithTileGroupSizes()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.Equal(13, catalog.Objects.Count);
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

        Assert.Equal(["forest", "interior_1", "start"], catalog.Maps.Keys.Order(StringComparer.Ordinal));
        var start = catalog.Maps["start"];
        Assert.Equal((45, 45, 56), (start.Layout.Width, start.Layout.Height, start.Layout.Objects.Count));
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
    public void Load_RepositoryContent_SarxianKnowsTheLegacySkills()
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

        // Developer decision 22: Recovery heals 30 HP on its user, as in legacy; it does not restore MP.
        Assert.Equal(new Effect(EffectKind.RestoreHp, Power: 30, Targeting.Self), recovery);
    }
}
