using Jrpg.Game.Battle;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

// A floating number (decisions 650-652): its digits' hops pinned to the reference clip's frames #80-#84 on a two-digit
// number, rounded to the pixel; its life; where its row rests.
public sealed class FloatingNumberTests
{
    private static readonly Rectangle _spot = new(100, 300, 200, 200);
    private static readonly Rectangle _tag = new(100, 530, 190, 30);

    [Fact]
    public void DigitOffsetAt_DuringTheWait_IsTheStagger()
    {
        Assert.Equal((0, 6), OffsetsAt(0.05));
    }

    [Fact]
    public void DigitOffsetAt_Frame80_FirstUpSecondAtRest()
    {
        Assert.Equal((-36, 6), OffsetsAt(0.100));
    }

    [Fact]
    public void DigitOffsetAt_Frame81_BothUp()
    {
        Assert.Equal((-36, -30), OffsetsAt(0.133));
    }

    [Fact]
    public void DigitOffsetAt_Frame82_BothStillUp()
    {
        Assert.Equal((-36, -30), OffsetsAt(0.167));
    }

    [Fact]
    public void DigitOffsetAt_Frame83_BothLanded()
    {
        Assert.Equal((0, 6), OffsetsAt(0.200));
    }

    [Fact]
    public void DigitOffsetAt_Frame84_AtRest()
    {
        Assert.Equal((0, 6), OffsetsAt(0.233));
    }

    [Fact]
    public void DigitOffsetAt_ThirdDigit_LandsAfterTheSecond()
    {
        Assert.Equal((-24, 12), (OffsetAt(2, 0.200), OffsetAt(2, 0.267)));
    }

    [Fact]
    public void IsShownAt_AtTheImpact_IsTrue()
    {
        Assert.True(FloatingNumber.IsShownAt(TimeSpan.Zero));
    }

    [Fact]
    public void IsShownAt_JustBeforeItsLifeEnds_IsTrue()
    {
        Assert.True(FloatingNumber.IsShownAt(TimeSpan.FromSeconds(0.57) - TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void IsShownAt_AtItsLifeEnd_IsFalse()
    {
        Assert.False(FloatingNumber.IsShownAt(TimeSpan.FromSeconds(0.57)));
    }

    // A backfire's number, before its beat.
    [Fact]
    public void IsShownAt_BeforeItsMoment_IsFalse()
    {
        Assert.False(FloatingNumber.IsShownAt(-TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void RowBottomOf_NoTag_IsBelowTheFeet()
    {
        Assert.Equal(510, FloatingNumber.RowBottomOf(_spot, null, 3));
    }

    [Fact]
    public void RowBottomOf_TagOneDigit_IsAboveTheTag()
    {
        Assert.Equal(526, FloatingNumber.RowBottomOf(_spot, _tag, 1));
    }

    // Each digit rests 6 px lower than the one before, so the third's bottom is the one 4 px above the tag.
    [Fact]
    public void RowBottomOf_TagThreeDigits_KeepsTheLastDigitAboveIt()
    {
        Assert.Equal(514, FloatingNumber.RowBottomOf(_spot, _tag, 3));
    }

    [Fact]
    public void ColorOf_PoisonTick_IsPoisonTint()
    {
        Assert.Equal(UiTheme.FloatingPoison, FloatingNumber.ColorOf(FloatingNumberTone.Poison));
    }

    [Fact]
    public void ColorOf_Critical_IsTheCriticalTint()
    {
        Assert.Equal(UiTheme.FloatingCritical, FloatingNumber.ColorOf(FloatingNumberTone.Critical));
    }

    [Fact]
    public void SizeOf_Critical_IsBigger()
    {
        Assert.Equal(1.4f, FloatingNumber.SizeOf(FloatingNumberTone.Critical));
    }

    [Fact]
    public void SizeOf_Damage_IsTheNormalSize()
    {
        Assert.Equal(1, FloatingNumber.SizeOf(FloatingNumberTone.Damage));
    }

    [Fact]
    public void ColorOf_DamageAndHeal_KeepTheirColors()
    {
        Assert.Equal(
            (UiTheme.TextPrimary, UiTheme.FloatingHeal),
            (FloatingNumber.ColorOf(FloatingNumberTone.Damage), FloatingNumber.ColorOf(FloatingNumberTone.Heal)));
    }

    private static (int First, int Second) OffsetsAt(double seconds) => (OffsetAt(0, seconds), OffsetAt(1, seconds));

    private static int OffsetAt(int digit, double seconds) =>
        (int)MathF.Round(FloatingNumber.DigitOffsetAt(digit, TimeSpan.FromSeconds(seconds)));
}
