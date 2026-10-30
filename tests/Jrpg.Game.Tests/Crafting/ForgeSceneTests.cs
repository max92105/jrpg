using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Crafting;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Crafting;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeSceneTests
{
    /// <summary>Longer than the closing fade (0.12 s), so one run of them ends it.</summary>
    public const int ClosingFadeUpdates = 8;

    // Cancel starts the fade out; the flow hears the close once the fade has run, and once: a second request would make
    // it pop what lies under the Forge.
    [Fact]
    public void Update_Cancel_RequestsCloseOnceTheFadeHasRun()
    {
        var forge = NewForge(ForgeSessions.WithoutLoosePieces());

        var atCancel = forge.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        var requests = Enumerable.Range(0, ClosingFadeUpdates).Select(_ => forge.Update(StartMapScenarios.OneUpdate, default)).ToList();
        var afterwards = forge.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Cancel));

        Assert.Null(atCancel);
        Assert.Single(requests, request => request is EventScreenClosed);
        Assert.Null(afterwards);
    }

    [Fact]
    public void Update_Confirm_RequestsNothing()
    {
        var forge = NewForge(ForgeSessions.WithoutLoosePieces());

        Assert.Null(forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm)));
    }

    [Fact]
    public void Update_Down_MovesTheList()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        var items = StartMapScenarios.BelowClosedDoor.Items;
        session.Inventory.AddGear(GearPiece.Plain(items["bronze_sword"]));
        session.Inventory.AddGear(GearPiece.Plain(items["wooden_shield"]));
        var forge = NewForge(session);
        var first = forge.SelectedPiece;

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Equal(session.Inventory.Gear[1], forge.SelectedPiece);
        Assert.NotEqual(first, forge.SelectedPiece);
    }

    private static ForgeScene NewForge(GameSession session) =>
        new(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor.Items);
}
