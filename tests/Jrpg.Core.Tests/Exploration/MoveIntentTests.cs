using Jrpg.Core.Exploration;

namespace Jrpg.Core.Tests.Exploration;

public sealed class MoveIntentTests
{
    [Fact]
    public void FromAxes_KeyboardDiagonal_HasLengthOne()
    {
        var intent = MoveIntent.FromAxes(1, 1);

        Assert.Equal(1, intent.Magnitude, precision: 12);
        Assert.Equal(intent.X, intent.Y);
    }

    [Fact]
    public void FromAxes_HalfTilt_KeepsHalfMagnitude()
    {
        var intent = MoveIntent.FromAxes(0, -0.5);

        Assert.Equal(0.5, intent.Magnitude, precision: 12);
        Assert.Equal(-0.5, intent.Y);
    }

    [Fact]
    public void None_HasNoMagnitude()
    {
        Assert.Equal(0, MoveIntent.None.Magnitude);
    }
}
