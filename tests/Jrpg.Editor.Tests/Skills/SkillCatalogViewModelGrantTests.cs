using Jrpg.Editor.Effects;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// The Skills page's problem line over the grant rows (decision 981): a whole number Data refuses, or a status granted
// twice, is Data's message, as a cost row's is (SkillCatalogViewModelTests has the rest of the list).
public sealed class SkillCatalogViewModelGrantTests
{
    [Fact]
    public void Problem_ChanceZero_IsDataMessage()
    {
        var catalog = BoltCatalog();

        catalog.Skills[0].Grants.Rows[0].ChanceText = "0";

        Assert.Equal("skills.json: skill 'bolt': effect.statuses[0].chance must be within 1..100 (was 0).", catalog.Problem);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("100")]
    public void Problem_ChanceAtABound_IsNull(string chance)
    {
        var catalog = BoltCatalog();

        catalog.Skills[0].Grants.Rows[0].ChanceText = chance;

        Assert.Null(catalog.Problem);
    }

    // The draft's one status is poison_1, which bolt already grants: the added row names it again.
    [Fact]
    public void Problem_StatusTwice_IsDataMessage()
    {
        var catalog = BoltCatalog();

        catalog.Skills[0].Grants.AddRowCommand.Execute(null);

        Assert.Equal("skills.json: skill 'bolt': effect.statuses lists poison_1 twice.", catalog.Problem);
    }

    private static SkillCatalogViewModel BoltCatalog()
    {
        var content = SkillFixtures.CatalogWithoutUsers([SkillFixtures.Bolt], [SkillFixtures.Fire1]);
        return new(content, SkillFixtures.PoisonOnlyChoices with { Tiers = TierChoice.AllIn(content) });
    }
}
