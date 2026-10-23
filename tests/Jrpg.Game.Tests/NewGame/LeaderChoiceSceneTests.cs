using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Data.Content;
using Jrpg.Game.Input;
using Jrpg.Game.NewGame;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.NewGame;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class LeaderChoiceSceneTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    [Fact]
    public void Update_ConfirmOnZanyr_RequestsZanyr()
    {
        var scene = NewScene();
        Select(scene, "zanyr");

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal("zanyr", Assert.IsType<LeaderChosen>(request).Choice.MemberId);
    }

    [Fact]
    public void Update_ConfirmTwice_RequestsLeaderOnce()
    {
        var scene = NewScene();
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var second = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(second);
    }

    [Fact]
    public void Update_Cancel_RequestsTitle()
    {
        var scene = NewScene();

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.IsType<TitleRequested>(request);
    }

    [Fact]
    public void Update_CancelAndConfirmTogether_RequestsTitle()
    {
        var scene = NewScene();

        var both = ButtonSet.Empty.With(GameButton.Cancel).With(GameButton.Confirm);

        var request = scene.Update(_oneUpdate, new InputFrame(MoveIntent.None, both, both));

        Assert.IsType<TitleRequested>(request);
    }

    [Fact]
    public void Update_NothingPressed_RequestsNothing()
    {
        var scene = NewScene();

        Assert.Null(scene.Update(_oneUpdate, default));
    }

    private static LeaderChoiceScene NewScene() => new(RepositoryScreenText.Text, _catalog);

    // Bounded by the number of choices, so a character who isn't offered fails the test instead of looping forever.
    private static void Select(LeaderChoiceScene scene, string memberId)
    {
        for (var presses = 0; presses < _catalog.NewGame.LeaderChoices.Count && scene.Selected.MemberId != memberId; presses++)
        {
            scene.Update(_oneUpdate, Pressing(GameButton.Down));
        }

        Assert.Equal(memberId, scene.Selected.MemberId);
    }
}
