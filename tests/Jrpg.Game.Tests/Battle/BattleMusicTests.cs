using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattleMusicTests
{
    private readonly BattleMusic _music = new();

    [Fact]
    public void Cue_WhileFighting_IsTheTheme()
    {
        Assert.Equal(BattleMusicCue.Theme, _music.Cue);
    }

    [Fact]
    public void Advance_WhileFighting_StaysOnTheTheme()
    {
        _music.Advance(BattleMusic.FanfareHold);

        Assert.Equal(BattleMusicCue.Theme, _music.Cue);
    }

    [Fact]
    public void Advance_VictoryBeforeTheHold_IsSilence()
    {
        _music.ShowVictory();

        _music.Advance(BattleMusic.FanfareHold - TimeSpan.FromMilliseconds(1));

        Assert.Equal(BattleMusicCue.Silence, _music.Cue);
    }

    [Fact]
    public void Advance_VictoryPastTheHold_IsTheField()
    {
        _music.ShowVictory();

        _music.Advance(BattleMusic.FanfareHold);

        Assert.Equal(BattleMusicCue.Field, _music.Cue);
    }

    [Fact]
    public void Cue_Defeat_IsSilence()
    {
        _music.ShowDefeat();
        _music.Advance(BattleMusic.FanfareHold);

        Assert.Equal(BattleMusicCue.Silence, _music.Cue);
    }

    [Fact]
    public void Cue_Escape_IsTheField()
    {
        _music.ShowEscape();

        Assert.Equal(BattleMusicCue.Field, _music.Cue);
    }
}
