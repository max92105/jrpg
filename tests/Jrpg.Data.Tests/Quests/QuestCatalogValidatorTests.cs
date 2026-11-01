using Jrpg.Core.Quests;
using Jrpg.Data.Content;
using Jrpg.Data.Quests;

namespace Jrpg.Data.Tests.Quests;

// quests.json's rules (ADR 0032): every problem names the quest, and a stage's problem names the stage too.
public sealed class QuestCatalogValidatorTests
{
    private const string FileName = "quests.json";

    private static readonly QuestDefinition _errand = TestQuests.Errand;

    [Fact]
    public void Validate_WellFormedQuest_Passes()
    {
        Assert.Null(Record.Exception(() => QuestCatalogValidator.Validate(FileName, [_errand])));
    }

    [Fact]
    public void Validate_RepeatedQuestId_ThrowsNamingIt()
    {
        var exception = Assert.Throws<ContentValidationException>(() => QuestCatalogValidator.Validate(FileName, [_errand, _errand with { Title = "Again" }]));

        Assert.Equal("quests.json: quest id 'errand' is used more than once.", exception.Message);
    }

    public static TheoryData<QuestDefinition, string> InvalidQuests => new()
    {
        { _errand with { Stages = [] }, "stages must list at least one stage." },
        { _errand with { Stages = null! }, "stages must list at least one stage." },
        { _errand with { Stages = [_errand.Stages[0], null!] }, "stages[1] is null." },
        { _errand with { Stages = [_errand.Stages[0], new QuestStage("take", "Again.")] }, "stages[1] repeats stage id 'take'." },
        { _errand with { Stages = [_errand.Stages[0], new QuestStage(" ", "No id.")] }, "stages[1] has an empty id." },
        { _errand with { Stages = [_errand.Stages[0], new QuestStage("deliver", " ")] }, "stages[1] 'deliver' has no text." },
        { _errand with { Title = " " }, "title must not be empty." },
        { _errand with { Description = "" }, "description must not be empty." },
    };

    [Theory]
    [MemberData(nameof(InvalidQuests))]
    public void Validate_InvalidQuest_ThrowsNamingTheQuestAndTheProblem(QuestDefinition quest, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => QuestCatalogValidator.Validate(FileName, [quest]));

        Assert.Equal($"quests.json: quest 'errand': {problem}", exception.Message);
    }
}
