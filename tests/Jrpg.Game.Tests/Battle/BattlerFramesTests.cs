using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Battle;

// Which frame each battler shows (decisions 634, 635): Sarxian, still, then two gnolls looping four frames of 100 ms,
// the test world's goblin given a loop.
public sealed class BattlerFramesTests
{
    private static readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(100);

    private readonly Combatant[] _combatants;
    private readonly BattlerFrames _frames;

    public BattlerFramesTests()
    {
        var loop = new TileAnimation { FrameCount = 4, FrameMilliseconds = 100 };
        var gnoll = StartMapScenarios.TestWorld.Enemies["goblin"] with { Id = "gnoll", IdleAnimation = loop };
        _combatants =
        [
            Combatant.FromPartyMember(BattleScenarios.Recruit("sarxian")),
            Combatant.FromEnemy(gnoll),
            Combatant.FromEnemy(gnoll),
        ];
        _frames = new BattlerFrames(_combatants, [.. _combatants.Select(BattlerArt.Of)]);
    }

    [Fact]
    public void FrameOf_StillBattler_IsZero()
    {
        _frames.Advance(_frameTime * 3);

        Assert.Equal(0, _frames.FrameOf(0));
    }

    [Fact]
    public void FrameOf_AnimatedAtStart_IsZero()
    {
        Assert.Equal(0, _frames.FrameOf(1));
    }

    [Fact]
    public void FrameOf_AnimatedAfterOneFrameTime_IsOne()
    {
        _frames.Advance(_frameTime);

        Assert.Equal(1, _frames.FrameOf(1));
    }

    [Fact]
    public void FrameOf_AfterTheLastFrame_LoopsToZero()
    {
        _frames.Advance(_frameTime * 4);

        Assert.Equal(0, _frames.FrameOf(1));
    }

    [Fact]
    public void FrameOf_SecondAnimatedEnemy_IsOneFrameAhead()
    {
        _frames.Advance(_frameTime);

        Assert.Equal(_frames.FrameOf(1) + 1, _frames.FrameOf(2));
    }

    [Fact]
    public void FrameOf_FallenEnemy_HoldsItsFrame()
    {
        _frames.Advance(_frameTime * 2);
        _combatants[1].Vitals.TakeDamage(9999);

        _frames.Advance(_frameTime);
        _frames.Advance(_frameTime * 2);

        Assert.Equal(2, _frames.FrameOf(1));
    }

    // Fallen at frame 2, held a frame time, revived: four frame times in, it shows the loop's frame 0 again.
    [Fact]
    public void FrameOf_RevivedEnemy_LoopsAgain()
    {
        _frames.Advance(_frameTime * 2);
        _combatants[1].Vitals.TakeDamage(9999);
        _frames.Advance(_frameTime);
        _combatants[1].Vitals.Revive(1);

        _frames.Advance(_frameTime);

        Assert.Equal(0, _frames.FrameOf(1));
    }

    [Fact]
    public void FrameOf_LivingBesideTheFallen_KeepsLooping()
    {
        _combatants[1].Vitals.TakeDamage(9999);
        _frames.Advance(_frameTime);

        _frames.Advance(_frameTime);

        Assert.Equal(3, _frames.FrameOf(2));
    }

    [Fact]
    public void Advance_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _frames.Advance(-TimeSpan.FromTicks(1)));
    }
}
