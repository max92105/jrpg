using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// A record's next tier (decision 985): the id's stem and one more than the highest number of that stem.
public sealed class TierIdsTests
{
    [Fact]
    public void Next_Fire1_IsFire2()
    {
        var next = TierIds.Next("fire_1", ["fire_1", "heal_1"]);

        Assert.Equal("fire_2", next);
    }

    [Fact]
    public void Next_StunWithoutNumber_IsStun1()
    {
        var next = TierIds.Next("stun", ["stun"]);

        Assert.Equal("stun_1", next);
    }

    // Set up with the highest number between the chosen one and a lower one, and another stem's higher number.
    [Fact]
    public void Next_Fire2Taken_IsFire3()
    {
        var next = TierIds.Next("fire_1", ["fire_1", "fire_2", "fire", "heal_7"]);

        Assert.Equal("fire_3", next);
    }

    [Fact]
    public void Next_ChosenNotAmongTaken_FollowsTheChosen()
    {
        var next = TierIds.Next("fire_4", ["fire_1"]);

        Assert.Equal("fire_5", next);
    }

    [Fact]
    public void Next_HighestIsIntMax_CountsPastIt()
    {
        var next = TierIds.Next("fire_1", ["fire_1", $"fire_{int.MaxValue}"]);

        Assert.Equal("fire_2147483648", next);
    }

    // Too long for an int: not counted, so Duplicate gives an id to retype rather than throwing.
    [Fact]
    public void Next_NumberPastIntRange_IsNotCounted()
    {
        var next = TierIds.Next("fire_1", ["fire_1", "fire_99999999999"]);

        Assert.Equal("fire_2", next);
    }

    [Fact]
    public void Stem_IdEndingInLetters_IsTheId()
    {
        var stem = TierIds.Stem("fire_bolt");

        Assert.Equal("fire_bolt", stem);
    }

    [Fact]
    public void Stem_TwoNumbers_DropsTheLastOnly()
    {
        var stem = TierIds.Stem("fire_1_2");

        Assert.Equal("fire_1", stem);
    }
}
