using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Items;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.NewGame;
using static Jrpg.Editor.Tests.NewGame.NewGameFixtures;

namespace Jrpg.Editor.Tests.NewGame;

// The starting items as the New game page types them (decision 968): rows of an item and a quantity, added after the
// chosen row from the items not listed, removed and moved, each change reaching the problem line once.
public sealed class NewGameItemListTests
{
    private const int NoneChosen = -1;

    private readonly NewGameViewModel _newGame = new(World);

    [Fact]
    public void Constructor_NewGame_ShowsItemsWithQuantities()
    {
        Assert.Equal([("tonic", "5"), ("ember", "1")], _newGame.StartingItems.Select(row => (row.Item.Id, row.QuantityText)));
    }

    [Fact]
    public void Constructor_ItemTheDraftLacks_ShowsItMissing()
    {
        var newGame = new NewGameViewModel(
            World with { NewGame = TestNewGame with { StartingItems = [new InventoryEntry("ghost", 1)] } });

        Assert.Equal(RecordChoice.Missing("ghost"), Assert.Single(newGame.StartingItems).Item);
    }

    [Fact]
    public void ToNewGame_RowsMatchingTheRecord_KeepsItsList()
    {
        Assert.Same(TestNewGame.StartingItems, _newGame.ToNewGame().StartingItems);
    }

    [Fact]
    public void ToNewGame_TypedQuantity_SetsIt()
    {
        _newGame.StartingItems[1].QuantityText = "2";

        Assert.Equal([new InventoryEntry("tonic", 5), new InventoryEntry("ember", 2)], _newGame.ToNewGame().StartingItems);
    }

    [Fact]
    public void ToNewGame_ItemMovedDown_WritesTheNewOrder()
    {
        _newGame.MoveItemDownCommand.Execute(null);

        Assert.Equal(["ember", "tonic"], _newGame.ToNewGame().StartingItems.Select(entry => entry.ItemId));
    }

    // The items stand above the gold on the page, so their box comes first on the line.
    [Fact]
    public void Problem_QuantityNotANumber_NamesTheItem()
    {
        (_newGame.StartingItems[1].QuantityText, _newGame.GoldText) = ("x", "lots");

        Assert.Equal("Quantity of Ember must be a whole number (was 'x').", _newGame.Problem);
    }

    [Fact]
    public void Problem_TwoQuantitiesNotNumbers_NamesTheFirstRowsItem()
    {
        (_newGame.StartingItems[0].QuantityText, _newGame.StartingItems[1].QuantityText) = ("y", "x");

        Assert.Equal("Quantity of Tonic must be a whole number (was 'y').", _newGame.Problem);
    }

    [Fact]
    public void Problem_QuantityPastMaxStack_ShowsDataMessage()
    {
        _newGame.StartingItems[1].QuantityText = "4";

        Assert.Equal("new-game.json: startingItems 'ember': quantity must be within 1..3 (was 4).", _newGame.Problem);
    }

    [Fact]
    public void Problem_QuantityAtMaxStack_IsNull()
    {
        _newGame.StartingItems[1].QuantityText = "3";

        Assert.Null(_newGame.Problem);
    }

    // Yarrow's id, acorn, sorts first, and the draft holds it before Salve; its name sorts last.
    [Fact]
    public void ItemsToAdd_LeavesListedItemsOut()
    {
        Assert.Equal(["salve", "acorn"], _newGame.ItemsToAdd.Select(item => item.Id));
    }

    [Fact]
    public void AddItem_WithSelection_InsertsAfterIt()
    {
        _newGame.ItemToAdd = _newGame.ItemsToAdd[0];

        _newGame.AddItemCommand.Execute(null);

        Assert.Equal(["tonic", "salve", "ember"], _newGame.StartingItems.Select(row => row.Item.Id));
    }

    [Fact]
    public void AddItem_Added_ChoosesOneCopyAndEmptiesTheAddBox()
    {
        _newGame.ItemToAdd = _newGame.ItemsToAdd[0];

        _newGame.AddItemCommand.Execute(null);

        var added = _newGame.SelectedItem;
        Assert.Equal(("salve", "1", null), (added?.Item.Id, added?.QuantityText, _newGame.ItemToAdd));
    }

    [Fact]
    public void RemoveItem_Chosen_ChoosesTheOneTakingItsPlace()
    {
        _newGame.RemoveItemCommand.Execute(null);

        Assert.Equal(("ember", "ember"), (Assert.Single(_newGame.StartingItems).Item.Id, _newGame.SelectedItem?.Item.Id));
    }

    [Theory]
    [InlineData(nameof(NewGameViewModel.AddItemCommand), 0)]
    [InlineData(nameof(NewGameViewModel.RemoveItemCommand), NoneChosen)]
    [InlineData(nameof(NewGameViewModel.MoveItemUpCommand), 0)]
    [InlineData(nameof(NewGameViewModel.MoveItemUpCommand), NoneChosen)]
    [InlineData(nameof(NewGameViewModel.MoveItemDownCommand), 1)]
    [InlineData(nameof(NewGameViewModel.MoveItemDownCommand), NoneChosen)]
    public void ItemCommand_CantAct_IsGreyed(string command, int chosen)
    {
        _newGame.SelectedItem = chosen == NoneChosen ? null : _newGame.StartingItems[chosen];

        Assert.False(CommandNamed(command).CanExecute(null));
    }

    [Theory]
    [InlineData(nameof(NewGameViewModel.RemoveItemCommand))]
    [InlineData(nameof(NewGameViewModel.MoveItemUpCommand))]
    [InlineData(nameof(NewGameViewModel.MoveItemDownCommand))]
    public void SelectedItem_Changed_RaisesCanExecuteChanged(string command)
    {
        var raised = false;
        CommandNamed(command).CanExecuteChanged += (_, _) => raised = true;

        _newGame.SelectedItem = _newGame.StartingItems[1];

        Assert.True(raised);
    }

    [Fact]
    public void ItemToAdd_Picked_RaisesAddCanExecuteChanged()
    {
        var raised = false;
        _newGame.AddItemCommand.CanExecuteChanged += (_, _) => raised = true;

        _newGame.ItemToAdd = _newGame.ItemsToAdd[0];

        Assert.True(raised);
    }

    // A move changes where the chosen row stands: the first, moved down, can now move up.
    [Fact]
    public void MoveItemDown_FirstChosen_RaisesUpCanExecuteChanged()
    {
        var raised = false;
        _newGame.MoveItemUpCommand.CanExecuteChanged += (_, _) => raised = true;

        _newGame.MoveItemDownCommand.Execute(null);

        Assert.True(raised && _newGame.MoveItemUpCommand.CanExecute(null));
    }

    [Fact]
    public void MoveItemUp_LastChosen_RaisesDownCanExecuteChanged()
    {
        _newGame.SelectedItem = _newGame.StartingItems[1];
        var raised = false;
        _newGame.MoveItemDownCommand.CanExecuteChanged += (_, _) => raised = true;

        _newGame.MoveItemUpCommand.Execute(null);

        Assert.True(raised && _newGame.MoveItemDownCommand.CanExecute(null));
    }

    [Theory]
    [InlineData(nameof(NewGameViewModel.Problem))]
    [InlineData(nameof(NewGameViewModel.ItemsToAdd))]
    public void RemoveItem_Chosen_RaisesTheListsNoticesOnce(string notice)
    {
        var raised = 0;
        _newGame.PropertyChanged += (_, e) => raised += e.PropertyName == notice ? 1 : 0;

        _newGame.RemoveItemCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    // Once: the row's own problem notice, raised beside its quantity's, isn't relayed.
    [Fact]
    public void Problem_QuantityTyped_RaisesProblemChangedOnce()
    {
        var raised = 0;
        _newGame.PropertyChanged += (_, e) => raised += e.PropertyName == nameof(NewGameViewModel.Problem) ? 1 : 0;

        _newGame.StartingItems[1].QuantityText = "2";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Problem_AddedRowTyped_RaisesProblemChanged()
    {
        _newGame.ItemToAdd = _newGame.ItemsToAdd[0];
        _newGame.AddItemCommand.Execute(null);
        var raised = false;
        _newGame.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(NewGameViewModel.Problem);

        _newGame.SelectedItem!.QuantityText = "2";

        Assert.True(raised);
    }

    [Fact]
    public void Problem_RemovedRowTyped_RaisesNothing()
    {
        var removed = _newGame.StartingItems[0];
        _newGame.RemoveItemCommand.Execute(null);
        var raised = false;
        _newGame.PropertyChanged += (_, _) => raised = true;

        removed.QuantityText = "2";

        Assert.False(raised);
    }

    private IRelayCommand CommandNamed(string command) => command switch
    {
        nameof(NewGameViewModel.AddItemCommand) => _newGame.AddItemCommand,
        nameof(NewGameViewModel.RemoveItemCommand) => _newGame.RemoveItemCommand,
        nameof(NewGameViewModel.MoveItemUpCommand) => _newGame.MoveItemUpCommand,
        _ => _newGame.MoveItemDownCommand,
    };
}
