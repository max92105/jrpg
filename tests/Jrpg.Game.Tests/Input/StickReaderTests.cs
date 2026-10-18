using Jrpg.Core.Exploration;
using Jrpg.Game.Input;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Input;

public sealed class StickReaderTests
{
    [Fact]
    public void FromStick_InsideDeadzone_ReturnsNone()
    {
        var move = StickReader.FromStick(new Vector2(0.1f, -0.15f));

        Assert.Equal(MoveIntent.None, move);
    }

    [Fact]
    public void FromStick_ExactlyAtDeadzone_Moves()
    {
        var move = StickReader.FromStick(new Vector2(StickReader.Deadzone, 0));

        Assert.Equal(StickReader.Deadzone, move.X, 6);
    }

    [Fact]
    public void FromStick_StickPushedUp_GivesNegativeY()
    {
        var move = StickReader.FromStick(new Vector2(0, 1));

        Assert.Equal(0, move.X);
        Assert.Equal(-1, move.Y);
    }

    [Fact]
    public void FromStick_HalfTilt_MovesAtHalfSpeed()
    {
        var move = StickReader.FromStick(new Vector2(0.5f, 0));

        Assert.Equal(0.5, move.Magnitude, 6);
    }

    [Fact]
    public void FromDirectionButtons_UpAndLeft_HasLengthOne()
    {
        var held = ButtonSet.Empty.With(GameButton.Up).With(GameButton.Left);

        var move = StickReader.FromDirectionButtons(held);

        Assert.Equal(1, move.Magnitude, 6);
        Assert.True(move.X < 0);
        Assert.True(move.Y < 0);
    }

    [Fact]
    public void FromDirectionButtons_OppositeButtons_CancelOut()
    {
        var held = ButtonSet.Empty.With(GameButton.Left).With(GameButton.Right).With(GameButton.Down);

        var move = StickReader.FromDirectionButtons(held);

        Assert.Equal(0, move.X);
        Assert.Equal(1, move.Y);
    }
}
