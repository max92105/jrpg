using Jrpg.Game.Dialogue;

namespace Jrpg.Game.Tests.Dialogue;

public sealed class TypewriterTests
{
    private static readonly TimeSpan _characterTime = TimeSpan.FromMilliseconds(30);

    private readonly Typewriter _typewriter = new(length: 5, _characterTime);

    [Fact]
    public void VisibleLength_New_IsZero()
    {
        Assert.Equal(0, _typewriter.VisibleLength);
        Assert.False(_typewriter.IsComplete);
    }

    [Fact]
    public void Update_OneCharacterTime_RevealsOneCharacter()
    {
        _typewriter.Update(_characterTime);

        Assert.Equal(1, _typewriter.VisibleLength);
    }

    [Fact]
    public void Update_TwoHalfCharacterTimes_RevealsOneCharacter()
    {
        _typewriter.Update(_characterTime / 2);
        _typewriter.Update(_characterTime / 2);

        Assert.Equal(1, _typewriter.VisibleLength);
    }

    [Fact]
    public void Update_PastWholePage_StopsAtLengthAndCompletes()
    {
        _typewriter.Update(TimeSpan.FromSeconds(1));

        Assert.Equal(5, _typewriter.VisibleLength);
        Assert.True(_typewriter.IsComplete);
    }

    [Fact]
    public void RevealAll_CompletesPage()
    {
        _typewriter.RevealAll();

        Assert.Equal(5, _typewriter.VisibleLength);
        Assert.True(_typewriter.IsComplete);
    }

    [Fact]
    public void New_ZeroCharacterTime_IsCompleteAtOnce()
    {
        var instant = new Typewriter(length: 5, TimeSpan.Zero);

        Assert.True(instant.IsComplete);
    }
}
