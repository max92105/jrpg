using Jrpg.Game.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// A floating number: grows from 60 % in 0.18 s, rises 50 px over 1.6 s, fades over the second half.
public sealed class FloatingNumberTests
{
    [Fact]
    public void At_Start_IsPopScale()
    {
        var look = FloatingNumber.At(TimeSpan.Zero);

        Assert.Equal(new FloatingNumberLook(0.6f, 0, 1), look);
    }

    [Fact]
    public void At_PastHalfLife_IsFading()
    {
        var look = FloatingNumber.At(TimeSpan.FromSeconds(1.2));

        Assert.Equal(0.5f, look.Opacity, 3);
        Assert.Equal(1, look.Scale);
    }

    [Fact]
    public void At_EndOfLife_HasRisenFullyAndIsGone()
    {
        var look = FloatingNumber.At(TimeSpan.FromSeconds(1.6));

        Assert.Equal(new FloatingNumberLook(1, FloatingNumber.RiseDistance, 0), look);
    }

    [Fact]
    public void ColorOf_PoisonTick_IsPoisonTint()
    {
        Assert.Equal(UiTheme.FloatingPoison, FloatingNumber.ColorOf(FloatingNumberTone.Poison));
    }

    [Fact]
    public void ColorOf_DamageAndHeal_KeepTheirColors()
    {
        Assert.Equal(
            (UiTheme.TextPrimary, UiTheme.FloatingHeal),
            (FloatingNumber.ColorOf(FloatingNumberTone.Damage), FloatingNumber.ColorOf(FloatingNumberTone.Heal)));
    }
}
