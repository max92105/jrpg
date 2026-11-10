using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The entry banner's text, measured with the repository's fonts and words (decision 527).</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class MapBannerViewTests
{
    private readonly MapBanner _banner = new();
    private readonly MapBannerView _view;

    public MapBannerViewTests() => _view = new MapBannerView(RepositoryScreenText.Text, _banner);

    [Fact]
    public void Update_MapWithLevel_FormatsTheLevelLine()
    {
        _banner.Enter(Map("forest", 3));

        _view.Update();

        Assert.Equal("Lv 3", _view.LevelLine);
    }

    [Fact]
    public void Update_MapWithoutLevel_HasNoLevelLine()
    {
        _banner.Enter(Map("forest", 3));
        _view.Update();

        _banner.Enter(Map("town", null));
        _view.Update();

        Assert.Null(_view.LevelLine);
    }

    private static MapDefinition Map(string id, int? level) => new(
        new MapLayout { Id = id, Width = 1, Height = 1, Layers = [], Objects = [] },
        new MapSettings { Id = id, DisplayName = id, AreaLevel = level });
}
