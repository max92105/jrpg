using Jrpg.Game.Dialogue;

namespace Jrpg.Game.Tests.Dialogue;

public sealed class TypingTicksTests
{
    private const string Page = "ab cd";

    [Fact]
    public void IsDue_SecondCharacterTyped_IsTrue() => Assert.True(TypingTicks.IsDue(Page, 1, 2, TimeSpan.FromSeconds(1)));

    [Fact]
    public void IsDue_FirstCharacterTyped_IsFalse() => Assert.False(TypingTicks.IsDue(Page, 0, 1, TypingTicks.MinimumGap));

    // The fourth character of "a c d" is the space before the d.
    [Fact]
    public void IsDue_MultipleOnASpace_IsFalse() => Assert.False(TypingTicks.IsDue("a c d", 3, 4, TypingTicks.MinimumGap));

    [Fact]
    public void IsDue_NothingTyped_IsFalse() => Assert.False(TypingTicks.IsDue(Page, 2, 2, TypingTicks.MinimumGap));

    [Fact]
    public void IsDue_TooSoonAfterTheLastTick_IsFalse() =>
        Assert.False(TypingTicks.IsDue(Page, 1, 2, TypingTicks.MinimumGap - TimeSpan.FromMilliseconds(1)));

    [Fact]
    public void IsDue_GapReachedExactly_IsTrue() => Assert.True(TypingTicks.IsDue(Page, 1, 2, TypingTicks.MinimumGap));

    [Fact]
    public void IsDue_JumpOverTwoMultiples_IsTrue() => Assert.True(TypingTicks.IsDue("abcdef", 1, 4, TypingTicks.MinimumGap));
}
