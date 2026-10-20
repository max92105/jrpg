using Jrpg.Game.Hosting;

namespace Jrpg.Game.Tests.Hosting;

public sealed class PlayerSettingsTests
{
    // TextSpeed is internal, so the theory names it rather than taking it.
    [Theory]
    [InlineData(nameof(TextSpeed.Slow), 45)]
    [InlineData(nameof(TextSpeed.Normal), 30)]
    [InlineData(nameof(TextSpeed.Fast), 15)]
    [InlineData(nameof(TextSpeed.Instant), 0)]
    public void CharacterTime_EachSpeed_MatchesQuestion90(string speed, int milliseconds)
    {
        var settings = new PlayerSettings { TextSpeed = Enum.Parse<TextSpeed>(speed) };

        Assert.Equal(TimeSpan.FromMilliseconds(milliseconds), settings.CharacterTime);
    }
}
