using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// One encounter as the Encounters page edits it (decision 905): its enemies in slot order, the same enemy any number of
// times, a slot moved or removed by its own row, and the problem the typed id's and a missing enemy's.
public sealed class EncounterViewModelTests
{
    [Fact]
    public void AddEnemy_SlotChosen_InsertsAfterItAndChoosesIt()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        encounter.EnemyToAdd = Orc;

        encounter.AddEnemyCommand.Execute(null);

        Assert.Equal(["goblin", "orc", "goblin"], encounter.Enemies.Select(slot => slot.EnemyId));
        Assert.Same(encounter.Enemies[1], encounter.SelectedSlot);
    }

    [Fact]
    public void AddEnemy_NoSlotChosen_AddsItLast()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        encounter.SelectedSlot = null;
        encounter.EnemyToAdd = Orc;

        encounter.AddEnemyCommand.Execute(null);

        Assert.Equal(["goblin", "goblin", "orc"], encounter.Enemies.Select(slot => slot.EnemyId));
    }

    [Fact]
    public void AddEnemy_SameEnemyTwice_AddsTwoRows()
    {
        var encounter = EncounterViewModel.New(Choices);
        encounter.EnemyToAdd = Orc;

        encounter.AddEnemyCommand.Execute(null);
        encounter.AddEnemyCommand.Execute(null);

        Assert.Equal(["orc", "orc"], encounter.Enemies.Select(slot => slot.EnemyId));
        Assert.NotSame(encounter.Enemies[0], encounter.Enemies[1]);
    }

    [Fact]
    public void AddEnemy_NothingToAdd_CannotExecute()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);

        Assert.False(encounter.AddEnemyCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEnemy_NoSlotChosen_CannotExecuteNorMove()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);

        encounter.SelectedSlot = null;

        Assert.False(encounter.RemoveEnemyCommand.CanExecute(null));
        Assert.False(encounter.MoveEnemyUpCommand.CanExecute(null));
        Assert.False(encounter.MoveEnemyDownCommand.CanExecute(null));
    }

    // Two goblins are equal by their fields: the chosen row object must move, not the first goblin found.
    [Fact]
    public void MoveEnemyDown_FirstOfTwoGoblins_MovesThatRow()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var first = encounter.Enemies[0];

        encounter.MoveEnemyDownCommand.Execute(null);

        Assert.Same(first, encounter.Enemies[1]);
        Assert.Same(first, encounter.SelectedSlot);
    }

    [Fact]
    public void MoveEnemyUp_SecondGoblin_MovesThatRow()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var second = encounter.Enemies[1];
        encounter.SelectedSlot = second;

        encounter.MoveEnemyUpCommand.Execute(null);

        Assert.Same(second, encounter.Enemies[0]);
    }

    [Fact]
    public void RemoveEnemy_SecondGoblin_RemovesThatRow()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var first = encounter.Enemies[0];
        encounter.SelectedSlot = encounter.Enemies[1];

        encounter.RemoveEnemyCommand.Execute(null);

        Assert.Equal([first], encounter.Enemies);
        Assert.Same(first, encounter.SelectedSlot);
    }

    [Fact]
    public void RemoveEnemy_LastSlot_ProblemNamesTheMissingEnemy()
    {
        var encounter = EncounterViewModel.From(GoblinPair with { EnemyIds = ["orc"] }, Choices);

        encounter.RemoveEnemyCommand.Execute(null);

        Assert.Null(encounter.SelectedSlot);
        Assert.Equal("An encounter needs at least one enemy.", encounter.Problem);
    }

    [Fact]
    public void Problem_NewWithBadId_NamesTheId()
    {
        var encounter = EncounterViewModel.New(Choices);

        encounter.Id = "Goblin Pair";

        Assert.Equal("The id may only use lowercase letters, digits and _ (like tree_4).", encounter.Problem);
    }

    [Fact]
    public void Problem_NewWithIdAndEnemy_IsNull()
    {
        var encounter = EncounterViewModel.New(Choices);
        encounter.Id = "orc_patrol";
        encounter.EnemyToAdd = Orc;

        encounter.AddEnemyCommand.Execute(null);

        Assert.Null(encounter.Problem);
    }

    // A saved id is fixed, whatever it looks like: the hand-written content's ids were never checked by this rule.
    [Fact]
    public void Problem_SavedEncounter_NoIdProblem()
    {
        var encounter = EncounterViewModel.From(GoblinPair with { Id = "Goblin-Pair" }, Choices);

        Assert.Null(encounter.Problem);
    }

    [Fact]
    public void ToEncounter_EnemiesInSlotOrder_KeepsTheOrder()
    {
        var encounter = EncounterViewModel.From(GoblinPair with { EnemyIds = ["orc", "goblin"] }, Choices);

        encounter.MoveEnemyDownCommand.Execute(null);

        Assert.Equal(["goblin", "orc"], encounter.ToEncounter().EnemyIds);
    }

    [Fact]
    public void ToEncounter_NewEncounterTyped_WritesTheTypedId()
    {
        var encounter = EncounterViewModel.New(Choices);
        encounter.Id = "orc_patrol";
        encounter.EnemyToAdd = Orc;
        encounter.AddEnemyCommand.Execute(null);

        var written = encounter.ToEncounter();

        Assert.Equal("orc_patrol", written.Id);
        Assert.Equal(["orc"], written.EnemyIds);
    }

    // The boxes come in EC2b: until then the record's own background and music are carried over.
    [Fact]
    public void ToEncounter_OwnBackgroundAndMusic_Kept()
    {
        var arena = GoblinPair with { BackgroundFile = "Cave.png", MusicFile = "boss/loop.wav" };

        var written = EncounterViewModel.From(arena, Choices).ToEncounter();

        Assert.Equal(("Cave.png", "boss/loop.wav"), (written.BackgroundFile, written.MusicFile));
    }

    [Fact]
    public void ToEncounter_NewWithoutId_Throws()
    {
        var encounter = EncounterViewModel.New(Choices);

        Assert.Throws<InvalidOperationException>(encounter.ToEncounter);
    }
}
