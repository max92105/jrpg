using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The entry banner's text and band, measured with the repository's fonts and words (decision 527).</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class MapBannerViewTests
{
    private readonly MapBanner _banner = new();
    private readonly MapBannerView _view;

    public MapBannerViewTests() => _view = new MapBannerView(RepositoryScreenText.Text, new UiTextures(), _banner);

    [Fact]
    public void Update_MapWithLevel_MeasuresTheLevelNumber()
    {
        _banner.Enter(Map("forest", 3));

        _view.Update();

        Assert.Equal("3", _view.LevelNumber);
        Assert.Equal("forest", _view.NameLine);
    }

    [Fact]
    public void Update_MapWithoutLevel_HasNoLevelNumber()
    {
        _banner.Enter(Map("forest", 3));
        _view.Update();

        _banner.Enter(Map("town", null));
        _view.Update();

        Assert.Null(_view.LevelNumber);
    }

    // The band and the hairline follow the wider line: the name here, far wider than "LV 3".
    [Fact]
    public void Update_LongName_TextWidthIsTheNames()
    {
        _banner.Enter(Map("a", 3));
        _view.Update();
        var narrow = _view.TextWidth;

        _banner.Enter(Map("Primeval Growth Crossroad", 3));
        _view.Update();

        Assert.True(_view.TextWidth > narrow);
        var nameWidth = RepositoryScreenText.Text.Fonts.TitleFontAt(UiTheme.MapBannerTitlePixels).MeasureString("Primeval Growth Crossroad").X;
        Assert.InRange(_view.TextWidth, nameWidth, nameWidth + 1);
    }

    [Fact]
    public void Update_NothingShown_HasNoText()
    {
        _banner.Enter(Map("a", 3));
        _view.Update();

        _banner.Enter(Map("intro", 3, showsBanner: false));
        _view.Update();

        Assert.Equal(0, _view.TextWidth);
        Assert.Null(_view.NameLine);
    }

    private static MapDefinition Map(string id, int? level, bool showsBanner = true) => new(
        new MapLayout { Id = id, Width = 1, Height = 1, Layers = [], Objects = [] },
        new MapSettings { Id = id, DisplayName = id, AreaLevel = level, ShowsBanner = showsBanner });
}
