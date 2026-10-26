using Jrpg.Game.Flow;

namespace Jrpg.Game.Tests.Flow;

public sealed class EncounterFlashTests
{
    private static readonly TimeSpan _rise = TimeSpan.FromSeconds(0.04);
    private static readonly TimeSpan _whole = TimeSpan.FromSeconds(0.12);

    [Fact]
    public void Whiteness_BeforeStart_IsZero()
    {
        var flash = new EncounterFlash();

        flash.Advance(_rise);

        Assert.Equal(0, flash.Whiteness);
    }

    [Fact]
    public void Whiteness_AtThePeak_IsTheMaximum()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_rise);

        Assert.Equal(EncounterFlash.PeakWhiteness, flash.Whiteness, 3);
    }

    [Fact]
    public void Whiteness_HalfwayDown_IsHalfThePeak()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_rise + TimeSpan.FromSeconds(0.04));

        Assert.Equal(EncounterFlash.PeakWhiteness / 2, flash.Whiteness, 3);
    }

    [Fact]
    public void IsPlaying_DuringTheFlash_IsTrue()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_whole - TimeSpan.FromSeconds(0.01));

        Assert.True(flash.IsPlaying);
    }

    [Fact]
    public void IsPlaying_AfterTheFlash_IsFalse()
    {
        var flash = new EncounterFlash();
        flash.Start();

        flash.Advance(_whole);

        Assert.False(flash.IsPlaying);
        Assert.Equal(0, flash.Whiteness);
    }
}
