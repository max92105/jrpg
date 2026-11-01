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
    public void Progress_TwoQuestsStarted_OldestFirst()
    {
        var heist = _errand with { Id = "heist" };
        _log.Start(heist);
        _log.AdvanceTo(_errand, "deliver");
        _log.Complete(heist);

        Assert.Equal([new QuestProgress("heist", "take", IsCompleted: true), new QuestProgress("errand", "deliver", IsCompleted: false)], _log.Progress);
    }

    [Fact]
    public void Restore_FromProgress_SameStagesCompletionAndOrder()
    {
        var heist = _errand with { Id = "heist" };
        IReadOnlyList<QuestProgress> saved = [new QuestProgress("heist", "deliver", IsCompleted: true), new QuestProgress("errand", "take", IsCompleted: false)];

        var restored = QuestLog.Restore(saved, new Dictionary<string, QuestDefinition> { ["errand"] = _errand, ["heist"] = heist });

        Assert.Equal(saved, restored.Progress);
        Assert.True(restored.IsCompleted("heist"));
        Assert.True(restored.HasReached("heist", "deliver"));
        Assert.Equal("take", restored.StageOf("errand"));
    }

    [Fact]
    public void Start_NotStarted_ActiveAtFirstStage()
    {
        var change = _log.Start(_errand);

        Assert.Equal(new QuestChange(_errand, QuestChangeKind.Started), change);
        Assert.Equal("take", _log.StageOf("errand"));
        Assert.False(_log.IsCompleted("errand"));
    }

    [Fact]
    public void Start_Active_KeepsItsStage()
    {
        _log.AdvanceTo(_errand, "deliver");

        var change = _log.Start(_errand);

        Assert.Null(change);
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

        var change = _log.AdvanceTo(_errand, "deliver");

        Assert.Equal(new QuestChange(_errand, QuestChangeKind.Advanced), change);
        Assert.Null(_log.AdvanceTo(_errand, "deliver"));

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
        var change = _log.AdvanceTo(_errand, "deliver");

        Assert.Equal(new QuestChange(_errand, QuestChangeKind.Started), change);
        Assert.Equal("deliver", _log.StageOf("errand"));
        Assert.False(_log.IsCompleted("errand"));
    }

    [Fact]
    public void AdvanceTo_Completed_StaysCompletedAtItsStage()
    {
        _log.AdvanceTo(_errand, "deliver");
        _log.Complete(_errand);

        Assert.Null(_log.AdvanceTo(_errand, "take"));

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

        var change = _log.Complete(_errand);

        Assert.Equal(new QuestChange(_errand, QuestChangeKind.Completed), change);

        Assert.True(_log.IsCompleted("errand"));
        Assert.Equal("take", _log.StageOf("errand"));
    }

    [Fact]
    public void Complete_NotStarted_CompletedAtLastStage()
    {
        var change = _log.Complete(_errand);

        Assert.Equal(QuestChangeKind.Completed, change?.Kind);

        Assert.True(_log.IsCompleted("errand"));
        Assert.Equal("deliver", _log.StageOf("errand"));
    }

    [Fact]
    public void Complete_Completed_StaysCompleted()
    {
        _log.Start(_errand);
        _log.Complete(_errand);

        var again = _log.Complete(_errand);

        Assert.Null(again);

        Assert.True(_log.IsCompleted("errand"));
        Assert.Equal("take", _log.StageOf("errand"));
    }
}
