using Jrpg.Core.Effects;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentCatalogLoaderTests
{
    [Fact]
    public void Load_StartMap_HasARestEvent()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var steps = catalog.Maps["start"].Layout.Objects.SelectMany(placed => placed.Events).SelectMany(mapEvent => mapEvent.Steps);
        Assert.Contains(steps, step => step is RestStep);
    }

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
        string[] classIds = ["fighter", "mage", "cleric", "rogue", "barbarian", "paladin", "monk", "warlock", "druid"];

        Assert.All(classIds, id => Assert.NotEmpty(catalog.Classes[id].Description));
        Assert.All(classIds, id => Assert.True(catalog.Classes[id].Base.MaxHp > 0, $"{id} starts with no HP."));
    }

    // Decision 207: the class was renamed, with no alias for its old id.
    [Fact]
    public void Load_RepositoryContent_RagnirIsAFighter()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var fighter = catalog.ClassOf(catalog.PartyMembers["ragnir"]);

        Assert.Equal(("fighter", "Fighter"), (fighter.Id, fighter.Name));
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
    public void Load_RepositoryContent_LeaderChoicesAreTheNineInOrder()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var memberIds = catalog.NewGame.LeaderChoices.Select(choice => choice.MemberId);

        Assert.Equal(["ragnir", "omer", "zanyr", "lazarus", "jazog", "teelle", "zaruk", "sarxian", "thaali"], memberIds);
    }

    [Fact]
    public void Load_RepositoryContent_EveryPartyMemberCanBeRecruitedAndGainExperience()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var stepsPerMember = catalog.PartyMembers.Values
            .Select(definition => PartyMember.Recruit(definition, catalog.ClassOf(definition), catalog.Skills, catalog.Rules.Stats)
                .GainExperience(1, catalog.Rules.Experience))
            .ToList();

        Assert.Equal(9, stepsPerMember.Count);
        Assert.All(stepsPerMember, steps => Assert.Single(steps));
    }

    // A long fight for watching class resources build up (question 226); no map rolls it.
    [Fact]
    public void Load_RepositoryContent_HasTrainingDummyEncounter()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var encounter = catalog.Encounters["training_dummy"];

        Assert.Equal(["training_dummy"], encounter.EnemyIds);
        Assert.Equal(400, catalog.Enemies["training_dummy"].MaxHp);
        Assert.DoesNotContain(catalog.Maps.Values, map => map.Settings.RandomEncounters?.EncounterIds.Contains("training_dummy") == true);
    }

    [Fact]
    public void Load_RepositoryFighter_HasPrimaries()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var fighter = catalog.ClassOf(catalog.PartyMembers["ragnir"]);

        Assert.True(fighter.Base.Primaries.Power > 0, "The fighter's base Power comes from classes.json.");
    }

    [Fact]
    public void Load_RepositoryChainMail_HasConstitution()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var chainMail = catalog.Items["chain_mail"].Equipment!;

        Assert.True(chainMail.Bonuses.Of(StatKind.Constitution) > 0, "items.json gives chain mail a Constitution bonus.");
    }

    [Fact]
    public void Load_RepositoryContent_FighterHasStamina()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var fighter = catalog.ClassOf(catalog.PartyMembers["ragnir"]);

        var stamina = fighter.Resources.Single(resource => resource.Id == "stamina");
        Assert.Equal((6, 3, 1), (stamina.Max, stamina.StartsAt, stamina.Gains.PerTurn));
        Assert.Equal(new StatusGrant(StatusKind.Stun, 60, 1), catalog.Skills["shield_bash"].Effect.Status);
    }

    [Fact]
    public void Load_RepositoryContent_RogueReadinessScalesWithSpeed()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var rogue = catalog.ClassOf(catalog.PartyMembers["lazarus"]);

        Assert.Equal(new SpeedScaling(10), rogue.Resources.Single(resource => resource.Id == "readiness").MaxFromSpeed);
        Assert.Equal(30, rogue.Base.Speed);
    }

    [Fact]
    public void Load_RepositoryContent_WarlockHasInstability()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var warlock = catalog.ClassOf(catalog.PartyMembers["sarxian"]);

        var instability = warlock.Resources.Single(resource => resource.Id == "instability");
        Assert.Equal((10, 0, -1), (instability.Max, instability.StartsAt, instability.Gains.PerTurn));
        Assert.Equal([new ResourceRequirement("instability", 7)], catalog.Skills["chaos_lance"].Requirements);
    }

    [Fact]
    public void Load_RepositoryContent_MageHasMana()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var mana = catalog.ClassOf(catalog.PartyMembers["omer"]).Resources.Single(resource => resource.Id == "mana");

        Assert.Equal((10, 3, 2), (mana.Max, mana.StartsAt, mana.Gains.PerTurn));
    }

    [Fact]
    public void Load_RepositoryContent_MeteorCostsNineMana()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var meteor = catalog.Skills["meteor"];

        Assert.Equal([new ResourceChange("mana", -9)], meteor.ResourceChanges);
        Assert.Contains("meteor", catalog.Classes["mage"].SkillIds);
    }

    [Fact]
    public void Load_RepositoryContent_ClericListsHeal()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var skillIds = catalog.ClassOf(catalog.PartyMembers["zanyr"]).SkillIds;

        Assert.Contains("heal", skillIds);
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
