using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The repository's boss encounter is reached only by a hand check's local edit (decision 399): this test fails when
// that edit to a map's random table stays in a commit.
public sealed class RepositoryEncounterTests
{
    [Fact]
    public void Load_RepositoryContent_HasGoblinChiefEncounter()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.Equal("goblin_chief", catalog.Encounters["goblin_chief"].EnemyIds[^1]);
        Assert.True(catalog.Enemies["goblin_chief"].NamesSkill("cleave"));
        Assert.True(catalog.Enemies["goblin_chief"].NamesSkill("field_dressing"));
        Assert.DoesNotContain(catalog.Maps.Values, map => map.Settings.RandomEncounters?.EncounterIds.Contains("goblin_chief") == true);
    }

    [Fact]
    public void Load_RepositoryContent_GoblinChiefIsABoss()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.True(catalog.Enemies["goblin_chief"].IsBoss);
        Assert.False(catalog.Enemies["goblin"].IsBoss);
    }

    [Fact]
    public void Load_RepositoryContent_GoblinChiefHasASecondPhase()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var phase = Assert.Single(catalog.Enemies["goblin_chief"].Phases);
        Assert.Contains(phase.Tactics, action => action.SkillId == "crushing_blow");
        Assert.Contains(phase.Tactics, action => action.SkillId == "war_cry");
    }
}
