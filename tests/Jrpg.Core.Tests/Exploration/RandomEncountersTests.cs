using Jrpg.Core.Exploration;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Exploration;

public sealed class RandomEncountersTests
{
    private static readonly RandomEncounterTable _forest = new()
    {
        OneInSteps = 25,
        GraceSteps = 2,
        EncounterIds = ["goblin_trio", "wolf_pack", "slime"],
    };

    [Fact]
    public void RollStep_NoTable_DoesNotDraw()
    {
        var encounters = new RandomEncounters(new FixedSequenceRandom());

        var encounterId = encounters.RollStep(table: null);

        Assert.Null(encounterId);
    }

    [Fact]
    public void RollStep_WithinGraceSteps_DoesNotDraw()
    {
        var encounters = new RandomEncounters(new FixedSequenceRandom());

        var rolls = new[] { encounters.RollStep(_forest), encounters.RollStep(_forest) };

        Assert.Equal([null, null], rolls);
    }

    [Fact]
    public void RollStep_FirstStepAfterGrace_Draws()
    {
        var random = new FixedSequenceRandom(7);
        var encounters = new RandomEncounters(random);
        encounters.RollStep(_forest);
        encounters.RollStep(_forest);

        var encounterId = encounters.RollStep(_forest);

        Assert.Null(encounterId);
        Assert.Throws<InvalidOperationException>(() => random.NextBelow(1));
    }

    [Fact]
    public void RollStep_RollZero_ReturnsEncounterPickedBySecondDraw()
    {
        var encounters = new RandomEncounters(new FixedSequenceRandom(0, 1));
        encounters.RollStep(_forest);
        encounters.RollStep(_forest);

        var encounterId = encounters.RollStep(_forest);

        Assert.Equal("wolf_pack", encounterId);
    }

    [Fact]
    public void RollStep_RollNonZero_ReturnsNull()
    {
        var encounters = new RandomEncounters(new FixedSequenceRandom(24));
        encounters.RollStep(_forest);
        encounters.RollStep(_forest);

        var encounterId = encounters.RollStep(_forest);

        Assert.Null(encounterId);
    }

    [Fact]
    public void ResetSteps_AfterGrace_GraceAppliesAgain()
    {
        var encounters = new RandomEncounters(new FixedSequenceRandom(5));
        encounters.RollStep(_forest);
        encounters.RollStep(_forest);
        encounters.RollStep(_forest);

        encounters.ResetSteps();
        var rolls = new[] { encounters.RollStep(_forest), encounters.RollStep(_forest) };

        Assert.Equal([null, null], rolls);
    }
}
