using Jrpg.Core.Quests;
using Jrpg.Data.Content;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Menus;

// The detail column's arithmetic (L3) with the repository's real fonts and quest texts: the parts fit above the footer
// with room for three lines of description and three of stage text, and the texts are wrapped once per detail or width,
// not per frame.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class QuestDetailViewTests
{
    private static readonly QuestDefinition _errand = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")).Quests["placeholder_errand"];
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void Show_PlaceholderErrandTexts_FitTheColumnAboveTheFooter()
    {
        var view = new QuestDetailView(RepositoryScreenText.Text);
        var column = QuestDetailView.ColumnIn(MenuLayout.PageArea(NavTabShape.Width), QuestListView.ColumnWidth);
        var detail = QuestDetail.Of(new QuestEntry(_errand, _errand.LastStage), QuestCategory.Active, _messages);

        var height = view.Show(detail, column.Width);

        Assert.True(column.Width > 0, $"the column is {column.Width} wide");
        Assert.True(height <= MenuLayout.BodyHeight, $"the parts take {height} of {MenuLayout.BodyHeight}");
    }

    [Fact]
    public void FixedHeight_WithThreeLinesOfEachText_FitsAboveTheFooter()
    {
        var view = new QuestDetailView(RepositoryScreenText.Text);
        var lineHeight = RepositoryScreenText.Text.Fonts.FontAt(QuestDetailView.TextPixels).LineHeight;

        var height = view.FixedHeight + (6 * lineHeight);

        Assert.True(height <= MenuLayout.BodyHeight, $"six text lines take the parts to {height} of {MenuLayout.BodyHeight}");
    }

    [Fact]
    public void Show_DetailOrWidthChanged_WrapsOncePerChange()
    {
        var view = new QuestDetailView(RepositoryScreenText.Text);
        var first = QuestDetail.Of(new QuestEntry(_errand, _errand.FirstStage), QuestCategory.Active, _messages);
        var second = QuestDetail.Of(new QuestEntry(_errand, _errand.LastStage), QuestCategory.Active, _messages);

        view.Show(first, 400);
        view.Show(first, 400);
        view.Show(second, 400);
        view.Show(second, 500);
        view.Show(null, 500);

        Assert.Equal(3, view.WrapCount);
    }

    [Fact]
    public void ColumnIn_PageArea_StartsAfterTheListAndItsGap()
    {
        var area = new Rectangle(300, 128, 1500, 848);

        var column = QuestDetailView.ColumnIn(area, QuestListView.ColumnWidth);

        Assert.Equal(300 + QuestListView.ColumnWidth + QuestDetailView.Gap, column.X);
        Assert.Equal(area.Right - QuestDetailView.Gap, column.Right);
    }
}
