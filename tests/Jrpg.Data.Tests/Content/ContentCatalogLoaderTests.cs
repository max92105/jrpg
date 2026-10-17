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
            .Select(definition => PartyMember.Recruit(definition, []).GainExperience(1, catalog.Rules.Experience))
            .ToList();

        Assert.Equal(7, stepsPerMember.Count);
        Assert.All(stepsPerMember, steps => Assert.Single(steps));
    }
}
