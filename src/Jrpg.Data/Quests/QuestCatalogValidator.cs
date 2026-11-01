using Jrpg.Core.Quests;
using Jrpg.Data.Content;

namespace Jrpg.Data.Quests;

/// <summary>
/// The rules for <c>quests.json</c> (ADR 0032): a title and a description, at least one stage, no null stage, stage ids
/// not empty and not repeated within the quest, and a text on every stage. Problems name the quest and the stage.
/// </summary>
internal static class QuestCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<QuestDefinition> quests) =>
        new ContentListValidator<QuestDefinition>(fileName, "quest", quest => quest.Id).Validate(quests, FindProblem);

    private static string? FindProblem(QuestDefinition quest)
    {
        if (string.IsNullOrWhiteSpace(quest.Title))
        {
            return "title must not be empty.";
        }

        if (string.IsNullOrWhiteSpace(quest.Description))
        {
            return "description must not be empty.";
        }

        if (quest.Stages is not { Count: > 0 })
        {
            return "stages must list at least one stage.";
        }

        var seenIds = new HashSet<string>();
        for (var index = 0; index < quest.Stages.Count; index++)
        {
            if (FindStageProblem(quest.Stages[index], seenIds) is { } problem)
            {
                return $"stages[{index}] {problem}";
            }
        }

        return null;
    }

    // The serializer doesn't apply nullable annotations to list elements, so a stray null gets here.
    private static string? FindStageProblem(QuestStage? stage, HashSet<string> seenIds) =>
        stage is null ? "is null."
        : string.IsNullOrWhiteSpace(stage.Id) ? "has an empty id."
        : !seenIds.Add(stage.Id) ? $"repeats stage id '{stage.Id}'."
        : string.IsNullOrWhiteSpace(stage.Text) ? $"'{stage.Id}' has no text."
        : null;
}
