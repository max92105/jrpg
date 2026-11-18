using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.NewGame;

/// <summary>
/// The new game's leader choices as the New game page types them (decision 968): the members in the order the leader
/// screen shows them, each added after the chosen row from the draft's members not already listed, removed and moved.
/// Remove isn't greyed at the last one: Data's line says a new game needs one. Every change of the list raises
/// <see cref="Problem"/>'s change once.
/// </summary>
internal sealed partial class NewGameViewModel
{
    /// <summary>
    /// The members the leader screen offers, in its order; a member the draft lacks is shown as missing. The rows
    /// compare by value (<see cref="RecordChoice"/> is a record), so the moves' <c>IndexOf</c> finds a row by its
    /// member: safe, since the loader refuses a repeated leader and the add box leaves the listed members out.
    /// </summary>
    public ObservableCollection<RecordChoice> Leaders { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveLeaderCommand), nameof(MoveLeaderUpCommand), nameof(MoveLeaderDownCommand))]
    public partial RecordChoice? SelectedLeader { get; set; }

    /// <summary>The add box's choice, added by Add; null while nothing is picked.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLeaderCommand))]
    public partial RecordChoice? LeaderToAdd { get; set; }

    /// <summary>
    /// The add box's entries: the draft's members by name, then id, the listed ones left out, so a member is listed
    /// once (Data's rule).
    /// </summary>
    public IReadOnlyList<RecordChoice> LeadersToAdd =>
        RecordChoice.ByName(
            _content.PartyMembers.Values
                .Where(member => !Leaders.Any(leader => leader.Id == member.Id))
                .Select(member => new RecordChoice(member.Id, member.Name)));

    private static ObservableCollection<RecordChoice> LeadersOf(NewGameDefinition newGame, ContentCatalog content) =>
    [
        .. newGame.LeaderChoices.Select(
            choice => RecordChoice.Of(choice.MemberId, content.PartyMembers.GetValueOrDefault(choice.MemberId)?.Name)),
    ];

    private IReadOnlyList<LeaderChoice> LeadersAsTyped() =>
        ListOrder.KeptIfEqual([.. Leaders.Select(leader => new LeaderChoice { MemberId = leader.Id })], _source.LeaderChoices);

    /// <summary>Puts the add box's member after the chosen row (last when none), and chooses it.</summary>
    [RelayCommand(CanExecute = nameof(HasLeaderToAdd))]
    private void AddLeader()
    {
        var leader = LeaderToAdd!;
        ListOrder.InsertAfter(Leaders, leader, SelectedLeader);
        SelectedLeader = leader;
        LeaderToAdd = null;
    }

    private bool HasLeaderToAdd() => LeaderToAdd is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedLeader))]
    private void RemoveLeader() => SelectedLeader = ListOrder.Remove(Leaders, SelectedLeader!);

    [RelayCommand(CanExecute = nameof(CanMoveLeaderUp))]
    private void MoveLeaderUp() => ListOrder.MoveBy(Leaders, SelectedLeader!, -1);

    [RelayCommand(CanExecute = nameof(CanMoveLeaderDown))]
    private void MoveLeaderDown() => ListOrder.MoveBy(Leaders, SelectedLeader!, 1);

    private bool HasSelectedLeader() => SelectedLeader is not null;

    private bool CanMoveLeaderUp() => ListOrder.CanMoveUp(Leaders, SelectedLeader);

    private bool CanMoveLeaderDown() => ListOrder.CanMoveDown(Leaders, SelectedLeader);

    private void OnLeadersChanged()
    {
        MoveLeaderUpCommand.NotifyCanExecuteChanged();
        MoveLeaderDownCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(LeadersToAdd));
        OnPropertyChanged(nameof(Problem));
    }
}
