using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

/// <summary>The rows: Full screen first, then Text speed.</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class OptionsPageTests
{
    private readonly PlayerSettings _settings = new();

    [Fact]
    public void Update_RightOnFullScreen_TogglesSetting()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Right));

        Assert.False(_settings.IsFullScreen);
    }

    [Fact]
    public void Update_LeftOnFullScreen_TogglesSettingBack()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));

        page.Update(Pressing(GameButton.Left));

        Assert.True(_settings.IsFullScreen);
    }

    [Fact]
    public void Update_RightOnTextSpeed_GoesToFaster()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(TextSpeed.Fast, _settings.TextSpeed);
    }

    [Fact]
    public void Update_RightOnTextSpeed_LastValue_StaysOnLast()
    {
        _settings.TextSpeed = TextSpeed.Instant;
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(TextSpeed.Instant, _settings.TextSpeed);
    }

    [Fact]
    public void Update_LeftOnTextSpeed_GoesToSlower()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(TextSpeed.Slow, _settings.TextSpeed);
    }

    [Fact]
    public void Update_LeftOnTextSpeed_FirstValue_StaysOnFirst()
    {
        _settings.TextSpeed = TextSpeed.Slow;
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(TextSpeed.Slow, _settings.TextSpeed);
    }

    private OptionsPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _settings);
}
