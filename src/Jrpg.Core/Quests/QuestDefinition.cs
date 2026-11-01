namespace Jrpg.Core.Quests;

/// <summary>
/// One quest as content describes it (ADR 0032): its title and description, whether it is a main or a side quest, and
/// its stages in order. Stages are named by id everywhere (steps, conditions, saves); only "reached" reads a stage's
/// position in this order.
/// </summary>
/// <param name="Id">The quest's id, which the quest steps and conditions name.</param>
/// <param name="Title">The quest's name, as the page shows it.</param>
/// <param name="Description">What the quest is about, shown under the title.</param>
/// <param name="Kind">Main or side.</param>
/// <param name="Stages">
/// Its stages in order, at least one; a quest starts at the first and completes at the last.
/// </param>
public sealed record QuestDefinition(string Id, string Title, string Description, QuestKind Kind, IReadOnlyList<QuestStage> Stages)
{
    public QuestStage FirstStage => Stages[0];

    public QuestStage LastStage => Stages[^1];

    public bool HasStage(string stageId) => Stages.Any(stage => stage.Id == stageId);

    /// <exception cref="ArgumentException">
    /// No stage has that id: content is validated, so only a bug gets here.
    /// </exception>
    public QuestStage Stage(string stageId) =>
        Stages.FirstOrDefault(stage => stage.Id == stageId)
        ?? throw new ArgumentException($"Quest '{Id}' has no stage '{stageId}'.", nameof(stageId));

    // A record compares a list by reference; two quests read from the same content are one quest.
    public bool Equals(QuestDefinition? other) =>
        other is not null
        && (Id, Title, Description, Kind) == (other.Id, other.Title, other.Description, other.Kind)
        && Stages.SequenceEqual(other.Stages);

    public override int GetHashCode() => HashCode.Combine(Id, Title, Description, Kind, Stages.Count);
}
