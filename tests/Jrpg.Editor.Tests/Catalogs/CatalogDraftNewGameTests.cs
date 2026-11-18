using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Editor.Catalogs;
using static Jrpg.Editor.Tests.NewGame.NewGameFixtures;

namespace Jrpg.Editor.Tests.Catalogs;

// The new game is replaced whole as one undo step (decision 968), compared by content, its lists included, so a page
// applied unchanged records nothing.
public sealed class CatalogDraftNewGameTests
{
    private readonly CatalogDraft _draft = new(World);

    [Fact]
    public void SetNewGame_ChangedGold_RecordsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _draft.SetNewGame(TestNewGame with { StartingGold = 900 });

        Assert.Equal((1, 900), (steps, _draft.Current.NewGame.StartingGold));
    }

    // The lists rebuilt, as the page's rows will build them: equal by content, not by reference.
    [Fact]
    public void SetNewGame_SameNewGame_RecordsNothing()
    {
        _draft.SetNewGame(
            TestNewGame with
            {
                LeaderChoices = [.. TestNewGame.LeaderChoices.Select(choice => new LeaderChoice { MemberId = choice.MemberId })],
                StartingItems = [.. TestNewGame.StartingItems.Select(entry => new InventoryEntry(entry.ItemId, entry.Quantity))],
            });

        Assert.False(_draft.CanUndo);
    }

    // A leader moved is a change: the leader screen's order.
    [Fact]
    public void SetNewGame_LeadersReordered_RecordsOneStep()
    {
        _draft.SetNewGame(TestNewGame with { LeaderChoices = [.. TestNewGame.LeaderChoices.Reverse()] });

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetNewGame_ChangedItemQuantity_RecordsOneStep()
    {
        var items = new[] { new InventoryEntry("tonic", 4), new InventoryEntry("ember", 1) };

        _draft.SetNewGame(TestNewGame with { StartingItems = items });

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetNewGame_Undo_RestoresIt()
    {
        _draft.SetNewGame(TestNewGame with { StartingGold = 900 });

        _draft.Undo();

        Assert.Same(TestNewGame, _draft.Current.NewGame);
    }
}
