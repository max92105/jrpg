using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

// A title longer than its column ends in three dots and fits; a short one is left alone.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class TextEllipsisTests
{
    [Fact]
    public void Fit_LongTitle_EndsWithDotsAndFits()
    {
        var font = RepositoryScreenText.Text.Fonts.FontAt(UiTheme.MenuTextPixels);
        var title = "A very long placeholder quest title that no column holds whole";

        var fitted = TextEllipsis.Fit(font, title, 200);

        Assert.EndsWith("...", fitted, StringComparison.Ordinal);
        Assert.True(font.MeasureString(fitted).X <= 200);
        Assert.True(fitted.Length < title.Length);
        var oneMore = title[..(fitted.Length - 2)] + "...";
        Assert.True(font.MeasureString(oneMore).X > 200, "the longest start that fits was kept");
    }

    [Fact]
    public void Fit_ShortTitle_Unchanged()
    {
        var font = RepositoryScreenText.Text.Fonts.FontAt(UiTheme.MenuTextPixels);

        Assert.Equal("Errand", TextEllipsis.Fit(font, "Errand", 200));
    }
}
