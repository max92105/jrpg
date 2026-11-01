using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Quests;

namespace Jrpg.Editor.Quests;

/// <summary>
/// Every quest as the Quests window edits it (decision 394): the draft's quests in id order, with the quests added this
/// session after them, each editable through its <see cref="QuestViewModel"/>. <see cref="Problem"/> names the first
/// thing the game would refuse: an id two quests share, or a quest's own problem.
/// </summary>
internal sealed partial class QuestCatalogViewModel : ObservableObject
{
    /// <param name="quests">The draft's quests, in any order.</param>
    public QuestCatalogViewModel(IEnumerable<QuestDefinition> quests)
    {
        Quests = [.. quests.OrderBy(quest => quest.Id, StringComparer.Ordinal).Select(QuestViewModel.From)];
        foreach (var quest in Quests)
        {
            quest.PropertyChanged += OnQuestChanged;
        }

        SelectedQuest = Quests.FirstOrDefault();
    }

    public ObservableCollection<QuestViewModel> Quests { get; }

    [ObservableProperty]
    public partial QuestViewModel? SelectedQuest { get; set; }

    /// <summary>
    /// Why the game would refuse the quests as typed, naming the quest by id (by position while it has none); null when
    /// it wouldn't.
    /// </summary>
    public string? Problem
    {
        get
        {
            for (var index = 0; index < Quests.Count; index++)
            {
                var quest = Quests[index];
                if (Quests.Take(index).Any(earlier => earlier.Id == quest.Id))
                {
                    return $"Quest {quest.Id} is listed twice.";
                }

                if (quest.Problem is { } problem)
                {
                    return $"Quest {(quest.Id.Length > 0 ? quest.Id : $"#{index + 1}")}: {problem}";
                }
            }

            return null;
        }
    }

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the window applies only without one.
    /// </exception>
    public IReadOnlyList<QuestDefinition> ToQuests() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Quests.Select(quest => quest.ToQuest())];

    /// <summary>Adds a quest with nothing typed yet and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddQuest()
    {
        var quest = QuestViewModel.New();
        quest.PropertyChanged += OnQuestChanged;
        Quests.Add(quest);
        SelectedQuest = quest;
    }

    private void OnQuestChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Problem));
}
