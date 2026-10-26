using Jrpg.Game.Flow;

namespace Jrpg.Game.Tests.Flow;

public sealed class EncounterShatterTests
{
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);

    [Fact]
    public void Share_BeforeStart_IsZero()
    {
        var shatter = new EncounterShatter(_fadeOut);

        shatter.Advance(_fadeOut);

        Assert.Equal(0, shatter.Share);
    }

    // The flow advances the shatter through the flash, before the fade starts it: the start must still be at rest.
    [Fact]
    public void Share_AtStart_IsZero()
    {
        var shatter = new EncounterShatter(_fadeOut);
        shatter.Advance(_fadeOut);

        shatter.Start();

        Assert.Equal(0, shatter.Share);
    }

    [Fact]
    public void Share_HalfwayThroughTheFadeOut_IsPastHalfway()
    {
        var shatter = new EncounterShatter(_fadeOut);
        shatter.Start();

        shatter.Advance(_fadeOut / 2);

        Assert.InRange(shatter.Share, 0.51f, 0.99f);
    }

    [Fact]
    public void Share_AfterTheFadeOut_IsOne()
    {
        var shatter = new EncounterShatter(_fadeOut);
        shatter.Start();

        shatter.Advance(_fadeOut);

        Assert.Equal(1, shatter.Share);
        Assert.Equal((float)_fadeOut.TotalSeconds, shatter.FlightSeconds, 3);
    }
}
