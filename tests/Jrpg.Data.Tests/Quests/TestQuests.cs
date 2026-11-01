using Jrpg.Core.Quests;

namespace Jrpg.Data.Tests.Quests;

/// <summary>The quests the tests run: a two-stage side errand.</summary>
internal static class TestQuests
{
    public static QuestDefinition Errand { get; } = new(
        "errand",
        "An errand",
        "Carry a letter.",
        QuestKind.Side,
        [new QuestStage("take", "Take the letter."), new QuestStage("deliver", "Deliver it.")]);
}
