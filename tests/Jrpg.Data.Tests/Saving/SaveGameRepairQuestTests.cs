using Jrpg.Core.Quests;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

// Saved quest progress repaired against the repository's content (decision 396): a quest content no longer has is
// forgotten, a stage it no longer has goes back to the first, a quest listed twice keeps its first entry; each is said.
public sealed class SaveGameRepairQuestTests
{
    private static readonly QuestProgress _errandAtReturn = new("placeholder_errand", "return_to_giver", IsCompleted: false);

    [Fact]
    public void Repair_KnownQuests_Unchanged()
    {
        var repaired = Repair([_errandAtReturn]);

        Assert.Equal([_errandAtReturn], repaired.Save.Quests);
        Assert.Empty(repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownQuest_ForgetsItAndSaysSo()
    {
        var repaired = Repair([new QuestProgress("heist", "case", IsCompleted: true), _errandAtReturn]);

        Assert.Equal([_errandAtReturn], repaired.Save.Quests);
        Assert.Equal(["forgot quest 'heist': it no longer exists."], repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownStage_BackToFirstStageAndSaysSo()
    {
        var repaired = Repair([_errandAtReturn with { StageId = "find_the_mill" }]);

        Assert.Equal([_errandAtReturn with { StageId = "ask_innkeeper" }], repaired.Save.Quests);
        Assert.Equal(["quest 'placeholder_errand': back to its first stage 'ask_innkeeper', since stage 'find_the_mill' no longer exists."], repaired.Changes);
    }

    // A finished quest at a vanished stage stays finished: the journal still lists it as done.
    [Fact]
    public void Repair_CompletedQuestAtUnknownStage_BackToFirstStageStillCompleted()
    {
        var repaired = Repair([_errandAtReturn with { StageId = "find_the_mill", IsCompleted = true }]);

        Assert.Equal([_errandAtReturn with { StageId = "ask_innkeeper", IsCompleted = true }], repaired.Save.Quests);
        Assert.Single(repaired.Changes);
    }

    [Fact]
    public void Repair_QuestListedTwice_KeepsTheFirst()
    {
        var repaired = Repair([_errandAtReturn, _errandAtReturn with { IsCompleted = true }]);

        Assert.Equal([_errandAtReturn], repaired.Save.Quests);
        Assert.Equal(["quest 'placeholder_errand': kept its first entry and dropped a repeated one."], repaired.Changes);
    }

    private static RepairedSave Repair(IReadOnlyList<QuestProgress> quests) =>
        SaveGameRepair.Repair("slot_01", RepairableSave with { Quests = quests }, Catalog);
}
