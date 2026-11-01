using Jrpg.Core.Quests;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Quests;

// The five condition kinds against a log in every state (decision 389); "reached" reads the stage order.
public sealed class QuestConditionTests
{
    private static readonly QuestDefinition _errand = TestQuests.Errand;
    private readonly QuestLog _log = new();

    [Fact]
    public void IsMetBy_NotStartedBeforeStart_Met()
    {
        Assert.True(new QuestCondition("errand", QuestConditionKind.NotStarted).IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_NotStartedAfterStart_NotMet()
    {
        _log.Start(_errand);

        Assert.False(new QuestCondition("errand", QuestConditionKind.NotStarted).IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_ActiveAtAnyStage_Met()
    {
        var active = new QuestCondition("errand", QuestConditionKind.Active);
        Assert.False(active.IsMetBy(_log));

        _log.AdvanceTo(_errand, "deliver");

        Assert.True(active.IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_ActiveOnceCompleted_NotMet()
    {
        _log.Start(_errand);
        _log.Complete(_errand);

        Assert.False(new QuestCondition("errand", QuestConditionKind.Active).IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_AtStage_MetAtItOnly()
    {
        var atTake = new QuestCondition("errand", QuestConditionKind.AtStage, "take");
        _log.Start(_errand);
        Assert.True(atTake.IsMetBy(_log));

        _log.AdvanceTo(_errand, "deliver");

        Assert.False(atTake.IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_AtStageOnceCompletedThere_NotMet()
    {
        _log.Start(_errand);
        _log.Complete(_errand);

        Assert.False(new QuestCondition("errand", QuestConditionKind.AtStage, "take").IsMetBy(_log));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("take", false)]
    [InlineData("deliver", true)]
    public void IsMetBy_ReachedDeliver_MetAtOrPastIt(string? stageId, bool met)
    {
        if (stageId is not null)
        {
            _log.AdvanceTo(_errand, stageId);
        }

        Assert.Equal(met, new QuestCondition("errand", QuestConditionKind.ReachedStage, "deliver").IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_ReachedTakeWhileAtDeliverOrCompleted_Met()
    {
        var reachedTake = new QuestCondition("errand", QuestConditionKind.ReachedStage, "take");
        _log.AdvanceTo(_errand, "deliver");
        Assert.True(reachedTake.IsMetBy(_log));

        _log.Complete(_errand);

        Assert.True(reachedTake.IsMetBy(_log));
    }

    // Decision 389's flagged reading: moving back un-reaches the later stages.
    [Fact]
    public void IsMetBy_ReachedStageAfterAdvanceBack_NotMet()
    {
        _log.AdvanceTo(_errand, "deliver");
        _log.AdvanceTo(_errand, "take");

        Assert.False(new QuestCondition("errand", QuestConditionKind.ReachedStage, "deliver").IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_ReachedUnknownStage_Throws()
    {
        _log.Start(_errand);

        Assert.Throws<ArgumentException>(() => new QuestCondition("errand", QuestConditionKind.ReachedStage, "burn").IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_AtStageWithoutStage_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new QuestCondition("errand", QuestConditionKind.AtStage).IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_ReachedWithoutStage_Throws()
    {
        _log.Start(_errand);

        Assert.Throws<InvalidOperationException>(() => new QuestCondition("errand", QuestConditionKind.ReachedStage).IsMetBy(_log));
    }

    [Fact]
    public void IsMetBy_Completed_MetOnlyOnceCompleted()
    {
        var completed = new QuestCondition("errand", QuestConditionKind.Completed);
        _log.Start(_errand);
        Assert.False(completed.IsMetBy(_log));

        _log.Complete(_errand);

        Assert.True(completed.IsMetBy(_log));
    }
}
