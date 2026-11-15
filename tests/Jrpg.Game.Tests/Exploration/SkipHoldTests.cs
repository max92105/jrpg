using Jrpg.Game.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Exploration;

// The hold to skip a cinematic (decision 623), at the game's 1/60 s step: a press arms it, and each update Confirm
// stays held after it counts toward the hold time.
public sealed class SkipHoldTests
{
    private static readonly InputFrame _press = InputFrames.Pressing(GameButton.Confirm);
    private static readonly InputFrame _hold = InputFrames.Holding(GameButton.Confirm);

    private readonly SkipHold _skip = new();

    [Fact]
    public void Update_PressThenHeldTwoSeconds_IsDone()
    {
        _skip.Update(StartMapScenarios.OneUpdate, _press);

        Hold(SkipHold.HoldTime);

        Assert.True(_skip.IsDone);
    }

    // The other side of the bound: one update short of the hold time.
    [Fact]
    public void Update_HeldJustUnderTwoSeconds_IsNotDone()
    {
        _skip.Update(StartMapScenarios.OneUpdate, _press);

        Hold(SkipHold.HoldTime - StartMapScenarios.OneUpdate);

        Assert.False(_skip.IsDone);
    }

    [Fact]
    public void Update_ReleasedBeforeTwoSeconds_ResetsProgress()
    {
        _skip.Update(StartMapScenarios.OneUpdate, _press);
        Hold(TimeSpan.FromSeconds(1));

        _skip.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal(0f, _skip.Progress);
    }

    [Fact]
    public void Update_HeldAgainAfterRelease_StartsFromZero()
    {
        _skip.Update(StartMapScenarios.OneUpdate, _press);
        Hold(TimeSpan.FromSeconds(1));
        _skip.Update(StartMapScenarios.OneUpdate, default);
        _skip.Update(StartMapScenarios.OneUpdate, _press);

        Hold(TimeSpan.FromSeconds(1.5));

        Assert.False(_skip.IsDone);
    }

    // A Confirm held from before, as the press that started the event: never armed.
    [Fact]
    public void Update_HeldWithoutAPress_DoesNotCount()
    {
        Hold(SkipHold.HoldTime * 2);

        Assert.Equal(0f, _skip.Progress);
    }

    [Fact]
    public void Update_SinglePress_IsNotDone()
    {
        _skip.Update(StartMapScenarios.OneUpdate, _press);

        Assert.False(_skip.IsDone);
        Assert.Equal(0f, _skip.Progress);
    }

    [Fact]
    public void Progress_HalfwayHeld_IsHalf()
    {
        _skip.Update(StartMapScenarios.OneUpdate, _press);

        _skip.Update(SkipHold.HoldTime / 2, _hold);

        Assert.Equal(0.5f, _skip.Progress);
    }

    [Fact]
    public void Progress_HeldLongerThanTheHoldTime_StaysAtOne()
    {
        _skip.Update(StartMapScenarios.OneUpdate, _press);

        _skip.Update(SkipHold.HoldTime * 3, _hold);

        Assert.Equal(1f, _skip.Progress);
    }

    // At the game's fixed step until the time is reached.
    private void Hold(TimeSpan time)
    {
        for (var held = TimeSpan.Zero; held < time; held += StartMapScenarios.OneUpdate)
        {
            _skip.Update(StartMapScenarios.OneUpdate, _hold);
        }
    }
}
