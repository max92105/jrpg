using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.NewGame;
using static Jrpg.Editor.Tests.NewGame.NewGameFixtures;

namespace Jrpg.Editor.Tests.NewGame;

// The leader choices as the New game page types them (decision 968): in the leader screen's order, added after the
// chosen row from the members not listed, removed and moved, each change reaching the problem line.
public sealed class NewGameLeaderListTests
{
    private readonly NewGameViewModel _newGame = new(World);

    [Fact]
    public void Constructor_NewGame_ShowsLeadersInTheirOrder()
    {
        Assert.Equal(["vela", "ilsa", "brom"], _newGame.Leaders.Select(leader => leader.Id));
    }

    [Fact]
    public void Constructor_NewGame_ChoosesTheFirstLeader()
    {
        Assert.Equal("vela", _newGame.SelectedLeader?.Id);
    }

    [Fact]
    public void Constructor_LeaderTheDraftLacks_ShowsItMissing()
    {
        var newGame = new NewGameViewModel(
            World with { NewGame = TestNewGame with { LeaderChoices = [new() { MemberId = "ghost" }] } });

        Assert.Equal(RecordChoice.Missing("ghost"), Assert.Single(newGame.Leaders));
    }

    [Fact]
    public void Problem_LeaderTheDraftLacks_ShowsDataMessage()
    {
        var newGame = new NewGameViewModel(
            World with { NewGame = TestNewGame with { LeaderChoices = [new() { MemberId = "ghost" }] } });

        Assert.Contains("leaderChoices has unknown party member id 'ghost'.", newGame.Problem);
    }

    [Fact]
    public void ToNewGame_RowsMatchingTheRecord_KeepsItsList()
    {
        Assert.Same(TestNewGame.LeaderChoices, _newGame.ToNewGame().LeaderChoices);
    }

    [Fact]
    public void ToNewGame_LeaderMovedUp_WritesTheNewOrder()
    {
        _newGame.SelectedLeader = _newGame.Leaders[2];

        _newGame.MoveLeaderUpCommand.Execute(null);

        Assert.Equal(["vela", "brom", "ilsa"], _newGame.ToNewGame().LeaderChoices.Select(choice => choice.MemberId));
    }

    [Fact]
    public void ToNewGame_LeaderMovedDown_WritesTheNewOrder()
    {
        _newGame.MoveLeaderDownCommand.Execute(null);

        Assert.Equal(["ilsa", "vela", "brom"], _newGame.ToNewGame().LeaderChoices.Select(choice => choice.MemberId));
    }

    // Zora's id, abel, sorts first; her name last.
    [Fact]
    public void LeadersToAdd_LeavesListedMembersOut()
    {
        Assert.Equal(["cato", "abel"], _newGame.LeadersToAdd.Select(member => member.Id));
    }

    [Fact]
    public void AddLeader_WithSelection_InsertsAfterIt()
    {
        _newGame.LeaderToAdd = _newGame.LeadersToAdd[0];

        _newGame.AddLeaderCommand.Execute(null);

        Assert.Equal(["vela", "cato", "ilsa", "brom"], _newGame.Leaders.Select(leader => leader.Id));
    }

    [Fact]
    public void AddLeader_NoneChosen_AddsItLast()
    {
        _newGame.SelectedLeader = null;
        _newGame.LeaderToAdd = _newGame.LeadersToAdd[0];

        _newGame.AddLeaderCommand.Execute(null);

        Assert.Equal("cato", _newGame.Leaders[^1].Id);
    }

    [Fact]
    public void AddLeader_Added_ChoosesItAndEmptiesTheAddBox()
    {
        _newGame.LeaderToAdd = _newGame.LeadersToAdd[0];

        _newGame.AddLeaderCommand.Execute(null);

        Assert.Equal(("cato", null), (_newGame.SelectedLeader?.Id, _newGame.LeaderToAdd));
    }

    [Fact]
    public void AddLeaderCommand_NothingPicked_IsGreyed()
    {
        Assert.False(_newGame.AddLeaderCommand.CanExecute(null));
    }

    [Fact]
    public void LeaderToAdd_Picked_RaisesAddCanExecuteChanged()
    {
        var raised = false;
        _newGame.AddLeaderCommand.CanExecuteChanged += (_, _) => raised = true;

        _newGame.LeaderToAdd = _newGame.LeadersToAdd[0];

        Assert.True(raised);
    }

    [Theory]
    [InlineData(nameof(NewGameViewModel.RemoveLeaderCommand))]
    [InlineData(nameof(NewGameViewModel.MoveLeaderUpCommand))]
    [InlineData(nameof(NewGameViewModel.MoveLeaderDownCommand))]
    public void SelectedLeader_Changed_RaisesCanExecuteChanged(string command)
    {
        var raised = false;
        CommandNamed(command).CanExecuteChanged += (_, _) => raised = true;

        _newGame.SelectedLeader = _newGame.Leaders[1];

        Assert.True(raised);
    }

    [Fact]
    public void RemoveLeader_Chosen_ChoosesTheOneTakingItsPlace()
    {
        _newGame.RemoveLeaderCommand.Execute(null);

        Assert.Equal(["ilsa", "brom"], _newGame.Leaders.Select(leader => leader.Id));
        Assert.Equal("ilsa", _newGame.SelectedLeader?.Id);
    }

    // Not greyed at the last one: Data's line says why the new game needs one (decision 968).
    [Fact]
    public void RemoveLeader_LastOne_ShowsDataMessage()
    {
        for (var removed = 0; removed < 3; removed++)
        {
            _newGame.RemoveLeaderCommand.Execute(null);
        }

        Assert.Contains("leaderChoices must list at least one character.", _newGame.Problem);
    }

    [Fact]
    public void RemoveLeaderCommand_NoneChosen_IsGreyed()
    {
        _newGame.SelectedLeader = null;

        Assert.False(_newGame.RemoveLeaderCommand.CanExecute(null));
    }

    [Fact]
    public void MoveLeaderUpCommand_FirstChosen_IsGreyed()
    {
        Assert.False(_newGame.MoveLeaderUpCommand.CanExecute(null));
    }

    [Fact]
    public void MoveLeaderDownCommand_LastChosen_IsGreyed()
    {
        _newGame.SelectedLeader = _newGame.Leaders[2];

        Assert.False(_newGame.MoveLeaderDownCommand.CanExecute(null));
    }

    // A move changes where the chosen row stands: the first, moved down, can now move up.
    [Fact]
    public void MoveLeaderDown_FirstChosen_RaisesUpCanExecuteChanged()
    {
        var raised = false;
        _newGame.MoveLeaderUpCommand.CanExecuteChanged += (_, _) => raised = true;

        _newGame.MoveLeaderDownCommand.Execute(null);

        Assert.True(raised && _newGame.MoveLeaderUpCommand.CanExecute(null));
    }

    [Fact]
    public void MoveLeaderUp_LastChosen_RaisesDownCanExecuteChanged()
    {
        _newGame.SelectedLeader = _newGame.Leaders[2];
        var raised = false;
        _newGame.MoveLeaderDownCommand.CanExecuteChanged += (_, _) => raised = true;

        _newGame.MoveLeaderUpCommand.Execute(null);

        Assert.True(raised && _newGame.MoveLeaderDownCommand.CanExecute(null));
    }

    [Theory]
    [InlineData(nameof(NewGameViewModel.Problem))]
    [InlineData(nameof(NewGameViewModel.LeadersToAdd))]
    public void RemoveLeader_Chosen_RaisesTheListsNoticesOnce(string notice)
    {
        var raised = 0;
        _newGame.PropertyChanged += (_, e) => raised += e.PropertyName == notice ? 1 : 0;

        _newGame.RemoveLeaderCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    private IRelayCommand CommandNamed(string command) => command switch
    {
        nameof(NewGameViewModel.RemoveLeaderCommand) => _newGame.RemoveLeaderCommand,
        nameof(NewGameViewModel.MoveLeaderUpCommand) => _newGame.MoveLeaderUpCommand,
        _ => _newGame.MoveLeaderDownCommand,
    };
}
