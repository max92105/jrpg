using Jrpg.Game.Audio;

namespace Jrpg.Game.Tests.Audio;

public sealed class VoiceChannelTests
{
    [Fact]
    public void Play_File_SetsVoiceFile()
    {
        var voice = new VoiceChannel();

        voice.Play("Placeholder.wav");

        Assert.Equal("Placeholder.wav", voice.VoiceFile);
    }

    [Fact]
    public void Stop_AfterPlay_ClearsVoiceFile()
    {
        var voice = new VoiceChannel();
        voice.Play("Placeholder.wav");

        voice.Stop();

        Assert.Null(voice.VoiceFile);
    }
}
