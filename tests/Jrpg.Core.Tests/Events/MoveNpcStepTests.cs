using Jrpg.Core.Events;
using Jrpg.Core.Geometry;

namespace Jrpg.Core.Tests.Events;

// A record compares its list by reference; the step compares by the path's content, so the editor's comparison and the
// tests see two steps with the same path as one step.
public sealed class MoveNpcStepTests
{
    [Fact]
    public void Equals_SamePathInSeparateLists_IsTrue()
    {
        var one = new MoveNpcStep("npc_1", [Facing.Left, Facing.Down]);
        var other = new MoveNpcStep("npc_1", new List<Facing> { Facing.Left, Facing.Down });

        Assert.Equal(one, other);
        Assert.Equal(one.GetHashCode(), other.GetHashCode());
    }

    [Fact]
    public void Equals_OtherPath_IsFalse()
    {
        var one = new MoveNpcStep("npc_1", [Facing.Left, Facing.Down]);

        Assert.NotEqual(one, new MoveNpcStep("npc_1", [Facing.Down, Facing.Left]));
        Assert.NotEqual(one, new MoveNpcStep("npc_2", [Facing.Left, Facing.Down]));
    }
}
