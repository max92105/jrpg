using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneTests
{
    /// <summary>Longer than the closing fade (0.12 s), so one run of them ends it.</summary>
    public const int ClosingFadeUpdates = 8;

    // Cancel starts the fade out; the flow hears the close once the fade has run, and once: a second request would make
    // it pop what lies under the Shop.
    [Fact]
    public void Update_Cancel_RequestsCloseOnceTheFadeHasRun()
    {
        var shop = NewShop();

        var atCancel = shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        var requests = Enumerable.Range(0, ClosingFadeUpdates).Select(_ => shop.Update(StartMapScenarios.OneUpdate, default)).ToList();
        var afterwards = shop.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Cancel));

        Assert.Null(atCancel);
        Assert.Single(requests, request => request is EventScreenClosed);
        Assert.Null(afterwards);
    }

    [Fact]
    public void Update_Confirm_RequestsNothing()
    {
        var shop = NewShop();

        Assert.Null(shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm)));
    }

    private static ShopScene NewShop()
    {
        GameSession session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        return new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
    }
}
