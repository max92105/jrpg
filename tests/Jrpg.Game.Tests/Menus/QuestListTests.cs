using Jrpg.Core.Quests;
using Jrpg.Game.Menus;

namespace Jrpg.Game.Tests.Menus;

// The listed quests' cursor: Up and Down move it, a new showing starts on the first, no quest means no cursor. The
// titles are fitted by the function given; the tests keep them whole.
public sealed class QuestListTests
{
    private static readonly QuestDefinition _errand = new("errand", "An errand", "Carry a letter.", QuestKind.Side, [new QuestStage("take", "Take the letter.")]);
    private static readonly QuestEntry _errandEntry = new(_errand, _errand.FirstStage);
    private static readonly QuestEntry _heistEntry = new(_errand with { Id = "heist", Title = "A heist" }, _errand.FirstStage);

    [Fact]
    public void MoveDown_TwoQuests_SelectsTheSecond()
    {
        var list = new QuestList(title => title);
        list.Show([_errandEntry, _heistEntry], []);

        list.MoveDown();

        Assert.Same(_heistEntry, list.Selected);
    }

    [Fact]
    public void Show_Again_BackOnTheFirst()
    {
        var list = new QuestList(title => title);
        list.Show([_errandEntry, _heistEntry], []);
        list.MoveDown();

        list.Show([_errandEntry, _heistEntry], []);

        Assert.Same(_errandEntry, list.Selected);
    }

    [Fact]
    public void ShowNext_FromActive_ShowsCompleted()
    {
        var list = new QuestList(title => title);
        list.Show([_errandEntry], [_heistEntry]);

        list.ShowNext();

        Assert.Equal(QuestCategory.Completed, list.Category);
        Assert.Same(_heistEntry, list.Selected);
    }

    [Fact]
    public void ShowNext_OnCompleted_StaysOnCompleted()
    {
        var list = new QuestList(title => title);
        list.Show([_errandEntry], [_heistEntry]);
        list.ShowNext();

        list.ShowNext();

        Assert.Equal(QuestCategory.Completed, list.Category);
    }

    [Fact]
    public void ShowPrevious_BackOnActive_KeepsItsCursor()
    {
        var list = new QuestList(title => title);
        list.Show([_errandEntry, _heistEntry], [_heistEntry]);
        list.MoveDown();
        list.ShowNext();

        list.ShowPrevious();

        Assert.Equal(QuestCategory.Active, list.Category);
        Assert.Same(_heistEntry, list.Selected);
        list.ShowPrevious();
        Assert.Equal(QuestCategory.Active, list.Category);
    }

    [Fact]
    public void Show_NothingCompleted_CompletedHasNoCursor()
    {
        var list = new QuestList(title => title);
        list.Show([_errandEntry], []);

        list.ShowNext();

        Assert.Null(list.Cursor);
        Assert.Null(list.Selected);
    }

    // The fitting function is the list's only way to shorten a title; the entry stays whole for the detail.
    [Fact]
    public void Show_LongTitle_RowHoldsTheFittedTitle()
    {
        var list = new QuestList(title => "fit:" + title);

        list.Show([_errandEntry], []);

        Assert.Equal("fit:An errand", list.Rows[0].Title);
        Assert.Same(_errandEntry, list.Selected);
    }

    [Fact]
    public void Show_NoQuest_HasNoCursor()
    {
        var list = new QuestList(title => title);
        list.Show([_errandEntry], []);

        list.Show([], []);

        Assert.Null(list.Cursor);
        Assert.Null(list.Selected);
        list.MoveDown();
        Assert.Null(list.Selected);
    }
}
