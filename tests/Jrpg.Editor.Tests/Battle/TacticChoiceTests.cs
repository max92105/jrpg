using Jrpg.Core.Battle;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The When and On boxes offer every trigger and target Core has, each once, in the enum's order (decision 850): a kind
// or target Core gains fails here until its slice gives it words.
public sealed class TacticChoiceTests
{
    [Fact]
    public void ConditionChoices_EveryCoreKind_IsOfferedOnce()
    {
        TacticConditionKind?[] kinds = [null, .. Enum.GetValues<TacticConditionKind>().Cast<TacticConditionKind?>()];

        var offered = TacticConditionChoice.All.Select(choice => choice.Kind);

        Assert.Equal(kinds, offered);
    }

    [Fact]
    public void TargetChoices_EveryCoreTarget_IsOfferedOnce()
    {
        TacticTarget?[] targets = [null, .. Enum.GetValues<TacticTarget>().Cast<TacticTarget?>()];

        var offered = TacticTargetChoice.All.Select(choice => choice.Target);

        Assert.Equal(targets, offered);
    }
}
