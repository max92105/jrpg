using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class MenuHeaderTests
{
    // The header formats the gold only when the purse changed; a new game starts with the content's starting gold.
    [Fact]
    public void Update_GoldChanged_ShowsTheNewAmount()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var header = new MenuHeader(RepositoryScreenText.Text, new UiTextures(), session);
        var before = header.ShownGold;
        session.Party.Purse.Add(250);

        header.Update();

        Assert.Equal(NumberText.Of(session.Party.Purse.Gold), header.ShownGold);
        Assert.NotEqual(before, header.ShownGold);
    }
}
