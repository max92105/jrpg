using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Game.Input;
using Jrpg.Game.NewGame;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.NewGame;

// Move, confirm and cancel on the leader choice; "Begin as X?" is silent (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class LeaderChoiceSceneSoundTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    private readonly LeaderChoiceScene _scene = new(RepositoryScreenText.Text, new UiTextures(), _catalog);

    [Fact]
    public void Update_Down_AsksTheMoveSound()
    {
        _scene.Update(_oneUpdate, Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _scene.Sounds);
    }

    [Fact]
    public void Update_Confirm_AsksTheConfirmSound()
    {
        _scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _scene.Sounds);
    }

    [Fact]
    public void Update_Cancel_AsksTheCancelSound()
    {
        _scene.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], _scene.Sounds);
    }

    [Fact]
    public void Update_ConfirmInThePrompt_AsksNothing()
    {
        _scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        _scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Empty(_scene.Sounds);
    }
}
