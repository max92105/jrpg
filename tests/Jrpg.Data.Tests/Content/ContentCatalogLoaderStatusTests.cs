using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// statuses.json loads with the catalog (ADR 0055): the repository's records by id and shape, never their numbers, and a
// copy of the content broken here.
public sealed class ContentCatalogLoaderStatusTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("statuses", "content");

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Load_RepositoryStatuses_StunSkipsATurn()
    {
        var stun = ContentCatalogLoader.Load(_content.Path).Statuses["stun"];

        Assert.Contains(stun.Parts, part => part.Kind == StatusPartKind.SkipTurn);
    }

    [Fact]
    public void Load_RepositoryStatuses_GuardsShareAFamily()
    {
        var statuses = ContentCatalogLoader.Load(_content.Path).Statuses;

        Assert.Equal(statuses["guard_1"].FamilyKey, statuses["guard_2"].FamilyKey);
    }

    // Decision 805: Fire Blast's Burning meets fire resistance.
    [Fact]
    public void Load_RepositoryStatuses_BurningTicksAsFire()
    {
        var burning = ContentCatalogLoader.Load(_content.Path).Statuses["burning_1"];

        Assert.Contains(burning.Parts, part => (part.Kind, part.DamageType) == (StatusPartKind.DamageEachTurn, DamageType.Fire));
    }

    // Decision 805: Burning acts as its holder's turn starts.
    [Fact]
    public void Load_RepositoryStatuses_BurningActsAtTheStartOfTurn()
    {
        var burning = ContentCatalogLoader.Load(_content.Path).Statuses["burning_1"];

        Assert.Equal(StatusTiming.StartOfTurn, burning.ActsAt);
    }

    [Fact]
    public void Load_StatusesFileMissing_RefusesNamingIt()
    {
        File.Delete(_content.PathOf("statuses.json"));

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("statuses.json: the file is missing.", exception.Message);
    }

    // A grant names its record by id, resolved as skills.json is read (decision 789).
    [Fact]
    public void Load_GrantNamingAnUnknownStatus_RefusesNamingFileAndId()
    {
        _content.Replace("skills.json", "\"status\": \"burning_1\"", "\"status\": \"burning_9\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("skills.json", exception.Message, StringComparison.Ordinal);
        Assert.Contains("no status 'burning_9'.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_StatusPartWithUnknownKind_RefusesNamingFileAndPath()
    {
        _content.Replace("statuses.json", "\"kind\": \"skipTurn\"", "\"kind\": \"skipTurns\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("statuses.json at $[", exception.Message, StringComparison.Ordinal);
        Assert.Contains(".parts[0].kind", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_StatusWithUnknownTiming_RefusesNamingIt()
    {
        _content.Replace("statuses.json", "\"actsAt\": \"startOfTurn\"", "\"actsAt\": \"beforeTurn\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("statuses.json at $[", exception.Message, StringComparison.Ordinal);
        Assert.Contains(".actsAt", exception.Message, StringComparison.Ordinal);
    }
}
