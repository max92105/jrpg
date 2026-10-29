using Jrpg.Core.Crafting;
using Jrpg.Game.Crafting;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Crafting;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeRefusalNamesTests
{
    // A refusal mapped to another's line would show the wrong reason and fail no scene test.
    [Fact]
    public void IdOf_EveryRefusal_HasItsOwnLine()
    {
        var ids = Enum.GetValues<ForgeRefusal>().Select(ForgeRefusalNames.IdOf).ToList();

        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ids, id => Assert.NotEmpty(RepositoryScreenText.Text.Messages.Text(id)));
    }
}
