using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class MemberCardViewTests
{
    [Fact]
    public void HpPreviewOf_Rise_ReadsBothValuesWithTheAfterInTheRiseColor()
    {
        var cardView = new MemberCardView(RepositoryScreenText.Text, new UiTextures(), StartMapScenarios.BelowClosedDoor);

        var preview = cardView.HpPreviewOf(hp: 45, hpAfter: 75);

        Assert.Equal("45", preview.Now.Text);
        Assert.Equal("75", preview.After.Text);
        Assert.True(preview.After.Width > 0);
        Assert.Equal(UiTheme.Positive, preview.Color);
    }
}
