using Jrpg.Game.Flow;

namespace Jrpg.Game.Tests.Flow;

public sealed class EncounterShatterTests
{
    // Between the crack and the fade the shards hold their places, however long the game has run.
    [Fact]
    public void Share_BeforeStart_IsZero()
    {
        var shatter = new EncounterShatter(TimeSpan.FromSeconds(0.5));

        shatter.Advance(TimeSpan.FromSeconds(10));

        Assert.Equal(0, shatter.Share);
    }

    [Fact]
    public void Share_AtStart_IsZero()
    {
        var shatter = new EncounterShatter(TimeSpan.FromSeconds(0.5));
        shatter.Advance(TimeSpan.FromSeconds(0.3));

        shatter.Start();

        Assert.Equal(0, shatter.Share);
    }

    [Fact]
    public void Share_AfterTheFadeOut_IsOne()
    {
        var shatter = new EncounterShatter(TimeSpan.FromSeconds(0.5));
        shatter.Start();

        shatter.Advance(TimeSpan.FromSeconds(0.5));

        Assert.Equal(1, shatter.Share);
    }

    [Fact]
    public void Share_Halfway_IsPastHalf()
    {
        var shatter = new EncounterShatter(TimeSpan.FromSeconds(0.5));
        shatter.Start();

        shatter.Advance(TimeSpan.FromSeconds(0.25));

        Assert.InRange(shatter.Share, 0.5f, 1f);
    }
}
