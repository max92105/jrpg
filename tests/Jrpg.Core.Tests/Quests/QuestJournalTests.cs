using Jrpg.Core.Quests;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Quests;

// The journal's order is Core's (decisions 390, 391): main before side, newest first in each; a completed quest leaves
// it.
public sealed class QuestJournalTests
{
    private static readonly QuestDefinition _errand = TestQuests.Errand;
    private static readonly QuestDefinition _chores = _errand with { Id = "chores", Title = "Chores" };
    private static readonly QuestDefinition _heist = _errand with { Id = "heist", Title = "A heist", Kind = QuestKind.Main };

    private readonly QuestLog _log = new();

    // The main quest is started first, so only the kind order, not newest-first, can put it ahead.
    [Fact]
    public void Active_MainAndSideQuests_MainFirst()
    {
        _log.Start(_heist);
        _log.Start(_errand);

        var active = new QuestJournal(_log).Active;

        Assert.Equal(["heist", "errand"], active.Select(entry => entry.Quest.Id));
    }

    [Fact]
    public void Active_TwoSideQuests_NewestFirst()
    {
        _log.Start(_errand);
        _log.Start(_chores);

        Assert.Equal(["chores", "errand"], new QuestJournal(_log).Active.Select(entry => entry.Quest.Id));
    }

    [Fact]
    public void Active_CompletedQuest_NotListed()
    {
        _log.Start(_errand);
        _log.Start(_chores);
        _log.Complete(_errand);

        Assert.Equal(["chores"], new QuestJournal(_log).Active.Select(entry => entry.Quest.Id));
    }

    [Fact]
    public void Active_AdvancedQuest_EntryAtTheCurrentStage()
    {
        _log.AdvanceTo(_errand, "deliver");

        var entry = Assert.Single(new QuestJournal(_log).Active);

        Assert.Equal(new QuestEntry(_errand, _errand.Stage("deliver")), entry);
    }

    [Fact]
    public void Completed_TwoQuests_OldestFirst()
    {
        _log.Start(_errand);
        _log.Start(_chores);
        _log.Complete(_chores);
        _log.Complete(_errand);

        Assert.Equal(["errand", "chores"], new QuestJournal(_log).Completed.Select(entry => entry.Quest.Id));
        Assert.Empty(new QuestJournal(_log).Active);
    }

    // The side quest is completed first, so only the kind order can put the main quest ahead; each stays at the stage
    // it was completed at.
    [Fact]
    public void Completed_MainAndSide_MainFirstAtTheirStages()
    {
        _log.AdvanceTo(_errand, "deliver");
        _log.Complete(_errand);
        _log.Complete(_heist);

        var completed = new QuestJournal(_log).Completed;

        Assert.Equal([new QuestEntry(_heist, _heist.LastStage), new QuestEntry(_errand, _errand.Stage("deliver"))], completed);
    }

    [Fact]
    public void Active_NothingStarted_IsEmpty()
    {
        Assert.Empty(new QuestJournal(_log).Active);
    }
}
