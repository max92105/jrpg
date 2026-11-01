using Jrpg.Core.Quests;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The condition as three boxes (design: the editor's condition): a quest, a kind, and a stage for the two stage kinds.
public sealed class QuestGateViewModelTests
{
    private static readonly IReadOnlyList<QuestStageChoice> _stages =
    [
        new QuestStageChoice("errand", "take"),
        new QuestStageChoice("errand", "deliver"),
        new QuestStageChoice("heist", "case"),
    ];

    [Fact]
    public void New_FromCondition_ShowsQuestKindAndStage()
    {
        var gate = new QuestGateViewModel(new QuestCondition("errand", QuestConditionKind.AtStage, "deliver"), _stages);

        Assert.Equal("errand", gate.QuestId);
        Assert.Equal(QuestConditionKind.AtStage, gate.Kind);
        Assert.Equal("deliver", gate.StageId);
        Assert.Null(gate.Problem);
    }

    [Fact]
    public void Stages_QuestChosen_ListsItsStagesInOrder()
    {
        var gate = new QuestGateViewModel(null, _stages);
        Assert.Equal([""], gate.Stages);

        gate.QuestId = "errand";

        Assert.Equal(["", "take", "deliver"], gate.Stages);
    }

    [Fact]
    public void QuestId_Changed_ClearsTheStage()
    {
        var gate = new QuestGateViewModel(new QuestCondition("errand", QuestConditionKind.AtStage, "deliver"), _stages);

        gate.QuestId = "heist";

        Assert.Equal("", gate.StageId);
        Assert.Equal("Choose the stage the quest must be at.", gate.Problem);
        Assert.Equal(" when heist is at ?", gate.Label);
    }

    [Fact]
    public void Problem_ReachedStageWithoutStage_AsksForIt()
    {
        var gate = new QuestGateViewModel(null, _stages) { QuestId = "errand", Kind = QuestConditionKind.ReachedStage };

        Assert.Equal("Choose the stage the quest must have reached.", gate.Problem);
        Assert.Throws<InvalidOperationException>(gate.ToCondition);
    }

    [Fact]
    public void ToCondition_NoQuest_ReturnsNull()
    {
        var gate = new QuestGateViewModel(null, _stages) { Kind = QuestConditionKind.ReachedStage };

        Assert.Null(gate.Problem);
        Assert.Null(gate.ToCondition());
        Assert.Equal("", gate.Label);
    }

    [Fact]
    public void ToCondition_AtStage_ReturnsKindAndStage()
    {
        var gate = new QuestGateViewModel(null, _stages) { QuestId = "errand", Kind = QuestConditionKind.AtStage, StageId = "take" };

        Assert.Equal(new QuestCondition("errand", QuestConditionKind.AtStage, "take"), gate.ToCondition());
        Assert.Equal(" when errand is at take", gate.Label);
    }

    [Fact]
    public void ToCondition_Completed_LeavesTheStageOut()
    {
        var gate = new QuestGateViewModel(new QuestCondition("errand", QuestConditionKind.ReachedStage, "deliver"), _stages);

        gate.Kind = QuestConditionKind.Completed;

        Assert.Equal(new QuestCondition("errand", QuestConditionKind.Completed), gate.ToCondition());
        Assert.Equal(" when errand is completed", gate.Label);
    }
}
