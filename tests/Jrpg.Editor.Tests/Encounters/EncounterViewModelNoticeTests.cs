using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// What an encounter tells the page and the list as it is edited: its label, the notices the buttons and the problem
// line read, and the Changed event the list's problem line listens to.
public sealed class EncounterViewModelNoticeTests
{
    [Fact]
    public void Label_SavedEncounter_NamesItsEnemiesInSlotOrder()
    {
        var encounter = EncounterViewModel.From(GoblinPair with { EnemyIds = ["orc", "goblin"] }, Choices);

        Assert.Equal("goblin_pair: Orc, Goblin", encounter.Label);
    }

    [Fact]
    public void Label_NewWithoutId_SaysNew()
    {
        var encounter = EncounterViewModel.New(Choices);

        Assert.Equal("(new encounter)", encounter.Label);
    }

    [Fact]
    public void Label_EnemyChoice_NamesTheEnemyAndItsId()
    {
        Assert.Equal("Goblin (goblin)", Goblin.Label);
    }

    [Fact]
    public void PropertyChanged_SlotRemoved_RaisesLabelAndProblem()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var raised = new List<string?>();
        encounter.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        encounter.RemoveEnemyCommand.Execute(null);

        Assert.Contains(nameof(EncounterViewModel.Label), raised);
        Assert.Contains(nameof(EncounterViewModel.Problem), raised);
        Assert.Equal("goblin_pair: Goblin", encounter.Label);
    }

    [Fact]
    public void PropertyChanged_IdTyped_RaisesLabelAndProblem()
    {
        var encounter = EncounterViewModel.New(Choices);
        var raised = new List<string?>();
        encounter.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        encounter.Id = "orc_patrol";

        Assert.Contains(nameof(EncounterViewModel.Label), raised);
        Assert.Contains(nameof(EncounterViewModel.Problem), raised);
    }

    [Fact]
    public void CanExecuteChanged_EnemyPicked_LightsAddEnemy()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var raised = 0;
        encounter.AddEnemyCommand.CanExecuteChanged += (_, _) => raised++;

        encounter.EnemyToAdd = Orc;

        Assert.Equal(1, raised);
        Assert.True(encounter.AddEnemyCommand.CanExecute(null));
    }

    [Fact]
    public void CanExecuteChanged_SlotCleared_GreysRemoveAndMoves()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var raised = new List<string>();
        encounter.RemoveEnemyCommand.CanExecuteChanged += (_, _) => raised.Add("remove");
        encounter.MoveEnemyUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        encounter.MoveEnemyDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        encounter.SelectedSlot = null;

        Assert.Equal(["remove", "up", "down"], raised);
    }

    [Fact]
    public void Changed_IdTyped_IsRaisedOnce()
    {
        var encounter = EncounterViewModel.New(Choices);
        var raised = 0;
        encounter.Changed += (_, _) => raised++;

        encounter.Id = "orc_patrol";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_SlotAdded_IsRaisedOnce()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        encounter.EnemyToAdd = Orc;
        var raised = 0;
        encounter.Changed += (_, _) => raised++;

        encounter.AddEnemyCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_SlotMoved_IsRaisedOnce()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var raised = 0;
        encounter.Changed += (_, _) => raised++;

        encounter.MoveEnemyDownCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    // A slot's choice and the add box's pick change no record: the list must not re-check for them.
    [Fact]
    public void Changed_SlotChosenOrEnemyPicked_IsNotRaised()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var raised = 0;
        encounter.Changed += (_, _) => raised++;

        encounter.SelectedSlot = encounter.Enemies[1];
        encounter.EnemyToAdd = Orc;

        Assert.Equal(0, raised);
    }
}
