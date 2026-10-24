using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Game.Input;
using Jrpg.Game.NewGame;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.NewGame;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class LeaderChoiceSceneTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    [Fact]
    public void Update_Confirm_AsksBeforeChoosing()
    {
        var scene = NewScene();

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.True(scene.IsAsking);
    }

    [Fact]
    public void Update_ConfirmOnZanyrThenYes_RequestsZanyr()
    {
        var scene = NewScene();
        Select(scene, "zanyr");
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var request = AnswerYes(scene);

        Assert.Equal("zanyr", Assert.IsType<LeaderChosen>(request).Choice.MemberId);
    }

    [Fact]
    public void Update_ConfirmAfterYes_RequestsNothingMore()
    {
        var scene = NewScene();
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));
        AnswerYes(scene);

        var second = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(second);
    }

    [Fact]
    public void Update_ConfirmThenNo_StaysOnTheSameCharacter()
    {
        var scene = NewScene();
        Select(scene, "zanyr");
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.False(scene.IsAsking);
        Assert.Equal("zanyr", scene.Selected.MemberId);
    }

    [Fact]
    public void Update_CancelWhileAsking_ClosesTheQuestionOnly()
    {
        var scene = NewScene();
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Null(request);
        Assert.False(scene.IsAsking);
    }

    [Fact]
    public void Update_DownWhileAsking_KeepsTheCursor()
    {
        var scene = NewScene();
        var first = scene.Selected;
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        scene.Update(_oneUpdate, Pressing(GameButton.Down));

        Assert.Same(first, scene.Selected);
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

    [Fact]
    public void ShownClassName_CursorOnJazog_IsBarbarian()
    {
        var scene = NewScene();

        Select(scene, "jazog");

        Assert.Equal("Barbarian", scene.SelectedProfile.ClassName);
    }

    [Fact]
    public void ShownStats_CursorOnOmer_AreTheMageAtLevelOne()
    {
        var scene = NewScene();
        var mage = ClassGrowth.StatsAt(_catalog.Classes["mage"], 1);

        Select(scene, "omer");

        string[] expected = ["1", .. new[] { mage.MaxHp, mage.MaxMp, mage.Attack, mage.Defense }.Select(NumberText.Of)];
        Assert.Equal(expected, scene.SelectedProfile.Stats.Select(line => line.Value));
    }

    [Fact]
    public void Update_Down_ShowsTheNextProfileFadingIn()
    {
        var scene = NewScene();

        scene.Update(_oneUpdate, Pressing(GameButton.Down));

        Assert.Equal((1, 0f), scene.ShownProfile);
    }

    private static LeaderChoiceScene NewScene() => new(RepositoryScreenText.Text, new UiTextures(), _catalog);

    // No starts selected, so a stray Confirm changes nothing: Left moves to Yes.
    private static SceneRequest? AnswerYes(LeaderChoiceScene scene)
    {
        scene.Update(_oneUpdate, Pressing(GameButton.Left));
        return scene.Update(_oneUpdate, Pressing(GameButton.Confirm));
    }

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
