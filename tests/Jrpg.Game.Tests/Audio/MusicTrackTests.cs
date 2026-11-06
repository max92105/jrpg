using Jrpg.Game.Audio;

namespace Jrpg.Game.Tests.Audio;

public sealed class MusicTrackTests
{
    [Fact]
    public void IntroBeside_LoopInAFolder_IsTheIntroInThatFolder()
    {
        Assert.Equal("titlescreen/intro.wav", MusicTrack.IntroBeside("titlescreen/loop.wav"));
    }

    [Fact]
    public void OutroBeside_LoopAtTheRoot_IsTheOutroAtTheRoot()
    {
        Assert.Equal("outro.wav", MusicTrack.OutroBeside("Theme.wav"));
    }

    [Fact]
    public void Parts_IntroAndOutro_SurroundTheLoop()
    {
        var title = new MusicTrack("titlescreen/loop.wav", HasIntro: true, HasOutro: true);

        Assert.Equal(["titlescreen/intro.wav", "titlescreen/loop.wav", "titlescreen/outro.wav"], title.Parts);
        Assert.True(title.CyclesParts);
    }

    [Fact]
    public void Parts_OutroOnly_IsTheLoopThenTheOutro()
    {
        var track = new MusicTrack("town/loop.wav", HasOutro: true);

        Assert.Equal(["town/loop.wav", "town/outro.wav"], track.Parts);
        Assert.True(track.CyclesParts);
    }

    [Fact]
    public void Parts_IntroOnly_IsTheIntroThenTheLoop()
    {
        var track = new MusicTrack("town/loop.wav", HasIntro: true);

        Assert.Equal(["town/intro.wav", "town/loop.wav"], track.Parts);
        Assert.True(track.CyclesParts);
    }

    [Fact]
    public void CyclesParts_LoopAlone_IsFalse()
    {
        Assert.False(new MusicTrack("town/loop.wav").CyclesParts);
    }
}
