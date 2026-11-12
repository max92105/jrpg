using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

/// <summary>The rows: Full screen first, then Text speed, Music, Sound effects and Voices.</summary>
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
    public void Open_AfterMovingDown_SelectsFullScreen()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        page.Open();
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
        var page = PageOn(OptionsPage.TextSpeedRow);

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(TextSpeed.Fast, _settings.TextSpeed);
    }

    [Fact]
    public void Update_RightOnTextSpeed_LastValue_StaysOnLast()
    {
        _settings.TextSpeed = TextSpeed.Instant;
        var page = PageOn(OptionsPage.TextSpeedRow);

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(TextSpeed.Instant, _settings.TextSpeed);
    }

    [Fact]
    public void Update_LeftOnTextSpeed_GoesToSlower()
    {
        var page = PageOn(OptionsPage.TextSpeedRow);

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(TextSpeed.Slow, _settings.TextSpeed);
    }

    [Fact]
    public void Update_LeftOnTextSpeed_FirstValue_StaysOnFirst()
    {
        _settings.TextSpeed = TextSpeed.Slow;
        var page = PageOn(OptionsPage.TextSpeedRow);

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(TextSpeed.Slow, _settings.TextSpeed);
    }

    [Fact]
    public void Advance_AfterMovingDown_SelectionReachesTheNextOption()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        page.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(1, page.SelectionPosition);
    }

    [Fact]
    public void Update_DownFromTextSpeed_SelectsMusic()
    {
        var page = PageOn(OptionsPage.TextSpeedRow);

        page.Update(Pressing(GameButton.Down));

        Assert.Equal(OptionsPage.MusicRow, page.SelectedRow);
    }

    [Fact]
    public void Update_RightOnMusic_RaisesItOneStep()
    {
        _settings.MusicVolume = 3;
        _settings.EffectsVolume = 6;
        var page = PageOn(OptionsPage.MusicRow);

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(4, _settings.MusicVolume);
        Assert.Equal(6, _settings.EffectsVolume);
    }

    [Fact]
    public void Update_LeftOnEffects_LowersItOneStep()
    {
        _settings.EffectsVolume = 5;
        _settings.MusicVolume = 2;
        var page = PageOn(OptionsPage.EffectsRow);

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(4, _settings.EffectsVolume);
        Assert.Equal(2, _settings.MusicVolume);
    }

    [Fact]
    public void Update_LeftOnEffectsAtZero_StaysAtZero()
    {
        _settings.EffectsVolume = 0;
        var page = PageOn(OptionsPage.EffectsRow);

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(0, _settings.EffectsVolume);
    }

    [Fact]
    public void Update_LeftOnVoices_LowersIt()
    {
        _settings.VoiceVolume = 5;
        var page = PageOn(OptionsPage.VoicesRow);

        page.Update(Pressing(GameButton.Left));

        Assert.Equal(4, _settings.VoiceVolume);
    }

    [Fact]
    public void Update_RightOnVoicesAtMax_StaysAtMax()
    {
        _settings.VoiceVolume = VolumeLevel.Max;
        var page = PageOn(OptionsPage.VoicesRow);

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(VolumeLevel.Max, _settings.VoiceVolume);
    }

    [Fact]
    public void Update_DownToTheNextRow_AsksTheMoveSound()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_RightOnMusic_AsksTheMoveSound()
    {
        _settings.MusicVolume = 3;
        var page = PageOn(OptionsPage.MusicRow);

        page.Update(Pressing(GameButton.Right));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_RightOnVoicesAtMax_AsksNothing()
    {
        _settings.VoiceVolume = VolumeLevel.Max;
        var page = PageOn(OptionsPage.VoicesRow);

        page.Update(Pressing(GameButton.Right));

        Assert.Empty(page.Sounds);
    }

    private OptionsPage PageOn(int row)
    {
        var page = NewPage();
        for (var presses = 0; presses < 5 && page.SelectedRow != row; presses++)
        {
            page.Update(Pressing(GameButton.Down));
        }

        Assert.Equal(row, page.SelectedRow);
        return page;
    }

    private OptionsPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _settings);
}
