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
}
