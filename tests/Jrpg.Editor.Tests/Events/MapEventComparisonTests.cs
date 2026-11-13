using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// Whether the dialog changed anything: a move step compares by its NPC and the content of its path, never by the
// reference of the list holding it.
public sealed class MapEventComparisonTests
{
    [Fact]
    public void AreSame_SameMovePathsInSeparateLists_IsTrue()
    {
        var before = Talk(new MoveNpcStep("npc_3", [Facing.Left, Facing.Down]));
        var after = Talk(new MoveNpcStep("npc_3", new List<Facing> { Facing.Left, Facing.Down }));

        Assert.True(MapEventComparison.AreSame([before], [after]));
    }

    [Fact]
    public void AreSame_MovePathsDiffer_IsFalse()
    {
        var before = Talk(new MoveNpcStep("npc_3", [Facing.Left, Facing.Down]));
        var after = Talk(new MoveNpcStep("npc_3", [Facing.Left]));

        Assert.False(MapEventComparison.AreSame([before], [after]));
    }

    private static MapEvent Talk(params EventStep[] steps) =>
        new() { Id = "npc_3_0", Activation = EventActivation.Interact, Prompt = "Talk", Steps = steps };
}
