using Jrpg.Core.Maps;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class SheetChoicesTests
{
    private static readonly TilesheetDefinition _forest = new() { Id = "forest", File = "Forest.png", TileWidth = 48, TileHeight = 48, Margin = 0, Spacing = 0 };
    private static readonly TilesheetDefinition _city = new() { Id = "city", File = "City.png", TileWidth = 48, TileHeight = 48, Margin = 0, Spacing = 0 };

    [Fact]
    public void Of_SheetWhoseImageIsMissing_ListsItLastAsMissing()
    {
        var choices = SheetChoices.Of(["Forest.png"], [_city, _forest]);

        Assert.Equal(["forest", "city (image missing)"], choices.Select(choice => choice.Label));
        Assert.True(choices[1].IsImageMissing);
        Assert.Equal(_city, choices[1].Sheet);
    }

    [Fact]
    public void Of_ImageWithoutSheet_ListsItNotSetUp()
    {
        var choices = SheetChoices.Of(["Forest.png", "Grass3.png"], [_forest]);

        Assert.Equal(["forest", "Grass3.png (not set up)"], choices.Select(choice => choice.Label));
        Assert.False(choices[1].IsImageMissing);
    }

    // Windows finds the file a sheet names whatever its case, so the sheet isn't missing.
    [Fact]
    public void Of_ImageNamedInOtherCase_IsTheSheetsImage()
    {
        var choices = SheetChoices.Of(["forest.PNG"], [_forest]);

        Assert.Equal(["forest"], choices.Select(choice => choice.Label));
    }
}
