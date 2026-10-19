using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Flow;

/// <summary>
/// Builds the scenes from what they share, so the transitions that swap scenes don't each carry every scene's
/// dependencies.
/// </summary>
internal sealed class SceneFactory
{
    private readonly ContentCatalog _catalog;
    private readonly ScreenText _text;
    private readonly MapArt _art;

    /// <param name="catalog">The content as played, without random battles.</param>
    /// <param name="text">Messages and fonts for everything a scene writes.</param>
    public SceneFactory(ContentCatalog catalog, ScreenText text)
    {
        _catalog = catalog;
        _text = text;
        _art = new MapArt(catalog, MapView.CellPixels);
    }

    public ExplorationScene CreateExploration(ExplorationState exploration) => new(exploration, _catalog, _art, _text);
}
