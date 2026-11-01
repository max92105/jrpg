using Jrpg.Core.Quests;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Quests;

// Decision 388's rules for the three quest steps in every state: not started, active, completed.
public sealed class QuestLogTests
{
    private static readonly QuestDefinition _errand = TestQuests.Errand;

    private readonly QuestLog _log = new();

    [Fact]
    public void StageOf_NeverStarted_IsNull()
    {
        Assert.Null(_log.StageOf("errand"));
        Assert.False(_log.IsCompleted("errand"));
    }

    [Fact]
    public void Start_NotStarted_ActiveAtFirstStage()
    {
        _log.Start(_errand);

        Assert.Equal("take", _log.StageOf("errand"));
        Assert.False(_log.IsCompleted("errand"));
    }

    [Fact]
    public void Start_Active_KeepsItsStage()
    {
        _log.AdvanceTo(_errand, "deliver");

        _log.Start(_errand);

        Assert.Equal("deliver", _log.StageOf("errand"));
    }

    [Fact]
    public void Start_Completed_StaysCompleted()
    {
        _log.Complete(_errand);

        _log.Start(_errand);

        Assert.True(_log.IsCompleted("errand"));
    }

    [Fact]
    public void AdvanceTo_Active_MovesToTheStage()
    {
        _log.Start(_errand);

        _log.AdvanceTo(_errand, "deliver");

        Assert.Equal("deliver", _log.StageOf("errand"));
        Assert.False(_log.IsCompleted("errand"));
    }

    [Fact]
    public void AdvanceTo_EarlierStage_MovesBack()
    {
        _log.AdvanceTo(_errand, "deliver");

        _log.AdvanceTo(_errand, "take");

        Assert.Equal("take", _log.StageOf("errand"));
    }

    [Fact]
    public void AdvanceTo_NotStarted_StartsAtTheStage()
    {
        _log.AdvanceTo(_errand, "deliver");

        Assert.Equal("deliver", _log.StageOf("errand"));
        Assert.False(_log.IsCompleted("errand"));
    }

    [Fact]
    public void AdvanceTo_Completed_StaysCompletedAtItsStage()
    {
        _log.AdvanceTo(_errand, "deliver");
        _log.Complete(_errand);

        _log.AdvanceTo(_errand, "take");

        Assert.True(_log.IsCompleted("errand"));
        Assert.Equal("deliver", _log.StageOf("errand"));
    }

    // The stage is resolved before the completed check, so a bug naming an unknown stage surfaces in every state.
    [Fact]
    public void AdvanceTo_UnknownStage_ThrowsInEveryState()
    {
        Assert.Throws<ArgumentException>(() => _log.AdvanceTo(_errand, "burn"));
        Assert.Null(_log.StageOf("errand"));

        _log.Complete(_errand);

        Assert.Throws<ArgumentException>(() => _log.AdvanceTo(_errand, "burn"));
        Assert.True(_log.IsCompleted("errand"));
    }

    [Fact]
    public void Complete_Active_CompletedAtItsStage()
    {
        _log.Start(_errand);

        _log.Complete(_errand);

        Assert.True(_log.IsCompleted("errand"));
        Assert.Equal("take", _log.StageOf("errand"));
    }

    [Fact]
    public void Complete_NotStarted_CompletedAtLastStage()
    {
        _log.Complete(_errand);

        Assert.True(_log.IsCompleted("errand"));
        Assert.Equal("deliver", _log.StageOf("errand"));
    }

    [Fact]
    public void Complete_Completed_StaysCompleted()
    {
        _log.Start(_errand);
        _log.Complete(_errand);

        _log.Complete(_errand);

        Assert.True(_log.IsCompleted("errand"));
        Assert.Equal("take", _log.StageOf("errand"));
    }
}
