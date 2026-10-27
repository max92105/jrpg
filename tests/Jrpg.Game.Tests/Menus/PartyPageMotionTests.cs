using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The Party page's card pointer and its actions' highlight glide (decision 262). Sarxian leads alone at first.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class PartyPageMotionTests
{
    private static readonly TimeSpan _glideDone = TimeSpan.FromSeconds(1);

    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Advance_AfterDownInTheReserve_CardPointerReachesTheNextCard()
    {
        var page = NewOpenPage();
        page.Update(Pressing(GameButton.Right));
        page.Update(Pressing(GameButton.Down));

        page.Advance(_glideDone);

        Assert.Equal(1, page.PointerPositions.Cards);
    }

    [Fact]
    public void Advance_AfterDownInTheActions_HighlightReachesTheSecondAction()
    {
        var page = NewOpenPage();
        page.Update(Pressing(GameButton.Right));
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Down));

        page.Advance(_glideDone);

        Assert.Equal(1, page.PointerPositions.Actions);
    }

    private PartyPage NewOpenPage()
    {
        var page = new PartyPage(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        return page;
    }
}
