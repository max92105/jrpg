using Jrpg.Core.Battle;

namespace Jrpg.Core.Tests.Battle;

// The shape table is what Data's number check and the target fit read, so a kind without a row would throw there.
public sealed class TacticConditionKindsTests
{
    [Fact]
    public void ShapeOf_EveryKind_HasAShape()
    {
        var kinds = Enum.GetValues<TacticConditionKind>();

        Assert.All(kinds, kind => Assert.NotNull(TacticConditionKinds.ShapeOf(kind)));
    }
}
