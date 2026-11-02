using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The tabs' move, confirm and cancel, and the open page's sounds added to the menu's (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameMenuSceneSoundTests
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    private readonly SoundingPage _page = new();
    private readonly GameMenuScene _menu;

    public GameMenuSceneSoundTests()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        _menu = new GameMenuScene(RepositoryScreenText.Text, new UiTextures(), HeaderOf(session), [_page, new SoundingPage()]);
    }

    [Fact]
    public void Update_DownOnTheTabs_AsksTheMoveSound()
    {
        _menu.Update(_oneUpdate, Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _menu.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnATab_AsksTheConfirmSound()
    {
        _menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _menu.Sounds);
    }

    [Fact]
    public void Update_CancelOnTheTabs_AsksTheCancelSound()
    {
        _menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], _menu.Sounds);
    }

    [Fact]
    public void Update_CancelInAPage_AsksTheCancelSound()
    {
        _menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        _menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], _menu.Sounds);
    }

    [Fact]
    public void Update_PageAsksASound_SceneAsksIt()
    {
        _menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        _page.Asks = ["page.wav"];

        _menu.Update(_oneUpdate, default);

        Assert.Equal(["page.wav"], _menu.Sounds);
    }

    [Fact]
    public void Update_NothingPressedOnTheTabs_AsksNothing()
    {
        _menu.Update(_oneUpdate, Pressing(GameButton.Down));

        _menu.Update(_oneUpdate, default);

        Assert.Empty(_menu.Sounds);
    }

    private static MenuHeader HeaderOf(GameSession session) => new(RepositoryScreenText.Text, new UiTextures(), session);

    private sealed class SoundingPage : IMenuPage
    {
        public IReadOnlyList<string> Asks { get; set; } = [];

        public string TitleMessageId => MessageIds.MenuOptions;

        public TabGlyph Glyph => TabGlyph.Options;

        public string Hint => "";

        public bool LeavesOnCancel => true;

        public IReadOnlyList<string> Sounds => Asks;

        public void Open()
        {
        }

        public SceneRequest? Update(InputFrame input) => null;

        public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
        {
        }
    }
}
