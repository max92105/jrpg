using Jrpg.Core.Quests;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Quests;

public sealed class QuestDefinitionTests
{
    private static readonly QuestDefinition _errand = TestQuests.Errand;

    [Fact]
    public void Stage_KnownStageId_ReturnsIt()
    {
        Assert.Equal(new QuestStage("deliver", "Deliver it."), _errand.Stage("deliver"));
    }

    [Fact]
    public void Stage_UnknownStageId_ThrowsNamingBothIds()
    {
        var exception = Assert.Throws<ArgumentException>(() => _errand.Stage("burn"));

        Assert.StartsWith("Quest 'errand' has no stage 'burn'.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HasStage_KnownAndUnknownIds_TellsThemApart()
    {
        Assert.True(_errand.HasStage("take"));
        Assert.False(_errand.HasStage("burn"));
    }

    [Fact]
    public void FirstAndLastStage_TwoStages_AreTheEnds()
    {
        Assert.Equal("take", _errand.FirstStage.Id);
        Assert.Equal("deliver", _errand.LastStage.Id);
    }

    // The record would compare its stages by reference: two quests read from the same content are one quest.
    [Fact]
    public void Equals_SameContentReadTwice_IsEqual()
    {
        var again = _errand with { Stages = [.. _errand.Stages.Select(stage => stage with { })] };

        Assert.Equal(_errand, again);
        Assert.NotEqual(_errand, _errand with { Stages = [_errand.Stages[0]] });
    }
}
