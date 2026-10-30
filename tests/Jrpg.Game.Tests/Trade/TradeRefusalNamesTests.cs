using Jrpg.Core.Trade;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;

namespace Jrpg.Game.Tests.Trade;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class TradeRefusalNamesTests
{
    // A refusal added to Core without its line would show as a missing-text error at the shop.
    [Fact]
    public void IdOf_EveryRefusal_HasItsOwnLine()
    {
        var ids = Enum.GetValues<TradeRefusal>().Select(TradeRefusalNames.IdOf).ToList();

        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ids, id => Assert.NotEmpty(RepositoryScreenText.Text.Messages.Text(id)));
    }
}
