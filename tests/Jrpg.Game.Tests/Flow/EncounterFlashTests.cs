using Jrpg.Game.Flow;

namespace Jrpg.Game.Tests.Flow;

public sealed class EncounterFlashTests
{
    private static readonly TimeSpan _rise = TimeSpan.FromSeconds(0.04);
    private static readonly TimeSpan _wholeFlash = TimeSpan.FromSeconds(0.12);

    [Fact]
    public void Whiteness_BeforeStart_IsZero()
    {
        var flash = new EncounterFlash();

        flash.Advance(_rise);

        Assert.Equal(0, flash.Whiteness);
        Assert.False(flash.IsPlaying);
    }

    [Fact]
    public void Whiteness_AtThePeak_IsTheMaximum()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_rise);

        Assert.Equal(0.85f, flash.Whiteness, precision: 3);
    }

    [Fact]
    public void Whiteness_HalfwayDown_IsHalfThePeak()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_rise + TimeSpan.FromSeconds(0.04));

        Assert.Equal(0.425f, flash.Whiteness, precision: 3);
    }

    [Fact]
    public void IsPlaying_AfterTheFlash_IsFalse()
    {
        var flash = new EncounterFlash();
        flash.Start();
        Assert.True(flash.IsPlaying);

        flash.Advance(_wholeFlash);

        Assert.False(flash.IsPlaying);
        Assert.Equal(0, flash.Whiteness);
    }

    [Fact]
    public void IsPastPeak_BeforeStart_IsFalse()
    {
        var flash = new EncounterFlash();

        flash.Advance(_wholeFlash);

        Assert.False(flash.IsPastPeak);
    }

    [Fact]
    public void IsPastPeak_JustBeforeThePeak_IsFalse()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_rise - TimeSpan.FromMilliseconds(1));

        Assert.False(flash.IsPastPeak);
    }

    [Fact]
    public void IsPastPeak_AtThePeak_IsTrue()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_rise);

        Assert.True(flash.IsPastPeak);
    }

    // The shards stay up through the fade, after the flash.
    [Fact]
    public void IsPastPeak_AfterTheFlash_StaysTrue()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_wholeFlash + TimeSpan.FromSeconds(1));

        Assert.True(flash.IsPastPeak);
    }
}
