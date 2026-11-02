using Jrpg.Game.Audio;

namespace Jrpg.Game.Tests.Audio;

public sealed class EffectChannelTests
{
    [Fact]
    public void PlayAll_SameFileTwice_KeepsItOnce()
    {
        var channel = new EffectChannel();

        channel.PlayAll(["move.wav", "move.wav", "confirm.wav"]);

        Assert.Equal(["move.wav", "confirm.wav"], channel.Played);
    }
}
