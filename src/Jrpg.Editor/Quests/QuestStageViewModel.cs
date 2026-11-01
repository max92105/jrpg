using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Quests;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Quests;

/// <summary>
/// One stage as the Quests window edits it (decision 394): its id is typed while the stage is new and fixed once the
/// draft holds it, since events and saves name it; its text is the objective the Quests page shows.
/// </summary>
internal sealed partial class QuestStageViewModel : ObservableObject
{
    private QuestStageViewModel(string id, string text)
    {
        Id = id;
        Text = text;
    }

    public static QuestStageViewModel From(QuestStage stage) => new(stage.Id, stage.Text);

    public static QuestStageViewModel New() => new("", "") { IsNew = true };

    /// <summary>
    /// True while the stage was added this session: the only time its id can be typed or the stage removed.
    /// </summary>
    public bool IsNew { get; private init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string Text { get; set; }

    /// <summary>The list's line: the id, or a placeholder while a new stage has none.</summary>
    public string Label => Id.Length > 0 ? Id : "(new stage)";

    /// <summary>
    /// Why the game would refuse the stage as typed (the id rule, an empty text); null when it wouldn't. An id another
    /// stage has is the quest's problem, which sees them all.
    /// </summary>
    public string? Problem => ObjectIds.ProblemOf(Id, []) ?? (string.IsNullOrWhiteSpace(Text) ? "Write the stage's text." : null);

    public QuestStage ToStage() => new(Id, Text.Trim());
}
