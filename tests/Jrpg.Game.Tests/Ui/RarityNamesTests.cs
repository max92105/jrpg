using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class RarityNamesTests
{
    public static TheoryData<Rarity> EveryRarity => [.. Enum.GetValues<Rarity>()];

    [Theory]
    [MemberData(nameof(EveryRarity))]
    public void IdOf_EveryRarity_HasText(Rarity rarity)
    {
        var name = RepositoryScreenText.Text.Messages.Text(RarityNames.IdOf(rarity));

        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    // Common is the body text; each rolled or reserved tier has a color of its own (decision 362).
    [Fact]
    public void RarityColorOf_EveryRarity_CommonIsBodyTextOthersDistinct()
    {
        var colors = Enum.GetValues<Rarity>().Select(UiTheme.RarityColorOf).ToList();

        Assert.Equal(UiTheme.TextPrimary, colors[0]);
        Assert.Equal(colors.Count, colors.Distinct().Count());
    }
}
